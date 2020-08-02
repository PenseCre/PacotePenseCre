using UnityEditor;
using UnityEngine;

namespace PacotePenseCre.Scripts.Editor.BuildPipeline
{
    public class ReleasePlayerSettings : WindowsPlayerSettings
    {
        public override void ApplySettings(string applicationName, string companyName)
        {
            SetCommonSettings(applicationName, companyName);
            SetIcons();

            // Resolution Settings 
            //PlayerSettings.defaultIsFullScreen = true;
            PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            PlayerSettings.runInBackground = true;

            // Standalone Player Settings 
            PlayerSettings.captureSingleScreen = false;
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            PlayerSettings.usePlayerLog = false;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.forceSingleInstance = true;

            //Splash 
            PlayerSettings.SplashScreen.show = false;

        }
    }
}
