using PacotePenseCre.BuildPipeline;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class ReleasePlayerSettings : WindowsPlayerSettings
    {
        protected new readonly BuildSetting[] defaultSettings = new BuildSetting[]
        {
             new BuildSetting("runInBackground"         ,true    )
            ,new BuildSetting("visibleInBackground"     ,true    )
            ,new BuildSetting("usePlayerLog"            ,false   )
            ,new BuildSetting("forceSingleInstance"     ,true    )
            //,new BuildSetting("captureSingleScreen"     ,true    )
        };

        public override void ApplySettings(string applicationName, string companyName, BuildSetting[] buildSettings = null)
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
