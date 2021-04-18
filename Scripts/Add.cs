using System.Reflection;
/// <summary>
/// Base namespace used as a shortcut or factory, to add project dependencies.
/// </summary>
namespace PacotePenseCre
{
    public class Add
    {
        /// <summary>
        /// This will add all the UI, Managers and things you need to get started with PenseCre framework.
        /// </summary>
        public static void Core()
        {
            var proj = Assembly.GetCallingAssembly();
            PacotePenseCre.Core.ApplicationManager.AddProjectDependencies_Static(proj);
        }
    }
}