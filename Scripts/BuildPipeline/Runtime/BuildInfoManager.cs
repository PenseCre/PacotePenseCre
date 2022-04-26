using System;
using System.IO;
using UnityEngine;
using PacotePenseCre.Utility;
using JsonUtility = PacotePenseCre.Utility.JsonUtility;

namespace PacotePenseCre.BuildPipeline
{
    public static class BuildInfoManager
    {
        private static BuildInfo _buildInfo;
        private static BuildConfig _buildConfig;
        public static BuildInfo BuildInfo
        {
            get
            {
                if (_buildInfo == null)
                {
                    var fullFilePath = GetFullFilePath<BuildInfo>();
                    if (FileUtility.CheckFileExists(fullFilePath))
                    {
                        _buildInfo = JsonUtility.ConvertJsonToObject<BuildInfo>(FileUtility.ReadFile(fullFilePath));
                    }
                }

                return _buildInfo;
            }
        }
        public static BuildConfig BuildConfig
        {
            get
            {
                if (_buildConfig == null)
                {
                    var fullFilePath = GetSOFullFilePath<BuildConfig>();
                    if (FileUtility.CheckFileExists(fullFilePath))
                    {
                        if (fullFilePath.EndsWith(".asset"))
#if UNITY_EDITOR
                            _buildConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildConfig>(fullFilePath);
#else
                            _buildConfig = null;
#endif
                        else // if(fullFilePath.EndsWith(".json")) // uncomment this if JsonUtility doesn't work with xml and other formats (not tested yet)
                            _buildConfig = JsonUtility.ConvertJsonToObject<BuildConfig>(FileUtility.ReadFile(fullFilePath));
                    }
                }

                return _buildConfig;
            }
        }

        public static string GetFullFilePath<T>() where T : BuildData
        {
            return Path.Combine(Application.streamingAssetsPath, GetFileName<T>());
        }
        public static string GetSOFullFilePath<T>(bool relative = false) where T : BuildDataSO
        {
            return Path.Combine(relative ? BuildDataSO.BasePathRelative : BuildDataSO.BasePath, GetSOFileName<T>());
        }

        public static string GetFileName<T>() where T : BuildData
        {
            return typeof(T).GetField("FileName").GetValue(null) as string;
        }
        public static string GetSOFileName<T>() where T : BuildDataSO
        {
            return typeof(T).GetField("FileName").GetValue(null) as string;
        }

        public static T GetDefaultValues<T>() where T : BuildData
        {
            return typeof(T).GetField("DefaultValues").GetValue(null) as T;
        }
        public static T GetSODefaultValues<T>() where T : BuildDataSO
        {
            return BuildDataSO.DefaultValues as T ?? ScriptableObject.CreateInstance<T>();
            //return typeof(T).GetField("DefaultValues").GetValue(null) as T; // unity doesn't like that
        }
    }
}