using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PacotePenseCre.BuildPipeline;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public abstract class PenseCrePlayerSettings
    {
        /// <summary>
        /// See <see cref="PlayerSettings"/>
        /// </summary>
        protected readonly BuildSetting[] defaultSettings = new BuildSetting[]
        {
            new BuildSetting("runInBackground", true),
            new BuildSetting("forceSingleInstance", true)
        };
        //protected readonly Dictionary<string, object> defaultSettings = new Dictionary<string, object>()
        //{
        //    { "runInBackground", true }
        //    ,{ "forceSingleInstance", true }
        //};

        public abstract void ApplySettings(string applicationName, string companyName, BuildSetting[] buildSettings = null);

        protected void SetName(string applicationName, string companyName)
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = applicationName;
        }

        protected void SetBuildSettings(BuildSetting[] buildSettings)
        {
            if (buildSettings == null) return;

            var properties = typeof(PlayerSettings).GetProperties();
            if (properties == null)
            {
                Debug.LogError("[SetBuildSettings]: Could not get properties from PlayerSettings. Check Assembly Definition and project settings");
                return;
            }

            foreach (var buildSetting in buildSettings)
            {
                var prop = properties.FirstOrDefault(x => x.Name == buildSetting.Key); // pay attention to capitalization
                if (prop != null)
                {
                    prop.SetValue(null, buildSetting.Value);
                }
            }
        }

        /// <summary>
        /// Icons are expected to be in folder Resources/Icons, with textures named Icon_1024, all the way through Icon_16
        /// </summary>
        protected void SetIcons()
        {
            var icons = new List<Texture2D>();

            Texture2D myIcon;
            myIcon = Resources.Load<Texture2D>("Icons/Icon_1024");
            if (myIcon) icons.Add(myIcon);
            myIcon = Resources.Load("Icons/Icon_512") as Texture2D;
            if (myIcon) icons.Add(myIcon);
            myIcon = Resources.Load("Icons/Icon_256") as Texture2D;
            if (myIcon) icons.Add(myIcon);
            myIcon = Resources.Load("Icons/Icon_128") as Texture2D;
            if (myIcon) icons.Add(myIcon);
            myIcon = Resources.Load("Icons/Icon_48") as Texture2D;
            if (myIcon) icons.Add(myIcon);
            myIcon = Resources.Load("Icons/Icon_32") as Texture2D;
            if (myIcon) icons.Add(myIcon);
            myIcon = Resources.Load("Icons/Icon_16") as Texture2D;
            if (myIcon) icons.Add(myIcon);

            if (icons.Count > 0)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, icons.ToArray());
            }
        }
    }
}
