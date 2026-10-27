using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PacotePenseCre;
using PacotePenseCre.Utility;
using PacotePenseCre.BuildPipeline;
using PacotePenseCre.Editor.BuildPipeline;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using JsonUtility = PacotePenseCre.Utility.JsonUtility;
using System.Linq;

namespace PacotePenseCre.Editor
{
    class PacotePenseCreEditorWindow : EditorWindow
    {
        #region Custom Editor Window Code

        [MenuItem("File/Build with PenseCre")]
        public static void ShowWindow()
        {
            GetWindow<PacotePenseCreEditorWindow>("PenseCre Build Window");
        }

        #endregion

        #region Properties

        public List<SceneSelection> scenes
        {
            get
            {
                if (_buildConfig != null)
                {
                    if (_buildConfig.scenes == null)
                    {
                        _buildConfig.scenes = new List<SceneSelection>();
                    }
                    return _buildConfig.scenes;
                }
                return _fallbackScenes;
            }
            set
            {
                if (_buildConfig != null)
                {
                    _buildConfig.scenes = value;
                }
                _fallbackScenes = value;
            }
        }
        private List<SceneSelection> _fallbackScenes = new List<SceneSelection>();

        public BuildInfo _buildInfo;
        public BuildConfig _buildConfig;

        private string[] _sceneOptions;
        private bool populatedOptions;

        private bool _busy;
        private bool _buildingScene;
        private bool _commandClicked;
        private bool _commandWaiting;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            BuildZipOperation.Changed += OnZipChanged;
        }

        private void OnDisable()
        {
            BuildZipOperation.Changed -= OnZipChanged;
            BuildZipOperation.AbandonPending(this);
            _commandWaiting = false;
        }

        private void OnZipChanged()
        {
            if (!BuildZipOperation.HasForegroundRequest) _commandWaiting = false;
            Repaint();
        }

        #endregion

        private const float leftColumnWidth = 120f;
        private const float leftColumnPadding = 10f;
        private const float clickToRemoveWidth = 16f;

