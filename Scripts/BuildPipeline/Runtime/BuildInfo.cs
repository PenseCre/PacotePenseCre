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

        public void TryParseVersion(string bundleVersion)
        {
            string[] versionNumbers = bundleVersion.Split('.');
            if(versionNumbers != null && versionNumbers.Length == 3)
            {
                int n0;
                int n1;
                int n2;
                if (int.TryParse(versionNumbers[0], out n0)
                    && int.TryParse(versionNumbers[1], out n1)
                    && int.TryParse(versionNumbers[2], out n2)
                    )
                {
                    MajorVersion = n0;
                    MinorVersion = n1;
                    BuildVersion = n2;
                }
            }
        }
    }
}