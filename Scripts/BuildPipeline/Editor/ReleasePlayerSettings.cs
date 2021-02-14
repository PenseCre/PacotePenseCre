using UnityEditor;
using UnityEngine;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class ReleasePlayerSettings : WindowsPlayerSettings
    {
        public override void ApplySettings(string applicationName, string companyName)
        {
            SetCommonSettings(applicationName, companyName);
            SetIcons();

            // Resolution Settings 
#if UNITY_2018_1_OR_NEWER
            PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
#else
            PlayerSettings.defaultIsFullScreen = true;
#endif
            PlayerSettings.runInBackground = true;

            // Standalone Player Settings 
            PlayerSettings.captureSingleScreen = false;
#if UNITY_2019_1_OR_NEWER
#else
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
#endif
            PlayerSettings.usePlayerLog = false;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.forceSingleInstance = true;

            //Splash 
            PlayerSettings.SplashScreen.show = false;

        }
    }
}
