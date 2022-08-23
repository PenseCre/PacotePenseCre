using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PacotePenseCre.BuildPipeline;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class DebugPlayerSettings : WindowsPlayerSettings
    {
        protected new readonly BuildSetting[] defaultSettings = new BuildSetting[]
        {
             new BuildSetting("runInBackground"      , true      )
            ,new BuildSetting("visibleInBackground"  , true      )
            ,new BuildSetting("usePlayerLog"         , true      )
            ,new BuildSetting("forceSingleInstance"  , false     )
            //,new BuildSetting("captureSingleScreen"  , false     )
#if !UNITY_2019_1_OR_NEWER
            ,new BuildSetting("displayResolutionDialog", ResolutionDialogSetting.Enabled)
#endif
        };

        public override void ApplySettings(string applicationName, string companyName, string version, BuildSetting[] buildSettings = null)
        {
            SetName(applicationName, companyName, version);
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
