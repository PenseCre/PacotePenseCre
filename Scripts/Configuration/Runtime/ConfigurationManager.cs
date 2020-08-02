using UnityEngine;
using PacotePenseCre.Scripts.Utility;
using JsonUtility = PacotePenseCre.Scripts.Utility.JsonUtility;

namespace PacotePenseCre.Scripts.Configuration
{
    /// <summary>
    /// ConfigurationManager - Loads config file(Json) and converts it to class 
    /// </summary>
    public class ConfigurationManager : Singleton<ConfigurationManager>
    {
        public CalibrationConfiguration calibrationConfiguration;

        private const string _calibrationConfigFile = "/CalibrationConfig.json";
        private const string _calibrationConfigPath = "/../Config/";

        void Awake()
        {
            LoadAllConfigurationFiles();
        }

        private void LoadAllConfigurationFiles()
        {
            calibrationConfiguration = JsonUtility.ConvertJsonToObject<CalibrationConfiguration>(LoadConfigurationFile(_calibrationConfigPath, _calibrationConfigFile));
            Debug.Log("[ConfigurationManager] - Loaded All Configuration Files");
        }   

        /// <summary>
        /// LoadConfiguration - 
        /// </summary>
        public string LoadConfigurationFile(string filePath, string fileName)
        {
            string file;

            string _filePathEditor = Application.streamingAssetsPath + fileName;
            string _filePathBuild = Application.dataPath + filePath + fileName;

            if (Application.isEditor)
            {
                file = _filePathEditor;
                Debug.Log("[ConfigurationManager] - Used Streaming Assets Calibration File");
            }
            else
            {
                if (FileUtility.CheckFileExists(_filePathBuild))
                {
                    file = _filePathBuild;
                    Debug.Log("[ConfigurationManager] - Used Config Folder Calibration File");
                }
                else
                {
                    file = _filePathEditor;
                    Debug.Log("[ConfigurationManager] - Used Streaming Assets Calibration File");
                }
            }

            return FileUtility.ReadFile(file);
        }
    }
}