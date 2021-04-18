using UnityEngine;
using System.Linq;
using System.Collections;
using System.Reflection;

using PacotePenseCre.Configuration;
using PacotePenseCre.Input;
using PacotePenseCre.Generics;

namespace PacotePenseCre.Core
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        /// <summary>
        /// This is the assembly that called the package.<br></br>
        /// We assume this scope to be the whole project,<br></br>
        /// where we'll find all the managers to instantiate.
        /// </summary>
        internal static Assembly proj;

        #region Startup methods
        IEnumerator Start()
        {
            UnityDefaults();
            yield return LoadingRoutine();
        }

        private void UnityDefaults()
        {
            Cursor.visible = Application.isEditor;
        }
        #endregion

        #region Loading internal managers
        private IEnumerator LoadingRoutine()
        {
            // First, let's find the managers already in the scene
            var managers = (FindObjectsOfType<Manager>() as Manager[]);

            // Then let's get the internal managers from the package
            var execAssembly = Assembly.GetExecutingAssembly(); // this actually contains all the references, not just our package but also System, UnityEngine, etc
            var penseCreManagers = execAssembly
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(Manager)))
                ;

            Debug.Log("[ApplicationManager]: Internal PenseCreManagers count: " + penseCreManagers.Count());
            foreach (var item in penseCreManagers)
                Debug.Log("[ApplicationManager]: Internal PenseCreManager: " + item.Name);

            if (execAssembly != null)
            {
                string packageName = GetType().Namespace.Split('.').FirstOrDefault(); // our package uses this namespace
                if (string.IsNullOrEmpty(packageName)) packageName = "PacotePenseCre"; // but the method above may not always work
                
                // Let's go through the assemblies that belong to our package
                foreach (var referencedAssembly in execAssembly.GetReferencedAssemblies().Where(x => x.Name.Contains(packageName)))
                {
                    if (referencedAssembly != null)
                    {
                        Debug.Log("[ApplicationManager]: Looking for more managers in " + referencedAssembly.Name);
                        // add all the manager classes to our list
                        penseCreManagers = penseCreManagers.Concat(Assembly.Load(referencedAssembly)
                            .GetTypes()
                            .Where(x => x.IsSubclassOf(typeof(Manager)) && // get only the manager classes
                                   managers.Count(y => y.GetType() == x) == 0) // only if it's not already in the scene
                            );
                    }
                }
            }

            //  These are the managers of our internal package that are not yet on the scene
            if (penseCreManagers != null)
            {
                Debug.Log("[ApplicationManager]: Internal PenseCreManagers to add to the scene: " + penseCreManagers.Count());

                // Go through each type
                foreach (var item in penseCreManagers)
                {
                    Debug.Log($"[ApplicationManager]: {item}\nName: {item.Name}  Type: {item.GetType()}");
                 
                    // Add the type as a component in the scene and store a reference in an array
                    managers = managers.Append(gameObject.AddComponent(item) as Manager).ToArray();
                }
            }

            if (managers != null)
            {
                // Sort the instanced components
                managers = managers.Sort();

                // loop through the managers sorted in order from 1 to float.max
                // and the ones without any order overwrite (default loadOrder -1) will be at the end
                foreach (var manager in managers)
                {
                    Debug.Log($"[ApplicationManager]: {manager}\nLoadOrder: {manager.LoadOrder}  Type: {manager.GetType()}   Interface: {manager.GetType().GetInterface(typeof(IManager).Name, true)}");
                    // Initialize them in order
                    yield return manager.Init();
                }
            }
            //onLoadingComplete?.Invoke();

            yield return null;
        }
        #endregion

        #region Adding project dependencies
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

        /// <summary>
        /// Add in the project that are child of <see cref="Manager"/>
        /// </summary>
        public static void AddProjectDependencies_Static(Assembly _proj)
        {
            proj = _proj;
            AddProjectDependencies(ApplicationManager.Instance.gameObject, proj);
        }

        private static void AddProjectDependencies(GameObject go, Assembly callingAssembly)
        {
            string callingFrom = "[ApplicationManager]: ["+ go.name + "] - ";

            // automatically load any resource files inside folder Resources/PenseCre_ProjectDependencies
            var resources = Resources.LoadAll(System.IO.Path.DirectorySeparatorChar + System.IO.Path.DirectorySeparatorChar + "PenseCre_ProjectDependencies");
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i])
                {
                    Debug.Log(callingFrom + resources[i].name + " was intanced from resources dependencies folder");
                    if (Manager.registry.ContainsKey(resources[i].GetType())) {
                        Debug.Log(resources[i].GetType() + " already instantiated. Skipping.");
                        continue; 
                    }
                    var instance = Instantiate(resources[i]);
                    instance.name = resources[i].name;
                }
            }

            // automatically add any class in the context of who called this function that is child of Manager
            // The way contexts work prevent an external package like this from accessing the application scripts
            var subclassTypes = callingAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Manager)));

            if (subclassTypes != null && subclassTypes.Count() > 0)
            {
                foreach (System.Type t in subclassTypes)
                {
                    if(FindObjectOfType(t))
                    {
                        Debug.Log("[ApplicationManager]: " + callingFrom + t.Name + " component detected in the scene");
                        continue;
                    }
                    Debug.Log("[ApplicationManager]: " + callingFrom + t.Name + " component instanced in runtime");
                    go.AddComponent(t);
                }
            }
            else
            {
                Debug.Log("[ApplicationManager]: " + callingFrom + " could not find any Manager in project");
            }
        }
        #endregion
    }
}
