using UnityEngine;
using System.Linq;
using System.Reflection;

using PacotePenseCre;
using PacotePenseCre.Configuration;
using PacotePenseCre.Utility;
using PacotePenseCre.Input;
using PacotePenseCre.Generics;

namespace PacotePenseCre.Core
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        internal static Assembly proj;

        void Awake()
        {
            AddDependencies();
        }

        void Start()
        {
            UnityDefaults();
        }

        private void UnityDefaults()
        {
            Cursor.visible = Application.isEditor;
        }

        private void AddDependencies()
        {
            // run configuration manager to setup all configs
            gameObject.AddComponent<ConfigurationManager>();

            // add security manager 
            gameObject.AddComponent<SecurityManager>();

            // add shortcut script dynamically
            gameObject.AddComponent<Shortcuts>();
        }

        /// <summary>
        /// Add in the project that are child of ConfigurationManager
        /// </summary>
        public void AddProjectDependencies_Instance()
        {
            proj = Assembly.GetCallingAssembly();
            AddProjectDependencies(gameObject, proj);
        }

        /// <summary>
        /// Add in the project that are child of ConfigurationManager
        /// </summary>
        public static void AddProjectDependencies_Static()
        {
            proj = Assembly.GetCallingAssembly();
            AddProjectDependencies(ApplicationManager.Instance.gameObject, proj);
        }

        private static void AddProjectDependencies(GameObject go, Assembly callingAssembly)
        {
            string callingFrom = string.Format("[{0}] - ", go.name);

            // automatically load any resource files inside folder Resources/PenseCre_ProjectDependencies
            var resources = Resources.LoadAll(string.Format("{0}{1}", "PenseCre_ProjectDependencies", System.IO.Path.DirectorySeparatorChar));
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i])
                {
                    Debug.Log(string.Format("{0}{1} was intanced from resources dependencies folder", callingFrom, resources[i].name));
                    var instance = Instantiate(resources[i]);
                    instance.name = resources[i].name;
                }
            }

            // automatically add any class in the context of who called this function that is child of ConfigurationManager
            // The way contexts work prevent an external package like this from accessing the application scripts
            var subclassTypes = callingAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ConfigurationManager)));

            if (subclassTypes != null && subclassTypes.Count() > 0)
            {
                foreach (System.Type t in subclassTypes)
                {
                    Debug.Log(string.Format("{0}{1} component instanced in runtime", callingFrom, t.Name));
                    go.AddComponent(t);
                }
            }
            else
            {
                Debug.Log(string.Format("{0} could not find any ConfigurationManager in project", callingFrom));
            }
        }
    }
}