        void OnGUI()
        {
            _commandClicked = false;
            if (_buildInfo == null)
            {
                LoadBuildData<BuildInfo>();
            }
            if(_buildConfig == null)
            {
                LoadBuildDataSO<BuildConfig>();
                if (_buildConfig != null && _buildConfig.scenes == null)
                {
                    _buildConfig.scenes = new List<SceneSelection>();
                }
                SyncSceneSelections();
            }
            if (_buildInfo == null || _buildConfig == null)
            {
                EditorGUILayout.HelpBox("Build information/configuration could not be loaded.", MessageType.Error);
                return;
            }
            if (string.IsNullOrEmpty(_buildConfig.InstallerScriptLocation) || !File.Exists(_buildConfig.InstallerScriptLocation))
            {
                // look for iss file
                string existingIss = FindFirstFileWithExtension(BuildConfig.BaseEditorPathProject, ".iss");
                if (existingIss != null)
                {
                    _buildConfig.InstallerScriptLocation = BuildConfig.ShortenPath(existingIss);
                }
                else
                {
                    // create default file from template
                    string defaultInstallerScriptDir = BuildConfig.DefaultFolderToBrowseInstallerScriptLocation;
                    if (!Directory.Exists(defaultInstallerScriptDir))
                    {
                        Directory.CreateDirectory(defaultInstallerScriptDir);
                    }
                    string installerScriptLocation = Path.GetFullPath(Path.Combine(defaultInstallerScriptDir, Installer.DEFAULT_FILENAME_WITH_EXTENSION));
                    Debug.Log("Created " + installerScriptLocation);
                    Installer.WriteDefault(installerScriptLocation);
                    _buildConfig.InstallerScriptLocation = BuildConfig.ShortenPath(installerScriptLocation);
                }
            }

            if (!populatedOptions)
            {
                PopulateSceneOptions();
            }

            DrawZipProgress();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUI.BeginChangeCheck();

            GUILayout.Space(20);

            if(_buildInfo == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("BuildInfo not found", GUILayout.Width(leftColumnWidth));
                return;
            }
            if(_buildConfig == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("BuildConfig not found", GUILayout.Width(leftColumnWidth));
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Company Name", GUILayout.Width(leftColumnWidth));
            _buildInfo.CompanyName = EditorGUILayout.TextField(_buildInfo.CompanyName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Application Name", GUILayout.Width(leftColumnWidth));
            _buildInfo.ApplicationName = EditorGUILayout.TextField(_buildInfo.ApplicationName);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Major Version", GUILayout.Width(leftColumnWidth));
            _buildInfo.MajorVersion = EditorGUILayout.IntField(_buildInfo.MajorVersion);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Minor Version", GUILayout.Width(leftColumnWidth));
            _buildInfo.MinorVersion = EditorGUILayout.IntField(_buildInfo.MinorVersion);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Version", GUILayout.Width(leftColumnWidth));
            _buildInfo.BuildVersion = EditorGUILayout.IntField(_buildInfo.BuildVersion);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Notes", GUILayout.Width(leftColumnWidth));
            _buildInfo.BuildNotes = EditorGUILayout.TextArea(_buildInfo.BuildNotes, GUILayout.Height(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(leftColumnWidth * 2f));
            if (GUILayout.Button("Load From Project", GUILayout.Width(leftColumnWidth))) LoadFromProject();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Scenes", GUILayout.Width(leftColumnWidth));

            EditorGUILayout.BeginVertical();

            List<int> clickedToRemove = new List<int>();
            for (int i = 0; i < scenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                int sceneSelection = scenes[i].selectedSceneId;
                GUILayout.Space(leftColumnPadding);
                EditorGUILayout.LabelField("Scene " + i.ToString(), GUILayout.Width(leftColumnWidth - leftColumnPadding - clickToRemoveWidth));
                if (GUILayout.Button("-", GUILayout.Width(clickToRemoveWidth))) clickedToRemove.Add(i);
                int newSelection = EditorGUILayout.Popup(sceneSelection, _sceneOptions, GUILayout.ExpandWidth(true));
                if (newSelection != sceneSelection || string.IsNullOrEmpty(scenes[i].selectedScene))
                {
                    scenes[i].selectedSceneId = newSelection;
                    if (_sceneOptions != null && newSelection >= 0 && newSelection < _sceneOptions.Length)
                    {
                        scenes[i].selectedScene = _sceneOptions[newSelection];
                    }
                }
                EditorGUILayout.EndHorizontal();

                bool showToDelete = _buildConfig.OneBuildPerScene || i == 0;
                if (showToDelete)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(leftColumnPadding);
                    EditorGUILayout.LabelField("To delete", GUILayout.Width(leftColumnWidth - leftColumnPadding));
                    scenes[i].toDelete = EditorGUILayout.TextField(scenes[i].toDelete, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
            }
            for (int i = clickedToRemove.Count - 1; i >= 0; i--)
            {
                scenes.RemoveAt(clickedToRemove[i]);
                SaveBuildConfig();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(leftColumnWidth * 2f));
            if (!_busy)
            {
                if (GUILayout.Button("↻ Refresh Scenes", GUILayout.Width(leftColumnWidth))) populatedOptions = false;
                if (GUILayout.Button("+ Add Scene", GUILayout.Width(leftColumnWidth)))
                {
                    var newScene = new SceneSelection();
                    if (_sceneOptions != null && _sceneOptions.Length > 0)
                    {
                        newScene.selectedSceneId = 0;
                        newScene.selectedScene = _sceneOptions[0];
                    }
                    scenes.Add(newScene);
                    SaveBuildConfig();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(leftColumnWidth * 2f));
            if (!_busy)
            {
                EditorGUILayout.LabelField("Debug (Development) build");
                _buildConfig.Debug = EditorGUILayout.Toggle(_buildConfig.Debug);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("One build per scene", GUILayout.Width(leftColumnWidth));
            _buildConfig.OneBuildPerScene = EditorGUILayout.Toggle(_buildConfig.OneBuildPerScene, GUILayout.Width(leftColumnWidth));
            if (_buildConfig.OneBuildPerScene && (_buildConfig.ArchiveToZip || _buildConfig.MakeInstaller))
            {
                EditorGUILayout.LabelField("Skip application name", GUILayout.Width(leftColumnWidth + 15f));
                _buildConfig.SkipApplicationName = EditorGUILayout.Toggle(_buildConfig.SkipApplicationName);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Archive to Zip", GUILayout.Width(leftColumnWidth));
            _buildConfig.ArchiveToZip = EditorGUILayout.Toggle(_buildConfig.ArchiveToZip);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Make Installer", GUILayout.Width(leftColumnWidth));
            _buildConfig.MakeInstaller = EditorGUILayout.Toggle(_buildConfig.MakeInstaller);
            EditorGUILayout.EndHorizontal();

            if (_buildConfig.MakeInstaller)
            {
                EditorGUILayout.BeginHorizontal();
                var overwriteGuidWithTooltip = new GUIContent(
                    "Overwrite Guid (?)", "This manages/replaces the unique identifier (guid) of your InstallerScript.iss with the unity scene's guid for each build (find it in MyUnityScene.meta file).\n\n" +
                    "This keeps installation and versioning auto-replacement consistent as long as you continue to use the same scene file.\n\n" +
                    "Untick this if you want to manage it manually.");
                EditorGUILayout.LabelField(overwriteGuidWithTooltip, GUILayout.Width(leftColumnWidth));
                _buildConfig.OverwriteGuid = EditorGUILayout.Toggle(_buildConfig.OverwriteGuid);
                EditorGUILayout.EndHorizontal();
            }

            if (_buildConfig.MakeInstaller)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Installer Script Location: " + _buildConfig.InstallerScriptLocation, GUILayout.Width(leftColumnWidth), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Browse", GUILayout.Width(leftColumnWidth)))
                {
                    var folderForPanel = BuildConfig.DefaultFolderToBrowseInstallerScriptLocation;
                    var chosenFile = EditorUtility.OpenFilePanelWithFilters("Select your installer script", folderForPanel, new string[] { Installer.DEFAULT_FILENAME_DESCRIPTION, Installer.DEFAULT_FILE_EXTENSION_WITHOUT_DOT });
                    if (!string.IsNullOrEmpty(chosenFile))
                    {
                        chosenFile = BuildConfig.ShortenPath(chosenFile);
                        _buildConfig.InstallerScriptLocation = chosenFile;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            var canBuild = scenes != null && scenes.Count > 0 && !_busy && !BuildZipOperation.HasForegroundRequest;
            var canArchive = canBuild && (_buildConfig.ArchiveToZip || _buildConfig.MakeInstaller);
            EditorGUILayout.LabelField("Build Commands", GUILayout.Width(leftColumnWidth));
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = canArchive;
            if (GUILayout.Button("Archive", GUILayout.Width(leftColumnWidth))) RequestCommand(false, true);
            GUI.enabled = canBuild;
            if (GUILayout.Button("Build", GUILayout.Width(leftColumnWidth))) RequestCommand(true, false);
            GUI.enabled = canArchive;
            if (GUILayout.Button("Build + Archive", GUILayout.Width(leftColumnWidth))) RequestCommand(true, true);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            //if (GUILayout.Button("Test Button for Development"))
            //{
            //    ////string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_buildConfig));
            //    //string guid = AssetDatabase.AssetPathToGUID(DataSO.ShortenPath(scenes[0].selectedScene));
            //    //// this is just an example guid from the MyUnityScene.meta, we want to change its format to
            //    ////                                         9fc0d401-0bbf-28b4-5940-72e72b8655ab
            //    //if (!guid.Contains("-") && guid.Length == "9fc0d4010bbf28b4594072e72b8655ab".Length)
            //    //{
            //    //    guid = StringExt.InsertStringAt(guid, new int[] { 8, 12, 16, 24 }, "-");
            //    //}
            //    //Debug.Log(guid);
            //    //Debug.Log(Path.Combine(Installer.innoSetupFolder, "PenseCreTemplate.iss"));
            //    //Debug.Log(BuildConfig.DefaultFolderToBrowseInstallerScriptLocation);
            //    //Debug.Log(Installer.DEFAULT_FILENAME_WITH_EXTENSION);
            //    //var folderForPanel = _buildConfig.InstallerScriptLocation;// BuildConfig.DefaultInstallerScriptLocation;
            //    //var chosenFile = EditorUtility.OpenFilePanelWithFilters("Select your installer script", folderForPanel, new string[] { Installer.DEFAULT_FILENAME_DESCRIPTION, Installer.DEFAULT_FILE_EXTENSION_WITHOUT_DOT });
            //    //Debug.Log(chosenFile);
            //    //Debug.Log(Path.Combine(UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(PacotePenseCreEditorWindow).Assembly).resolvedPath, "Utilities~", "InnoSetupPortable", "ISCC.exe"));

            //    //Debug.Log(typeof(PacotePenseCreEditorWindow).Assembly.Location);
            //    //var a = System.Reflection.Assembly.GetAssembly(typeof(Build)).Location; //this is the location of the cached dll, useless in this context
            //    // example: C:\Users\user\AppData\Local\Unity\cache\packages\package.openupm.com\org.pensecre.pacote@version\Utilities~\InnoSetupPortable
            //    //var a = Environment.GetEnvironmentVariable("UPM_CACHE_PATH") ?? Environment.GetEnvironmentVariable("LOCALAPPDATA");
            //    //Debug.Log(a);
            //    for (int i = 0; i < _buildConfig.buildSettings.Length; i++)
            //    {
            //        //Debug.Log(_buildConfig.buildSettings[i].Key + " : " + _buildConfig.buildSettings[i].Value);
            //    }
            //    //new ReleasePlayerSettings().ApplySettings(_buildInfo.ApplicationName, _buildInfo.CompanyName, _buildConfig.buildSettings);
            //}


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // expand horizontal space so next element is on the right side of the window
            EditorGUILayout.LabelField("Pacote Pense & Cre v" + PacotePenseCreVersionString, GUILayout.Width(160f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck() && !_commandClicked)
            {
                SaveBuildConfig();
            }
            EditorGUILayout.EndScrollView();
        }

        private void SaveBuildConfig()
        {
            if (_buildConfig != null)
            {
                EditorUtility.SetDirty(_buildConfig);
                AssetDatabase.SaveAssets();
            }
        }

        private void PopulateSceneOptions()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] sceneFiles = directory.GetFiles("*.unity", SearchOption.AllDirectories);

            _sceneOptions = new string[sceneFiles.Length];

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            for (int i = 0; i < sceneFiles.Length; i++)
            {
                string fullPath = Path.GetFullPath(sceneFiles[i].FullName);
                if (fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _sceneOptions[i] = fullPath.Substring(projectRoot.Length).TrimStart('\\', '/');
                }
                else
                {
                    _sceneOptions[i] = sceneFiles[i].FullName;
                }
            }

            SyncSceneSelections();

            populatedOptions = true;
        }

        private void SyncSceneSelections()
        {
            if (_sceneOptions == null || _sceneOptions.Length == 0 || scenes == null) return;

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i] == null)
                {
                    scenes[i] = new SceneSelection();
                }

                if (!string.IsNullOrEmpty(scenes[i].selectedScene))
                {
                    string normalized = scenes[i].selectedScene;
                    if (Path.IsPathRooted(normalized) && normalized.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        normalized = normalized.Substring(projectRoot.Length).TrimStart('\\', '/');
                        scenes[i].selectedScene = normalized;
                    }

                    int index = Array.FindIndex(_sceneOptions, opt =>
                        string.Equals(opt, normalized, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(opt.Replace('/', '\\'), normalized.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase));

                    if (index >= 0)
                    {
                        scenes[i].selectedSceneId = index;
                        scenes[i].selectedScene = _sceneOptions[index];
                    }
                    else if (scenes[i].selectedSceneId >= _sceneOptions.Length || scenes[i].selectedSceneId < 0)
                    {
                        scenes[i].selectedSceneId = 0;
                        scenes[i].selectedScene = _sceneOptions[0];
                    }
                }
                else if (_sceneOptions.Length > 0)
                {
                    if (scenes[i].selectedSceneId < 0 || scenes[i].selectedSceneId >= _sceneOptions.Length)
                    {
                        scenes[i].selectedSceneId = 0;
                    }
                    scenes[i].selectedScene = _sceneOptions[scenes[i].selectedSceneId];
                }
            }
        }

        private string GuidFromAbsPath(string fullPath, bool forceFormatWithDashes = true)
        {
            string guid = AssetDatabase.AssetPathToGUID(DataSO.ShortenPath(fullPath));
            // this is just an example guid from the MyUnityScene.meta, we want to change its format to
            //                                         9fc0d401-0bbf-28b4-5940-72e72b8655ab
            if (!guid.Contains("-") && guid.Length == "9fc0d4010bbf28b4594072e72b8655ab".Length)
            {
                guid = StringExt.InsertStringAt(guid, new int[] { 8, 12, 16, 24 }, "-");
            }
            return guid;
        }

        /// <summary>
        /// Returns full path of first file with the extension in given path.
        /// </summary>
        /// <param name="extension">with the dot</param>
        private string FindFirstFileWithExtension(string rootPath, string extension)
        {
            foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file).Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }

        #region Custom Build Methods

        private void LoadFromProject()
        {
            _buildInfo.TryParseVersion(PlayerSettings.bundleVersion);
            _buildInfo.CompanyName = PlayerSettings.companyName;
            _buildInfo.ApplicationName = PlayerSettings.productName;
        }

        private void RequestCommand(bool buildPlayer, bool archive)
        {
            _commandClicked = true;
            if (!BuildZipOperation.TryClaimForeground()) return;
            try
            {
                if (BuildZipOperation.IsActive && !EditorUtility.DisplayDialog(
                    "ZIP in progress",
                    "A ZIP operation is in progress. Starting a new " + (buildPlayer ? "build" : "archive") +
                    " will cancel unfinished zipping and delete its partial ZIP. Continue?",
                    buildPlayer ? "Cancel ZIP and Build" : "Cancel ZIP and Archive", "Keep Zipping"))
                {
                    BuildZipOperation.ReleaseForeground();
                    return;
                }
                string configSnapshot = EditorJsonUtility.ToJson(_buildConfig);
                string infoSnapshot = JsonUtility.ConvertToJson(_buildInfo);
                string configGuid = GuidFromAbsPath(AssetDatabase.GetAssetPath(_buildConfig));
                _commandWaiting = true;
                BuildZipOperation.AfterCancellation(this, () =>
                {
                    _commandWaiting = false;
                    var config = CreateInstance<BuildConfig>();
                    try
                    {
                        EditorJsonUtility.FromJsonOverwrite(configSnapshot, config);
                        config.hideFlags = HideFlags.HideAndDontSave;
                        var info = JsonUtility.ConvertJsonToObject<BuildInfo>(infoSnapshot);
                        _busy = true;
                        EditorCoroutine.start(RunRequestedRoutine(buildPlayer, archive, info, config, configGuid));
                    }
                    catch
                    {
                        DestroyImmediate(config);
                        _busy = false;
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _commandWaiting = false;
                BuildZipOperation.ReleaseForeground();
                Debug.LogError("[BUILD ZIP] Request did not start: " + ex.Message);
            }
        }

        private IEnumerator RunRequestedRoutine(bool buildPlayer, bool archive, BuildInfo info, BuildConfig config, string configGuid)
        {
            try
            {
                BuildZipOperation.EnsureCanBuild();
                SaveBuildConfig();
                if (buildPlayer) Build.ClearLog();
                bool release = !config.Debug;
                if (config.scenes == null || config.scenes.Count == 0) throw new InvalidOperationException("Select at least one build scene.");
                string[] selectedScenes = config.scenes.Select(s => s.selectedScene.Substring(s.selectedScene.LastIndexOf("Assets")).Replace(@"\", "/")).ToArray();
                var zipRequests = new List<BuildZipRequest>();
                int count = config.OneBuildPerScene ? selectedScenes.Length : 1;
                for (int i = 0; i < count; i++)
                {
                    BuildZipOperation.EnsureCanBuild();
                    string[] buildScenes = config.OneBuildPerScene ? new[] { selectedScenes[i] } : selectedScenes;
                    string guid = config.OneBuildPerScene ? GuidFromAbsPath(config.scenes[i].selectedScene) : configGuid;
                    CheckForDefaults(info);
                    info = WriteBuildInfo(release, info);
                    if (buildPlayer)
                    {
                        _buildingScene = true;
                        BuildOptions options = release ? BuildOptions.ShowBuiltPlayer : BuildOptions.Development;
                        bool success = Build.RunBuild(buildScenes, info, config, BuildTarget.StandaloneWindows64, options, () => _buildingScene = false);
                        if (!success) yield break;
                        while (_buildingScene) yield return null;
                    }
                    Build.DeleteAfterBuild(buildScenes, info, BuildTarget.StandaloneWindows64, config.scenes[i].toDelete);
                    if (archive)
                    {
                        zipRequests.AddRange(Build.CreateZipRequests(buildScenes, info, BuildTarget.StandaloneWindows64, config));
                        Build.MakeInstaller(buildScenes, info, BuildTarget.StandaloneWindows64, config, guid);
                    }
                    // Explicit polling/yields: the existing helper does not interpret Unity wait objects.
                    yield return null;
                }
                if (zipRequests.Count > 0) BuildZipOperation.Start(zipRequests);
            }
            finally
            {
                DestroyImmediate(config);
                _buildingScene = false;
                _busy = false;
                _commandWaiting = false;
                BuildZipOperation.ReleaseForeground();
                populatedOptions = false;
                if (this != null) Repaint();
            }
        }

        // Preserve the existing archive-only entry point and apply the same admission guard.
        public IEnumerator ArchiveOnlyRoutine()
        {
            RequestCommand(false, true);
            yield break;
        }

        private void DrawZipProgress()
        {
            var status = BuildZipOperation.Status;
            if (status == null && !_commandWaiting) return;
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_commandWaiting) EditorGUILayout.LabelField("Waiting for ZIP cancellation and cleanup...");
            if (status != null)
            {
                EditorGUILayout.LabelField("Build ZIP: " + status.State, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Archive " + status.ArchiveIndex + " / " + status.ArchiveCount + ": " + status.ArchiveName, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrEmpty(status.CurrentFile)) EditorGUILayout.LabelField(status.CurrentFile, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField(string.Format("{0} files • {1:0.0} MiB processed • {2} archive(s) complete", status.FilesCopied, status.BytesCopied / (1024d * 1024d), status.CompletedArchives));
                if (BuildZipOperation.IsActive)
                {
                    EditorGUILayout.LabelField(status.State == BuildZipState.Cancelling ? "Stopping and deleting partial ZIP..." : "Working in background...");
                    using (new EditorGUI.DisabledScope(status.State == BuildZipState.Cancelling))
                        if (GUILayout.Button("Cancel ZIP"))
                        {
                            _commandClicked = true;
                            BuildZipOperation.Cancel();
                        }
                }
                if (!string.IsNullOrEmpty(status.Error)) EditorGUILayout.HelpBox(status.Error, MessageType.Error);
            }
            EditorGUILayout.EndVertical();
        }
        private BuildInfo WriteBuildInfo(bool release, BuildInfo info)
        {
            BuildInfo currentBuildInfo = new BuildInfo
            {
                CompanyName = info.CompanyName,
                ApplicationName = info.ApplicationName,
                MajorVersion = info.MajorVersion,
                MinorVersion = info.MinorVersion,
                BuildVersion = info.BuildVersion,
                BuildNotes = info.BuildNotes,
                BuildDateTime = CurrentDateString,
                PacotePenseCreVersion = PacotePenseCreVersionString,
                Release = release
            };

            FileUtility.WriteFile(BuildInfoManager.GetFullFilePath<BuildInfo>(), JsonUtility.ConvertToJson(currentBuildInfo, Formatting.Indented));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return currentBuildInfo;
        }

        void LoadBuildData<T>() where T : BuildData
        {
            string fullFilePath = BuildInfoManager.GetFullFilePath<T>();
            if (!FileUtility.CheckFileExists(fullFilePath))
            {
                var defaultValues = BuildInfoManager.GetDefaultValues<T>();
                FileUtility.WriteFile(fullFilePath, JsonUtility.ConvertToJson(defaultValues, Formatting.Indented));
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            // first field of matching type passed to this function that is in this class
            var myFieldWithMatchingType = this.GetType().GetFields().FirstOrDefault(x => x.FieldType == typeof(T));
            if(myFieldWithMatchingType != null)
            {
                myFieldWithMatchingType.SetValue(this, JsonUtility.ConvertJsonToObject<T>(FileUtility.ReadFile(fullFilePath)));
                // this above line is to make this method adapt to further work on this class, and is equivalent to hardcoding the following:
                //if (typeof(T) == typeof(BuildInfo))
                //    _buildInfo = JsonUtility.ConvertJsonToObject<BuildInfo>(FileUtility.ReadFile(fullFilePath));
                //else if (typeof(T) == typeof(BuildConfig))
                //    _buildConfig = JsonUtility.ConvertJsonToObject<BuildConfig>(FileUtility.ReadFile(fullFilePath));
            }
        }
        void LoadBuildDataSO<T>() where T : DataSO
        {
            string fullFilePath = BuildInfoManager.GetSOFullFilePath<T>();
            string relativeFilePath = BuildInfoManager.GetSOFullFilePath<T>(true);
            if (!FileUtility.CheckFileExists(fullFilePath))
            {
                var defaultValues = BuildInfoManager.GetSODefaultValues<T>();
                string directory = Path.GetDirectoryName(fullFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                }
                AssetDatabase.CreateAsset(defaultValues, relativeFilePath);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            // first field of matching type passed to this function that is in this class
            var myFieldWithMatchingType = this.GetType().GetFields().FirstOrDefault(x => x.FieldType == typeof(T));
            if (myFieldWithMatchingType != null)
            {
                myFieldWithMatchingType.SetValue(this, AssetDatabase.LoadAssetAtPath<T>(relativeFilePath));
            }
        }

        private void CheckForDefaults(BuildInfo info)
        {
            if (info.ApplicationName == "" || info.ApplicationName == String.Empty || info.ApplicationName == "DEFAULT")
            {
                throw new Exception("[APPLICATIONNAME] Defaults Still Set. Change Application Name");
            }

            if (info.CompanyName == "" || info.CompanyName == String.Empty)
            {
                throw new Exception("[COMPANYNAME] Defaults Still Set. Change Company Name");
            }
        }

        private string CurrentDateString
        {
            get { return string.Format("{0:MM/dd/yy - hh:mm:ss}", System.DateTime.Now); }
        }

        /// <summary>
        /// Example format: 1.2.3 (major, minor, build/update)
        /// Update Version of this package in PacotePenseCre/package.json when developing it. Check the latest version in the Window > PackageManager.
        /// </summary>
        public string PacotePenseCreVersionString
        {
            get { return UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(PacotePenseCreEditorWindow).Assembly).version; }
        }
        #endregion
    }
}
