using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class ReleasePlayerSettings : WindowsPlayerSettings
    {
        protected new readonly Dictionary<string, object> defaultSettings = new Dictionary<string, object>()
        {
            { "runInBackground", true }
            ,{ "visibleInBackground", true }
            ,{ "usePlayerLog", false }
            ,{ "forceSingleInstance", true }
            //,{ "captureSingleScreen", true }
        };

        public override void ApplySettings(string applicationName, string companyName, Dictionary<string, object> buildSettings = null)
        {
            SetName(applicationName, companyName);
            SetIcons();
            //SetDisplay();

            if (buildSettings == null) buildSettings = defaultSettings;
            SetBuildSettings(buildSettings);

            //Splash 
            if (PlayerSettings.advancedLicense)
            {
                PlayerSettings.SplashScreen.show = false;
                PlayerSettings.SplashScreen.showUnityLogo = false;
            }
        }
    }
}
