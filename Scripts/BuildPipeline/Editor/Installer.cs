using System;
using System.IO;
using System.Diagnostics;

namespace PacotePenseCre.Editor.BuildPipeline
{
    /// <summary>
    /// Using Utilities~/InnoSetupPortable, create a windows installer executable for a given build using a given .iss script template
    /// </summary>
    public static class Installer
    {
        public const string DEFAULT_FILENAME_WITHOUT_EXTENSION = "Installer-InnoSetupCompilerScript";
        public const string DEFAULT_FILE_EXTENSION_WITHOUT_DOT = "iss";
        public const string DEFAULT_FILENAME_DESCRIPTION = "Inno Setup Files";
        public const string DEFAULT_FOLDER_NAME = "InstallerScript";
        
        public const string DEFAULT_FILE_EXTENSION = "." + DEFAULT_FILE_EXTENSION_WITHOUT_DOT;
        public const string DEFAULT_FILENAME_WITH_EXTENSION = DEFAULT_FILENAME_WITHOUT_EXTENSION + DEFAULT_FILE_EXTENSION;

        /// <summary>
        /// Maximum amount of seconds to wait for the installer to finish.
        /// </summary>
        public const int TIMEOUT = 120 * 1000;

        public static readonly string packageRoot = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Installer).Assembly).resolvedPath;
        public static readonly string innoSetupFolder = Path.Combine(packageRoot, "Utilities~", "InnoSetupPortable");
        //public static readonly string defaultInnoSetupScript = Path.GetFullPath(Path.Combine(packageRoot, DEFAULT_FOLDER_NAME, DEFAULT_FILENAME_WITH_EXTENSION));

        public static void CreateFromDirectory(string innoSetupScript, InstallerScriptManagedVariables managedVariables = null, bool overwriteGuid = true)
        {
            string innoSetupCommandLine = Path.Combine(innoSetupFolder, "ISCC.exe");
            if (!File.Exists(innoSetupCommandLine)) throw new FileNotFoundException("[Installer.CreateFromDirectory]: InnoSetup CommandLine (ISCC.exe) not found in " + innoSetupCommandLine);
            
            //if (!File.Exists(innoSetupScript)) innoSetupScript = defaultInnoSetupScript;
            if (!File.Exists(innoSetupScript)) throw new FileNotFoundException("[Installer.CreateFromDirectory]: InnoSetup Compiler Script not found in " + innoSetupScript);

            if(managedVariables != null) ManageScript(innoSetupScript, managedVariables, overwriteGuid);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = innoSetupCommandLine;// "cmd.exe";
            startInfo.Arguments = "\"" + innoSetupScript + "\"";// "/C md " + Path.Combine(Environment.GetLogicalDrives()[0], "Test");
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(TIMEOUT);
        }

        public static void WriteDefault(string installerScriptLocation)
        {
            if (File.Exists(installerScriptLocation))
            {
                return;
            }
            string template = Path.GetFullPath(Path.Combine(innoSetupFolder, "Examples", "PenseCreTemplate.iss"));
            if (File.Exists(template))
            {
                File.Copy(template, installerScriptLocation, false);

                // overwrite default guid from template with new one
                var lines = File.ReadAllLines(installerScriptLocation);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    string suffix = "\"";
                    string prefix;
                    if (line.StartsWith(prefix = "#define MyAppId \""))
                    {
                        lines[i] = prefix + "{{" + Guid.NewGuid().ToString() + "}}" + suffix;
                    }
                }
                File.WriteAllLines(installerScriptLocation, lines);
            }
        }

        public static void ManageScript(string script, InstallerScriptManagedVariables managedVariables = null, bool overwriteGuid = true)
        {
            if (managedVariables == null) return;
            
            //#define MyAppName "Project Think & Believe"
            //#define MyAppVersion "0.0.1"
            //#define MyAppPublisher "Pense & Cre"
            //#define MyAppExeName "Project Think & Believe.exe" 
            //#define InputDir "..\Builds\Windows\Release\Main"

            var lines = File.ReadAllLines(script);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                string suffix = "\"";
                string prefix;
                string managedVar;
                if (line.StartsWith(prefix = "#define MyAppName \"") && !string.IsNullOrEmpty(managedVar = managedVariables.applicationName))
                {
                    lines[i] = prefix + managedVar + suffix;
                    continue;
                }
                if (line.StartsWith(prefix = "#define MyAppVersion \"") && !string.IsNullOrEmpty(managedVar = managedVariables.versionName))
                {
                    lines[i] = prefix + managedVar + suffix;
                    continue;
                }
                if (line.StartsWith(prefix = "#define MyAppPublisher \"") && !string.IsNullOrEmpty(managedVar = managedVariables.companyName))
                {
                    lines[i] = prefix + managedVar + suffix;
                    continue;
                }
                if (line.StartsWith(prefix = "#define MyAppExeName \"") && !string.IsNullOrEmpty(managedVar = managedVariables.applicationName))
                {
                    lines[i] = prefix + managedVar + ".exe" + suffix;
                    continue;
                }
                if (line.StartsWith(prefix = "#define InputDir \"") && !string.IsNullOrEmpty(managedVar = managedVariables.buildLocation_relative))
                {
                    lines[i] = prefix + managedVar.Replace("/", "\\") + suffix;
                    continue;
                }
                if (line.StartsWith(prefix = "#define MyInstallerName \"") && !string.IsNullOrEmpty(managedVar = managedVariables.fileName))
                {
                    lines[i] = prefix + managedVar + suffix;
                    continue;
                }
                if (line.StartsWith(prefix = "#define MyAppId \"") && !string.IsNullOrEmpty(managedVar = managedVariables.guid) && overwriteGuid)
                {
                    lines[i] = prefix + "{{" + managedVar + "}}" + suffix;
                    continue;
                }
            }
            File.WriteAllLines(script, lines);
        }
    }

    public class InstallerScriptManagedVariables
    {
        public string buildLocation;
        public string buildLocation_relative;
        public string destinationFullPath;
        public string applicationName;
        public string versionName;
        public string companyName;
        public string fileName;
        public string guid;
    }
}