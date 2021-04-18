using UnityEngine;
using PacotePenseCre.Utility;
using PacotePenseCre.Generics;
using JsonUtility = PacotePenseCre.Utility.JsonUtility;

namespace PacotePenseCre.Configuration
{
    /// <summary>
    /// ConfigurationManager - Loads config file(Json) and converts it to class 
    /// </summary>
    public class ConfigurationManager : Manager
    {
        public Configuration penseCreConfiguration;

        protected readonly string _penseCreConfigFile = "/PenseCreConfig.json";
        protected readonly string _penseCreConfigPath = "/../Config/";

        void Awake()
        {
            LoadAllConfigurationFiles();
        }

        private void LoadAllConfigurationFiles()
        {
            penseCreConfiguration = JsonUtility.ConvertJsonToObject<Configuration>(LoadConfigurationFile(_penseCreConfigPath, _penseCreConfigFile));
            Debug.Log("[ConfigurationManager] - Loaded All Configuration Files");
        }

        /// <summary>
        /// Read the content of the configuration file into a string 
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