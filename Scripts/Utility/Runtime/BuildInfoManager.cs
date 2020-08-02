using PacotePenseCre.Scripts.Utility;
using UnityEngine;
using JsonUtility = PacotePenseCre.Scripts.Utility.JsonUtility;

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
