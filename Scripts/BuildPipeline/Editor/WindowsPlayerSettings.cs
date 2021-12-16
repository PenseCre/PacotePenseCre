using UnityEngine;

using UnityEditor;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public abstract class WindowsPlayerSettings
    {
        public abstract void ApplySettings(string applicationName, string companyName);

        protected void SetCommonSettings(string applicationName, string companyName)
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = applicationName;

            //PlayerSettings.d3d9FullscreenMode = D3D9FullscreenMode.FullscreenWindow;
            //PlayerSettings.d3d11FullscreenMode = D3D11FullscreenMode.FullscreenWindow;
            PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

            PlayerSettings.SetAspectRatio(AspectRatio.Aspect4by3, true);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect5by4, true);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by9, true);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by10, true);
            PlayerSettings.SetAspectRatio(AspectRatio.AspectOthers, true);
        }

        protected void SetIcons()
        {
            Texture2D[] icons = new Texture2D[7];

            icons[0] = Resources.Load("Icons/Icon_1024") as Texture2D;
            icons[1] = Resources.Load("Icons/Icon_512") as Texture2D;
            icons[2] = Resources.Load("Icons/Icon_256") as Texture2D;
            icons[3] = Resources.Load("Icons/Icon_128") as Texture2D;
            icons[4] = Resources.Load("Icons/Icon_48") as Texture2D;
            icons[5] = Resources.Load("Icons/Icon_32") as Texture2D;
            icons[6] = Resources.Load("Icons/Icon_16") as Texture2D;

            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, icons);
        }
    }
}
