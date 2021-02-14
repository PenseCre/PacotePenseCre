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

namespace PacotePenseCre.Editor
{
    class PacotePenseCreEditorWindow : EditorWindow
    {
        #region Custom Editor Window Code

        [MenuItem("Window/PenseCre")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<PacotePenseCreEditorWindow>("PenseCre Editor Window");
        }

        #endregion

        #region Properties

        private BuildInfo _buildInfo;

        public List<SceneSelection> scenes = new List<SceneSelection>();

        private string[] _sceneOptions;
        private bool populatedOptions;

        private bool _building;
        private bool _buildingScene;

        #endregion

        void OnGUI()
        {
            if (_buildInfo == null)
            {
                LoadBuildData();
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
            EditorGUILayout.EndVertical();


            GUILayout.Space(20);

            EditorGUILayout.LabelField("Build Settings", GUILayout.Width(120));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build Debug")) EditorCoroutine.start(BuildProjectRoutine(false));
            if (GUILayout.Button("Build Release")) EditorCoroutine.start(BuildProjectRoutine(true));
            EditorGUILayout.EndHorizontal();

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

            for (int i = 0; i < scenes.Count; i++)
            {
                _buildingScene = true;
                buildScenes[i] = scenes[i].selectedScene.Substring(scenes[i].selectedScene.LastIndexOf("Assets")).Replace(@"\", "/");

                CheckForDefaults();
                WriteBuildInfo(release);
                BuildOptions buildOptions = release ? BuildOptions.None | BuildOptions.ShowBuiltPlayer : BuildOptions.Development;
                Build.RunBuild(new[] { buildScenes[i] }, _buildInfo, BuildTarget.StandaloneWindows64, buildOptions, BuildSceneCompleted);

                yield return new WaitUntil(() => !_buildingScene);
            }

            _building = false;
        }

        private void BuildSceneCompleted()
        {
            LoadBuildData();
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

            FileUtility.WriteFile(Application.streamingAssetsPath + "/" + BuildInfoManager.fileName, JsonUtility.ConvertToJson(currentBuildInfo, Formatting.Indented));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        void LoadBuildData()
        {
            if (!FileUtility.CheckFileExists(Application.streamingAssetsPath + "/" + BuildInfoManager.fileName))
            {
                FileUtility.WriteFile(Application.streamingAssetsPath + "/" + BuildInfoManager.fileName, JsonUtility.ConvertToJson(BuildInfo.DefaultValues, Formatting.Indented));
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            _buildInfo = JsonUtility.ConvertJsonToObject<BuildInfo>(FileUtility.ReadFile(Application.streamingAssetsPath + "/" + BuildInfoManager.fileName));
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