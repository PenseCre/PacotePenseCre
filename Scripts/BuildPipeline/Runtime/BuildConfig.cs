using System;
using System.Collections.Generic;
using UnityEngine;

namespace PacotePenseCre.BuildPipeline
{
    [Serializable]
    public class BuildConfig : DataSO
    {
        public static new readonly string FileName = "buildConfig.asset";
        public BuildSetting[] buildSettings = null;

        public bool OneBuildPerScene;
        public bool ArchiveToZip;
        public bool MakeInstaller;

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