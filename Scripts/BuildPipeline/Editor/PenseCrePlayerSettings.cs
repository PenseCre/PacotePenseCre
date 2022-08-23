using System;
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

        public abstract void ApplySettings(string applicationName, string companyName, string version, BuildSetting[] buildSettings = null);

        protected void SetName(string applicationName, string companyName, string version)
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = applicationName;
            PlayerSettings.bundleVersion = version;
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
                var prop = properties.FirstOrDefault(x => x.Name.ToLower() == buildSetting.Key.ToLower());
                if (prop == null)
                {
                    Debug.LogError("[SetBuildSettings]: Could not find a property from PlayerSettings named " + buildSetting.Key);
                    continue;
                }

                object val = buildSetting.Value;

                if (prop.PropertyType.IsEnum)
                {
                    // Here we don't know exactly what enum is it. Therefore we can't use Enum.TryParse<MyEnumType>();
                    // So below we implement equivalent functionality using enum helper functions.
                    try
                    {
                        var enumUnderlyingType = prop.PropertyType;
                        var enumValues = Enum.GetValues(prop.PropertyType);
                        for (int i = 0; i < enumValues.Length; i++)
                        {
                            var converted = Convert.ChangeType(enumValues.GetValue(i), enumUnderlyingType).ToString().ToLower();
                            if (converted == ((string)val).ToLower())
                            {
                                val = enumValues.GetValue(i);
                                break;
                            }
                        }
                        val = Convert.ChangeType(val, Enum.GetUnderlyingType(prop.PropertyType));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.StackTrace);
                        continue; // skip setting it up, because we clearly did not get a valid value
                    }
                }

                prop.SetValue(null, val);
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
