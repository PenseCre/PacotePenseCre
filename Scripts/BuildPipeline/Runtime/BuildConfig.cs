using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace PacotePenseCre.BuildPipeline
{
    [Serializable]
    public class SceneSelection
    {
        public int selectedSceneId;
        public string selectedScene;
        public string toDelete;
    }

    [Serializable]
    public class BuildConfig : DataSO
    {
        public static new readonly string FileName = "buildConfig.asset";
        public static readonly string DefaultInstallerScriptLocation = Path.Combine(BaseEditorPathProject, "InstallerScript");
        public static string DefaultFolderToBrowseInstallerScriptLocation
        {
            get
            {
                if (!Directory.Exists(DefaultInstallerScriptLocation))
                {
                    Directory.CreateDirectory(DefaultInstallerScriptLocation);
                }
                return DefaultInstallerScriptLocation;
            }
        }

        public BuildSetting[] buildSettings = null;

        public List<SceneSelection> scenes = new List<SceneSelection>();

        public bool OneBuildPerScene;
        public bool ArchiveToZip;
        public bool MakeInstaller;
        public bool SkipApplicationName;
        public bool Debug;
        public bool OverwriteGuid = true;

        /// <summary>
        /// Use <see cref="DataSO.ExpandPath"/> or <see cref="DataSO.ShortenPath"/> accordingly
        /// </summary>
        [ReadOnly] public string InstallerScriptLocation;

        public static new BuildConfig DefaultValues
        {
            get
            {
                var ret = CreateInstance<BuildConfig>();
                ret.scenes = new List<SceneSelection>();
                ret.OneBuildPerScene = false;
                ret.ArchiveToZip = true;
                ret.MakeInstaller = false;
                ret.SkipApplicationName = false;
                return ret;
            }
        }

        //// scriptable objects don't support constructors as of Unity 2020.3, it requires CreateInstance<>() like the above method
        //public static new BuildConfig DefaultValues = new BuildConfig()
        //{
        //    OneBuildPerScene = false,
        //    ArchiveToZip = true,
        //    MakeInstaller = false
        //};
    }
}