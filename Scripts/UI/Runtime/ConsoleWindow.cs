using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PacotePenseCre.BuildPipeline;

namespace PacotePenseCre.UI
{
    public class ConsoleWindow : UIScreen
    {
        public Text versionText;
        public Text platformText;
        public Text appText;
        public Text dateText;
        public Text noteText;

        public Text fpsDisplay;
        private float _deltaTime;

        public Button clearLog;
        public Toggle consoleToggle;
        public Text consoleText;
        private List<string> _consoleLogList = new List<string>();
        private int _maxConsoleLines = 300;
        private int _consoleLineCount;

        #region Unity Methods

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            if(clearLog)
                clearLog.onClick.AddListener(ClearLog);

            UpdateBuildInfo(BuildInfoManager.BuildInfo);
            UpdateBuildConfig(BuildInfoManager.BuildConfig);
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            if(clearLog)
                clearLog.onClick.RemoveListener(ClearLog);
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                if (isVisible)
                {
                    HideScreen();
                }
                else
                {
                    ShowScreen();
                }
            }
            if (Input.GetKeyUp(KeyCode.M))
            {
                Cursor.visible = !Cursor.visible;
            }
        }

        void OnGUI()
        {
            if (!isVisible) return;

            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
            if (fpsDisplay)
                fpsDisplay.text = string.Format("FPS : {0}", (1.0f / _deltaTime));
        }

        public void UpdateBuildInfo(BuildInfo buildInfo)
        {
            if (versionText)
                versionText.text = string.Format("[VERSION] {0}.{1}.{2}.{3}", buildInfo.MajorVersion, buildInfo.MinorVersion, buildInfo.PatchVersion, buildInfo.BuildVersion);
            if (platformText)
                platformText.text = string.Format("[BUILD_TYPE] {0} [PLATFORM] {1}", buildInfo.Release ? "Release" : "Development", Application.platform);
            if (appText)
                appText.text = string.Format("[COMPANY_NAME] {0} [APPLICATION_NAME] {1}",buildInfo.CompanyName, buildInfo.ApplicationName);
            if (dateText)
                dateText.text = string.Format("[BUILD_DATE] {0}", buildInfo.BuildDateTime);
            if (noteText)
                noteText.text = string.Format("[BUILD_NOTES] {0}", buildInfo.BuildNotes);
        }
        public void UpdateBuildConfig(BuildConfig buildConfig)
        {
            // todo
            //buildConfig.ArchiveToZip
            //buildConfig.MakeInstaller
            //buildConfig.OneBuildPerScene
        }


        #endregion

        #region Console

        private void HandleLog(string message, string stacktrace, LogType type)
        {
            if (consoleText != null && consoleToggle.isOn)
            {
                UpdateLog(message);
                UpdateConsole();
            }
        }

        private void ClearLog()
        {
            consoleText.text = String.Empty;
        }

        private void UpdateLog(string message)
        {
            if (_consoleLineCount > _maxConsoleLines)
            {
                _consoleLogList.RemoveAt(0);
            }

            _consoleLogList.Add(message);
            _consoleLineCount++;
        }

        private void UpdateConsole()
        {
            ClearLog();

            for (int i = 0; i < _consoleLogList.Count; i++)
            {
                consoleText.text += _consoleLogList[i] + "\n";
            }
        }

        #endregion
        
    }
}
