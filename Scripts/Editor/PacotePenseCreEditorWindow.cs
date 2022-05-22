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
            EditorWindow.GetWindow<PacotePenseCreEditorWindow>("PenseCre Build Window");
        }

        #endregion

        #region Properties

        public List<SceneSelection> scenes = new List<SceneSelection>();
        public BuildInfo _buildInfo;
        public BuildConfig _buildConfig;

        private string[] _sceneOptions;
        private bool populatedOptions;

        private bool _building;
        private bool _buildingScene;

        #endregion

        void OnGUI()
        {
            if (_buildInfo == null)
            {
                LoadBuildData<BuildInfo>();
            }
            if(_buildConfig == null)
            {
                LoadBuildDataSO<BuildConfig>();
            }

            if (!populatedOptions)
            {
                PopulateSceneOptions();
            }

            GUILayout.Space(20);

            if(_buildInfo == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("BuildInfo not found", GUILayout.Width(120));
                return;
            }
            if(_buildConfig == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("BuildConfig not found", GUILayout.Width(120));
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Company Name", GUILayout.Width(120));
            _buildInfo.CompanyName = EditorGUILayout.TextField(_buildInfo.CompanyName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Application Name", GUILayout.Width(120));
            _buildInfo.ApplicationName = EditorGUILayout.TextField(_buildInfo.ApplicationName);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Major Version", GUILayout.Width(120));
            _buildInfo.MajorVersion = EditorGUILayout.IntField(_buildInfo.MajorVersion);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Minor Version", GUILayout.Width(120));
            _buildInfo.MinorVersion = EditorGUILayout.IntField(_buildInfo.MinorVersion);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Patch Version", GUILayout.Width(120));
            _buildInfo.PatchVersion = EditorGUILayout.IntField(_buildInfo.PatchVersion);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Version", GUILayout.Width(120));
            _buildInfo.BuildVersion = EditorGUILayout.IntField(_buildInfo.BuildVersion);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Notes", GUILayout.Width(120));
            _buildInfo.BuildNotes = EditorGUILayout.TextArea(_buildInfo.BuildNotes, GUILayout.Height(50));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("One build per scene", GUILayout.Width(120));
            _buildConfig.OneBuildPerScene = EditorGUILayout.Toggle(_buildConfig.OneBuildPerScene);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Archive to Zip", GUILayout.Width(120));
            _buildConfig.ArchiveToZip = EditorGUILayout.Toggle(_buildConfig.ArchiveToZip);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Make Installer", GUILayout.Width(120));
            _buildConfig.MakeInstaller = EditorGUILayout.Toggle(_buildConfig.MakeInstaller);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(20);

            EditorGUILayout.LabelField("Scenes", GUILayout.Width(120));

            EditorGUILayout.BeginVertical();

            for (int i = 0; i < scenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                int sceneSelection = scenes[i].selectedSceneId;
                scenes[i].selectedSceneId = EditorGUILayout.Popup("Scene " + i, sceneSelection, _sceneOptions);
                scenes[i].selectedScene = _sceneOptions[scenes[i].selectedSceneId];
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (!_building)
            {
                if (GUILayout.Button("Refresh Scenes")) populatedOptions = false;
                if (GUILayout.Button("Add Scene")) scenes.Add(new SceneSelection());
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Build Settings", GUILayout.Width(120));
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = scenes != null && scenes.Count > 0;
            if (GUILayout.Button("Build Debug")) EditorCoroutine.start(BuildProjectRoutine(false));
            if (GUILayout.Button("Build Release")) EditorCoroutine.start(BuildProjectRoutine(true));
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Test Button"))
            {
                //var a = System.Reflection.Assembly.GetAssembly(typeof(Build)).Location; //this is the location of the cached dll, useless in this context
                // example: C:\Users\user\AppData\Local\Unity\cache\packages\package.openupm.com\org.pensecre.pacote@version\Utilities~\InnoSetupPortable
                //var a = Environment.GetEnvironmentVariable("UPM_CACHE_PATH") ?? Environment.GetEnvironmentVariable("LOCALAPPDATA");
                //Debug.Log(a);
                for (int i = 0; i < _buildConfig.buildSettings.Length; i++)
                {
                    Debug.Log(_buildConfig.buildSettings[i].Key + " : " + _buildConfig.buildSettings[i].Value);
                }
                new ReleasePlayerSettings().ApplySettings(_buildInfo.ApplicationName, _buildInfo.CompanyName, _buildConfig.buildSettings);
            }

            EditorGUILayout.EndVertical();
        }

        private void PopulateSceneOptions()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] sceneFiles = directory.GetFiles("*.unity", SearchOption.AllDirectories);

            _sceneOptions = new string[sceneFiles.Length];

            for (int i = 0; i < sceneFiles.Length; i++)
            {
                string sceneName = sceneFiles[i].Directory + @"\" + sceneFiles[i].Name;
                if (sceneName != null) _sceneOptions[i] = sceneName;
            }

            populatedOptions = true;
        }

        #region Custom Build Methods

        private IEnumerator BuildProjectRoutine(bool release)
        {
            _building = true;

            string[] buildScenes = new string[scenes.Count];

            if (_buildConfig.OneBuildPerScene)
            {
                for (int i = 0; i < scenes.Count; i++)
                {
                    _buildingScene = true;
                    buildScenes[i] = scenes[i].selectedScene.Substring(scenes[i].selectedScene.LastIndexOf("Assets")).Replace(@"\", "/");

                    CheckForDefaults();
                    WriteBuildInfo(release);
                    BuildOptions buildOptions = release ? BuildOptions.None | BuildOptions.ShowBuiltPlayer : BuildOptions.Development;
                    var myScene = new[] { buildScenes[i] };
                    Build.RunBuild(myScene, _buildInfo, _buildConfig, BuildTarget.StandaloneWindows64, buildOptions, BuildSceneCompleted);

                    yield return new WaitUntil(() => !_buildingScene);

                    yield return new WaitForSeconds(1);
                    Build.Zip(myScene, _buildInfo, BuildTarget.StandaloneWindows64, _buildConfig);
                    yield return new WaitForSeconds(1);
                    Build.MakeInstaller(myScene, _buildInfo, BuildTarget.StandaloneWindows64, _buildConfig);
                }
            }
            else
            {
                _buildingScene = true;
                for (int i = 0; i < scenes.Count; i++)
                {
                    buildScenes[i] = scenes[i].selectedScene.Substring(scenes[i].selectedScene.LastIndexOf("Assets")).Replace(@"\", "/");
                }
                CheckForDefaults();
                WriteBuildInfo(release);
                BuildOptions buildOptions = release ? BuildOptions.None | BuildOptions.ShowBuiltPlayer : BuildOptions.Development;
                Build.RunBuild(buildScenes, _buildInfo, _buildConfig, BuildTarget.StandaloneWindows64, buildOptions, BuildSceneCompleted);

                yield return new WaitUntil(() => !_buildingScene);

                yield return new WaitForSeconds(1);
                Build.Zip(buildScenes, _buildInfo, BuildTarget.StandaloneWindows64, _buildConfig);
                yield return new WaitForSeconds(1);
                Build.MakeInstaller(buildScenes, _buildInfo, BuildTarget.StandaloneWindows64, _buildConfig);
            }
            _building = false;
        }

        private void BuildSceneCompleted()
        {
            LoadBuildData<BuildInfo>();
            LoadBuildDataSO<BuildConfig>();
            _buildingScene = true;
        }

        private void WriteBuildInfo(bool release)
        {
            BuildInfo currentBuildInfo = new BuildInfo
            {
                CompanyName = _buildInfo.CompanyName,
                ApplicationName = _buildInfo.ApplicationName,
                MajorVersion = _buildInfo.MajorVersion,
                MinorVersion = _buildInfo.MinorVersion,
                PatchVersion = _buildInfo.PatchVersion,
                BuildVersion = _buildInfo.BuildVersion,
                BuildNotes = _buildInfo.BuildNotes,
                BuildDateTime = CurrentDateString,
                Release = release
            };

            _buildInfo = currentBuildInfo;

            FileUtility.WriteFile(BuildInfoManager.GetFullFilePath<BuildInfo>(), JsonUtility.ConvertToJson(currentBuildInfo, Formatting.Indented));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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

        private void CheckForDefaults()
        {
            if (_buildInfo.ApplicationName == "" || _buildInfo.ApplicationName == String.Empty || _buildInfo.ApplicationName == "DEFAULT")
            {
                throw new Exception("[APPLICATIONNAME] Defaults Still Set. Change Application Name");
            }

            if (_buildInfo.CompanyName == "" || _buildInfo.CompanyName == String.Empty)
            {
                throw new Exception("[COMPANYNAME] Defaults Still Set. Change Company Name");
            }
        }

        private string CurrentDateString
        {
            get { return string.Format("{0:MM/dd/yy - hh:mm:ss}", System.DateTime.Now); }
        }

        #endregion
    }

    public class SceneSelection
    {
        public int selectedSceneId { get; set; }
        public string selectedScene { get; set; }
    }
}