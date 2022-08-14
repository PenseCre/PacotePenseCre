using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace PacotePenseCre.BuildPipeline
{
    [Serializable]
    public class BuildConfig : DataSO
    {
        public static new readonly string FileName = "buildConfig.asset";
        public static readonly string DefaultInstallerScriptLocation = Path.Combine(BaseEditorPathProject, "InstallerScript");
        public static readonly string DefaultFolderToBrowseInstallerScriptLocation = Directory.Exists(DefaultInstallerScriptLocation) ? DefaultInstallerScriptLocation : BaseEditorPathProject;

        public BuildSetting[] buildSettings = null;

        public bool OneBuildPerScene;
        public bool ArchiveToZip;
        public bool MakeInstaller;
        public bool Debug;

        /// <summary>
        /// Use <see cref="DataSO.ExpandPath"/> or <see cref="DataSO.ShortenPath"/> accordingly
        /// </summary>
        [ReadOnly] public string InstallerScriptLocation;

        public static new BuildConfig DefaultValues
        {
            get
            {
                var ret = CreateInstance<BuildConfig>();
                ret.OneBuildPerScene = false;
                ret.ArchiveToZip = true;
                ret.MakeInstaller = false;
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