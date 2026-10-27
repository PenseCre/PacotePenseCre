using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace PacotePenseCre.Editor.BuildPipeline
{
    // Pure BCL: never depends on an editor continuation or a Unity object.
    internal sealed class BuildZipWorker : IDisposable
    {
        internal static readonly StringComparer PathComparer = Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        private readonly object _gate = new object();
        private readonly BuildZipRequest[] _requests;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly BuildZipStatus _status;
        private bool _cancelRequested;
        private bool _finished;

        internal BuildZipWorker(BuildZipRequest[] requests)
        {
            _requests = requests;
            _status = new BuildZipStatus { State = BuildZipState.Preparing, ArchiveCount = requests.Length, ArchiveName = requests[0].DisplayName };
        }

        internal BuildZipStatus Snapshot() { lock (_gate) return _status.Copy(); }

        internal void Cancel()
        {
            lock (_gate)
            {
                if (_finished || _status.State == BuildZipState.Completed || _status.State == BuildZipState.Cancelled || _status.State == BuildZipState.Failed) return;
                _cancelRequested = true;
                _status.State = BuildZipState.Cancelling;
            }
            _cancellation.Cancel();
        }

        internal void Run()
        {
            try
            {
                var buffer = new byte[64 * 1024];
                for (int i = 0; i < _requests.Length; i++)
                {
                    _cancellation.Token.ThrowIfCancellationRequested();
                    lock (_gate)
                    {
                        _status.ArchiveIndex = i + 1;
                        _status.ArchiveName = _requests[i].DisplayName;
                        _status.CurrentFile = null;
                        _status.FilesCopied = 0;
                        _status.BytesCopied = 0;
                        if (!_cancelRequested) _status.State = BuildZipState.Preparing;
                    }
                    WriteArchive(_requests[i], buffer);
                }
                lock (_gate) _status.State = BuildZipState.Completed;
            }
            catch (OperationCanceledException)
            {
                lock (_gate) _status.State = BuildZipState.Cancelled;
            }
            catch (Exception ex)
            {
                lock (_gate)
                {
                    _status.State = BuildZipState.Failed;
                    _status.Error = ex.Message;
                }
            }
            finally { lock (_gate) _finished = true; }
        }

        private void WriteArchive(BuildZipRequest request, byte[] buffer)
        {
            string partial = request.DestinationPath + "." + Guid.NewGuid().ToString("N") + ".partial";
            bool owned = false;
            try
            {
                if (IsWithin(request.DestinationPath, request.SourceDirectory)) throw new IOException("ZIP destination is inside its source directory.");
                if (File.Exists(request.DestinationPath) || Directory.Exists(request.DestinationPath))
                    throw new IOException("Destination already exists; move or rename it before archiving: " + request.DestinationPath);
                RejectReparsePoint(request.SourceDirectory);
                using (var output = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None, buffer.Length))
                {
                    owned = true;
                    lock (_gate) _status.PartialPath = partial;
                    using (var archive = new ZipArchive(output, ZipArchiveMode.Create))
                    {
                        CopyDirectory(archive, request.SourceDirectory, request.SourceDirectory, buffer, 0);
                        lock (_gate) if (!_cancelRequested) _status.State = BuildZipState.Finalizing;
                    }
                }
                // Reserving publication is the linearization point. No disk I/O under the gate.
                lock (_gate)
                    if (_cancelRequested) throw new OperationCanceledException(_cancellation.Token);
                File.Move(partial, request.DestinationPath);
                owned = false;
                lock (_gate)
                {
                    _status.CompletedArchives++;
                    _status.PartialPath = null;
                }
            }
            finally
            {
                if (owned)
                {
                    try { File.Delete(partial); }
                    catch (Exception ex)
                    {
                        lock (_gate) _status.CleanupFailurePath = partial;
                        throw new IOException("Partial ZIP cleanup failed. Close programs using it, then remove this file before rebuilding: " + partial + ". " + ex.Message, ex);
                    }
                    lock (_gate) _status.PartialPath = null;
                }
            }
        }

        private void CopyDirectory(ZipArchive archive, string root, string directory, byte[] buffer, int depth)
        {
            _cancellation.Token.ThrowIfCancellationRequested();
            if (depth > 128) throw new IOException("ZIP source exceeds maximum directory depth of 128: " + directory);
            bool empty = true;
            foreach (string path in Directory.EnumerateFileSystemEntries(directory))
            {
                empty = false;
                _cancellation.Token.ThrowIfCancellationRequested();
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("ZIP source contains an unsupported symbolic link/reparse point: " + path);
                if ((attributes & FileAttributes.Directory) != 0)
                    CopyDirectory(archive, root, path, buffer, depth + 1);
                else
                    CopyFile(archive, root, path, buffer);
            }
            if (empty && depth > 0)
            {
                var entry = archive.CreateEntry(RelativeName(root, directory) + "/");
                entry.LastWriteTime = ZipTimestamp(Directory.GetLastWriteTime(directory));
            }
        }

        private void CopyFile(ZipArchive archive, string root, string path, byte[] buffer)
        {
            string relative = RelativeName(root, path);
            lock (_gate)
            {
                _status.CurrentFile = relative;
                if (!_cancelRequested) _status.State = BuildZipState.Archiving;
            }
            using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length))
            {
                var entry = archive.CreateEntry(relative, CompressionLevel.Optimal);
                entry.LastWriteTime = ZipTimestamp(File.GetLastWriteTime(path));
                using (var output = entry.Open())
                {
                    while (true)
                    {
                        _cancellation.Token.ThrowIfCancellationRequested();
                        int read = input.Read(buffer, 0, buffer.Length);
                        if (read == 0) break;
                        output.Write(buffer, 0, read);
                        lock (_gate) _status.BytesCopied += read;
                    }
                }
            }
            lock (_gate) _status.FilesCopied++;
        }

        private static string RelativeName(string root, string path)
        {
            if (!IsWithin(path, root)) throw new IOException("ZIP entry escapes the source directory: " + path);
            return path.Substring(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length + 1).Replace('\\', '/');
        }

        internal static bool IsWithin(string path, string root)
        {
            string prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return path.StartsWith(prefix, comparison) || PathComparer.Equals(path.TrimEnd(Path.DirectorySeparatorChar), root.TrimEnd(Path.DirectorySeparatorChar));
        }

        private static void RejectReparsePoint(string path)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new IOException("ZIP source is an unsupported reparse point: " + path);
        }

        private static DateTimeOffset ZipTimestamp(DateTime value)
        {
            return new DateTimeOffset(value.Year < 1980 || value.Year > 2107 ? new DateTime(1980, 1, 1) : value);
        }

        public void Dispose() { _cancellation.Dispose(); }
    }
}
