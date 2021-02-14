using UnityEngine;
using PacotePenseCre.Utility;
using JsonUtility = PacotePenseCre.Utility.JsonUtility;

namespace PacotePenseCre.BuildPipeline
{
    public static class BuildInfoManager
    {
        private static BuildInfo _buildInfo;
        public static BuildInfo BuildInfo
        {
            get
            {
                if (_buildInfo == null)
                {
                    if (FileUtility.CheckFileExists(Application.streamingAssetsPath + "/" + fileName))
                    {
                        _buildInfo = JsonUtility.ConvertJsonToObject<BuildInfo>(FileUtility.ReadFile(Application.streamingAssetsPath + "/" + fileName));
                    }
                }

                return _buildInfo;
            }
        }

        public const string fileName = "buildInfo.json";
    }
}