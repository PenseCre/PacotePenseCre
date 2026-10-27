using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public sealed class BuildZipRequest
    {
        public string SourceDirectory { get; private set; }
        public string DestinationPath { get; private set; }
        public string DisplayName { get; private set; }

        public BuildZipRequest(string sourceDirectory, string destinationPath, string displayName)
        {
            if (string.IsNullOrWhiteSpace(sourceDirectory) || string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("ZIP source and destination paths are required.");
            SourceDirectory = Path.GetFullPath(sourceDirectory);
            DestinationPath = Path.GetFullPath(destinationPath);
            DisplayName = displayName ?? Path.GetFileName(DestinationPath);
        }
    }

    public enum BuildZipState { Preparing, Archiving, Finalizing, Cancelling, Completed, Cancelled, Failed }

    public sealed class BuildZipStatus
    {
        public BuildZipState State { get; internal set; }
        public string ArchiveName { get; internal set; }
        public string CurrentFile { get; internal set; }
        public int ArchiveIndex { get; internal set; }
        public int ArchiveCount { get; internal set; }
        public int CompletedArchives { get; internal set; }
        public long FilesCopied { get; internal set; }
        public long BytesCopied { get; internal set; }
        public string Error { get; internal set; }
        public string PartialPath { get; internal set; }
        public string CleanupFailurePath { get; internal set; }
        internal BuildZipStatus Copy() { return (BuildZipStatus)MemberwiseClone(); }
    }

    /// <summary>Editor-owned ZIP lifetime; all public methods are called on the main thread.</summary>
    [InitializeOnLoad]
    public static class BuildZipOperation
    {
        private static BuildZipWorker _worker;
        private static Task _task;
        private static BuildZipStatus _lastStatus;
        private static bool _reloadLocked;
        private static double _nextUpdate;
        private static string _unresolvedPartial;
        private static Action _pendingAction;
        private static UnityEngine.Object _pendingOwner;
        private static bool _foregroundClaimed;
        private static bool _compilationCancellationRequested;

        public static event Action Changed;
        public static bool IsActive { get { return _task != null; } }
        public static bool HasForegroundRequest { get { return _foregroundClaimed; } }
        public static BuildZipStatus Status { get { return _worker != null ? _worker.Snapshot() : _lastStatus; } }

        static BuildZipOperation()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            EditorApplication.wantsToQuit += OnWantsToQuit;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
        }

        public static void EnsureCanBuild()
        {
            if (IsActive) throw new InvalidOperationException("[BUILD ZIP] Zipping is still active. Cancel it and wait for cleanup before building.");
            if (!string.IsNullOrEmpty(_unresolvedPartial))
            {
                // File.GetAttributes distinguishes inaccessible output from a genuinely absent file.
                try { File.GetAttributes(_unresolvedPartial); }
                catch (FileNotFoundException) { _unresolvedPartial = null; return; }
                catch (DirectoryNotFoundException) { _unresolvedPartial = null; return; }
                throw new IOException("[BUILD ZIP] Resolve or remove the leftover partial ZIP before building: " + _unresolvedPartial);
            }
        }

        public static void Start(IList<BuildZipRequest> requests)
        {
            EnsureCanBuild();
            if (requests == null || requests.Count == 0) throw new ArgumentException("At least one ZIP request is required.");
            var copy = new BuildZipRequest[requests.Count];
            var destinations = new HashSet<string>(BuildZipWorker.PathComparer);
            for (int i = 0; i < copy.Length; i++)
            {
                var request = requests[i];
                if (request == null) throw new ArgumentException("ZIP requests cannot contain null.");
                if (!destinations.Add(request.DestinationPath)) throw new ArgumentException("Duplicate ZIP destination: " + request.DestinationPath);
                for (int j = 0; j < requests.Count; j++)
                    if (requests[j] != null && BuildZipWorker.IsWithin(request.DestinationPath, requests[j].SourceDirectory))
                        throw new ArgumentException("ZIP destination must be outside every source directory.");
                copy[i] = request;
            }
            _worker = new BuildZipWorker(copy);
            _compilationCancellationRequested = false;
            try
            {
                EditorApplication.LockReloadAssemblies();
                _reloadLocked = true;
                _task = Task.Run((Action)_worker.Run);
                EditorApplication.update += Update;
                _nextUpdate = 0;
            }
            catch
            {
                _worker.Dispose();
                _worker = null;
                _task = null;
                ReleaseReloadLock();
                throw;
            }
            NotifyChanged();
        }

        public static void Cancel()
        {
            if (_worker == null) return;
            _worker.Cancel();
            NotifyChanged();
        }

        /// <summary>Claim before showing a dialog or starting an editor coroutine.</summary>
        public static bool TryClaimForeground()
        {
            if (_foregroundClaimed || EditorApplication.isCompiling) return false;
            _foregroundClaimed = true;
            return true;
        }

        public static void ReleaseForeground()
        {
            _foregroundClaimed = false;
            NotifyChanged();
        }

        public static void AfterCancellation(UnityEngine.Object owner, Action action)
        {
            if (!_foregroundClaimed || _pendingAction != null) throw new InvalidOperationException("A foreground request is already pending or unclaimed.");
            _pendingOwner = owner;
            _pendingAction = action ?? throw new ArgumentNullException("action");
            if (IsActive) Cancel();
            else RunPendingAction();
        }

        public static void AbandonPending(UnityEngine.Object owner)
        {
            if (_pendingAction == null || _pendingOwner != owner) return;
            ClearPending();
            Debug.Log("[BUILD ZIP] Pending build/archive request discarded because its window closed.");
        }

        private static void Update()
        {
            if (_task == null) return;
            if (EditorApplication.isCompiling && !_compilationCancellationRequested)
                OnCompilationStarted(null);
            if (!_task.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup >= _nextUpdate)
                {
                    _nextUpdate = EditorApplication.timeSinceStartup + 0.2;
                    NotifyChanged();
                }
                return;
            }
            _lastStatus = _worker.Snapshot();
            if (_task.IsFaulted)
            {
                _lastStatus.State = BuildZipState.Failed;
                _lastStatus.Error = _task.Exception.GetBaseException().Message;
            }
            _unresolvedPartial = _lastStatus.CleanupFailurePath;
            _task = null;
            _worker.Dispose();
            _worker = null;
            EditorApplication.update -= Update;
            ReleaseReloadLock();
            if (_lastStatus.State == BuildZipState.Failed)
                Debug.LogError("[BUILD ZIP] " + _lastStatus.ArchiveName + ": " + _lastStatus.Error);
            else
                Debug.Log("[BUILD ZIP] " + _lastStatus.State + "; completed " + _lastStatus.CompletedArchives + "/" + _lastStatus.ArchiveCount + " archive(s).");
            NotifyChanged();
            // Unlock may schedule a reload. Never start a pending build while compilation is active.
            if (EditorApplication.isCompiling) ClearPending();
            else RunPendingAction();
        }

        private static void RunPendingAction()
        {
            var action = _pendingAction;
            var owner = _pendingOwner;
            _pendingAction = null;
            _pendingOwner = null;
            if (action == null) return;
            try
            {
                EnsureCanBuild();
                if (owner == null) { ReleaseForeground(); return; }
                action();
            }
            catch (Exception ex)
            {
                ReleaseForeground();
                Debug.LogError("[BUILD ZIP] New build/archive did not start: " + ex.Message);
            }
        }

        private static void ClearPending()
        {
            if (_pendingAction == null) return;
            _pendingAction = null;
            _pendingOwner = null;
            ReleaseForeground();
        }

        private static void OnCompilationStarted(object context)
        {
            _compilationCancellationRequested = true;
            ClearPending();
            Cancel();
        }

        private static bool OnWantsToQuit()
        {
            ClearPending();
            if (!IsActive) return true;
            Cancel();
            Debug.LogWarning("[BUILD ZIP] Finishing ZIP cancellation and partial-file cleanup. Please quit again once cleanup finishes; normal save prompts will be preserved.");
            return false;
        }

        private static void OnBeforeReload()
        {
            ClearPending();
            if (IsActive) Debug.LogError("[BUILD ZIP] Unexpected reload while a ZIP task owns the reload lock.");
        }

        private static void ReleaseReloadLock()
        {
            if (!_reloadLocked) return;
            _reloadLocked = false;
            EditorApplication.UnlockReloadAssemblies();
        }

        private static void NotifyChanged()
        {
            var handlers = Changed;
            if (handlers == null) return;
            foreach (Action handler in handlers.GetInvocationList())
                try { handler(); }
                catch (Exception ex) { Debug.LogException(ex); }
        }
    }
}
