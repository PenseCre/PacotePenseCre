using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class DebugPlayerSettings : WindowsPlayerSettings
    {
        protected new readonly Dictionary<string, object> defaultSettings = new Dictionary<string, object>()
        {
            { "runInBackground", true }
            ,{ "visibleInBackground", true }
            ,{ "usePlayerLog", true }
            ,{ "forceSingleInstance", false }
            ,{ "captureSingleScreen", false }
#if !UNITY_2019_1_OR_NEWER
            ,{ "displayResolutionDialog", ResolutionDialogSetting.Enabled }
#endif
        };

        public override void ApplySettings(string applicationName, string companyName, Dictionary<string, object> buildSettings = null)
        {
            SetName(applicationName, companyName);
            SetIcons();
            //SetDisplay();

            if (buildSettings == null) buildSettings = defaultSettings;
            SetBuildSettings(buildSettings);

            //Splash 
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;
        }
    }
}
