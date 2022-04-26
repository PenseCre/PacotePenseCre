using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public abstract class WindowsPlayerSettings : PenseCrePlayerSettings
    {
//        /// <summary>
//        /// Fullscreen Mode, Native Resolution, and Aspect Ratio
//        /// </summary>
//        protected void SetDisplay(FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow, AspectRatio[] acceptedAspectRatios = null, Vector2Int defaultScreenResolution = new Vector2Int(), bool allowFullScreenSwitch = true)
//        {
//            // Fullscreen Mode
//#if UNITY_2018_1_OR_NEWER
//            PlayerSettings.fullScreenMode = fullScreenMode;
//#else
//            PlayerSettings.defaultIsFullScreen = true;
//#endif

//            // Native Resolution
//            if (defaultScreenResolution != Vector2Int.zero && defaultScreenResolution.x > 16 && defaultScreenResolution.y > 16)
//            {
//                PlayerSettings.defaultIsNativeResolution = false;
//                PlayerSettings.defaultScreenWidth = defaultScreenResolution.x;
//                PlayerSettings.defaultScreenHeight = defaultScreenResolution.y;
//#if !UNITY_2019_1_OR_NEWER
//                PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
//#endif
//            }
//            else
//            {
//                PlayerSettings.defaultIsNativeResolution = true;
//            }

//            // Standalone Player Settings 
//            PlayerSettings.captureSingleScreen = false;
//            PlayerSettings.resizableWindow = false;
//            PlayerSettings.visibleInBackground = true;

//            PlayerSettings.allowFullscreenSwitch = allowFullScreenSwitch;

//            // Aspect ratio
//            if (acceptedAspectRatios != null)
//            {
//                PlayerSettings.SetAspectRatio(AspectRatio.AspectOthers, acceptedAspectRatios.Contains(AspectRatio.AspectOthers));
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect4by3, acceptedAspectRatios.Contains(AspectRatio.Aspect4by3));
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect5by4, acceptedAspectRatios.Contains(AspectRatio.Aspect5by4));
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by10, acceptedAspectRatios.Contains(AspectRatio.Aspect16by10));
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by9, acceptedAspectRatios.Contains(AspectRatio.Aspect16by9));
//            }
//            else
//            {
//                PlayerSettings.SetAspectRatio(AspectRatio.AspectOthers, true);
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect4by3, true);
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect5by4, true);
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by10, true);
//                PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by9, true);
//            }
//        }
    }
}
