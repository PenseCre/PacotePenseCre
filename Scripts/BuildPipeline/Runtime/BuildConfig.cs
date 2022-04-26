using System;

namespace PacotePenseCre.BuildPipeline
{
    [Serializable]
    public class BuildConfig : BuildDataSO
    {
        public static new readonly string FileName = "buildConfig.asset";

        public bool OneBuildPerScene;
        public bool ArchiveToZip;
        public bool MakeInstaller;

        public static new BuildConfig DefaultValues { get => CreateInstance<BuildConfig>(); }

        //// scriptable objects don't support constructors as of Unity 2020.3, it requires CreateInstance<>()
        //public static new BuildConfig DefaultValues = new BuildConfig()
        //{
        //    OneBuildPerScene = false,
        //    ArchiveToZip = true,
        //    MakeInstaller = false
        //};
    }
}