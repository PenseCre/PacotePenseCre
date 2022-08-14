using System;

namespace PacotePenseCre.BuildPipeline
{
    [Serializable]
    public class BuildInfo : BuildData
    {
        public static new readonly string FileName = "buildInfo.json";

        public string CompanyName;
        public string ApplicationName;

        public int MajorVersion;
        public int MinorVersion;
        public int BuildVersion;

        public string BuildNotes;

        public string BuildDateTime;
        [ReadOnly] public string PacotePenseCreVersion;

        public bool Release;

        public static new BuildInfo DefaultValues = new BuildInfo
        {
            CompanyName = "Pense & Cre",
            ApplicationName = "Nosso Projeto",
            MajorVersion = 1,
            MinorVersion = 0,
            BuildVersion = 0,
            BuildNotes = "-",
            BuildDateTime = "",
            Release = false
        };

        public string GetVersionName
        {
            get
            {
                return MajorVersion + "." + MinorVersion + "." + BuildVersion;
            }
        }
    }
}