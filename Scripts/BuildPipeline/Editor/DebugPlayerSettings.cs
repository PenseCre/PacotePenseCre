using UnityEditor;
using UnityEngine;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class DebugPlayerSettings : WindowsPlayerSettings
    {
        public override void ApplySettings(string applicationName, string companyName)
        {
            SetCommonSettings(applicationName, companyName);
            SetIcons();

            // Resolution Settings Panel
#if UNITY_2018_1_OR_NEWER
            PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
#else
            PlayerSettings.defaultIsFullScreen = true;
#endif

            PlayerSettings.runInBackground = true;

            // Standalone Player Settings Panel
            PlayerSettings.captureSingleScreen = false;
#if UNITY_2019_1_OR_NEWER
#else
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
#endif
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.visibleInBackground = false;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.forceSingleInstance = false;

            //Splash 
            PlayerSettings.SplashScreen.show = true;

        }
    }
}
