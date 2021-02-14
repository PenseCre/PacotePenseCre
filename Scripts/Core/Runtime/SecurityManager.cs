using System;
using System.Collections;
//using PacotePenseCre.Helpers;
using PacotePenseCre.Extensions;
using PacotePenseCre.Utility;
using PacotePenseCre.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace PacotePenseCre.Core
{
    public class SecurityManager : MonoBehaviour
    {
        #region private variables
        protected readonly string Key = "tvZ#y$7(rKPG4u-#{93/Q:MUHT(n(K";

        private const string SecurityFilePath = "Config";
        private const string SecurityFileName = "akn.xdon";

        #endregion

        #region Unity Methods

        private void Start()
        {
            if (Application.isEditor | Debug.isDebugBuild) return;

            if (!AppIsAuthorised())
            {
                StartCoroutine(SecurityShutdownRoutine());
            }
        }

        #endregion

        #region SecurityManager Methods

        private bool AppIsAuthorised()
        {
            string path = Application.dataPath + "/../" + SecurityFilePath + "/" + SecurityFileName;

            if (!FileUtility.CheckFileExists(path))
            {
                Debug.Log("[SecurityManager] - Security file is missing");
                return false;
            }

            string fileContents = FileUtility.ReadFile(path);

            if (string.IsNullOrEmpty(fileContents)) return false;

            string decryptFileContents;

            try
            {
                Debug.Log(ApplicationManager.proj.ToString());
                decryptFileContents = EncryptionUtility.DecryptString(fileContents, Key);
            }
            catch (Exception)
            {
                return false;
            }

            if (string.IsNullOrEmpty(decryptFileContents)) return false;

            string decryptedStringMinusRnd = decryptFileContents.Substring(3, decryptFileContents.Length - 5).Remove(27,2).Remove(13,1); // get the string minus some random characters
            string appDataPath = Application.dataPath;
            int startIndex = appDataPath.LastIndexOf("/", StringComparison.Ordinal);
            string applicationName = appDataPath.Substring(appDataPath.LastIndexOf("/", StringComparison.Ordinal), appDataPath.Length - startIndex).GetStringBetween("/", "_Data");
            string[] decryptedKeys = decryptedStringMinusRnd.Split('!');

            if (decryptedKeys.Length != 4)
            {
                return false;
            }

            string macAddress = decryptedKeys[0];
            string decryptedApplicationExeName = decryptedKeys[1];
            string decryptedBiosInfo = decryptedKeys[2];
            string decryptedOSInfo = decryptedKeys[3];

            if (!VerifyMacAddress(macAddress)) //Check MAC ADDRESS is correct
            {
                Debug.Log("[SecurityManager] - The app was not authorized to run on this device");
                return false;
            }

            if (decryptedApplicationExeName != applicationName) //Check Application Name is correct
            {
                Debug.Log("[SecurityManager] - This app was not authorized to run");
                return false;
            }

            if (!VerifyBiosInfo(decryptedBiosInfo)) //Check hardware / bios information is correct
            {
                Debug.Log("[SecurityManager] - The app was not authorized to run on this hardware");
                return false;
            }
            
            if (!VerifyOSInfo(decryptedBiosInfo)) //Check OS information is correct
            {
                Debug.Log("[SecurityManager] - The app was not authorized to run on this system");
                return false;
            }

            return true;
        }

        private bool VerifyMacAddress(string macAddress)
        {
            return macAddress == NetworkUtility.GetMacAddress();
        }

        private bool VerifyBiosInfo(string biosInfo)
        {
            return biosInfo == HardwareUtility.GetBiosInfo();
        }

        private bool VerifyOSInfo(string OSInfo)
        {
            return OSInfo == HardwareUtility.GetOSInfo();
        }

        #endregion

        #region Routines

        /// <summary>
        /// Deprecated, moved to UI
        /// </summary>
        /// <returns></returns>
        private IEnumerator SecurityShutdownRoutine()
        {
            Debug.Log("[SecurityManager] - Security Shutdown Routine");
            WindowManager.Instance.ShowWindow("SecurityWindow", false);
            
            yield return new WaitForSeconds(6.0f);
            
            Application.Quit();
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        }

        #endregion
    }
}
