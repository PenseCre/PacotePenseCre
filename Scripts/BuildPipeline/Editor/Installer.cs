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

        public static void CreateFromDirectory(string buildLocation, string destinationArchiveFileName, string innoSetupScript = "")
        {
            string innoSetupCommandLine = Path.Combine(innoSetupFolder, "ISCC.exe");
            if (!File.Exists(innoSetupCommandLine)) throw new FileNotFoundException("[Installer.CreateFromDirectory]: InnoSetup CommandLine (ISCC.exe) not found in " + innoSetupCommandLine);
            
            //if (!File.Exists(innoSetupScript)) innoSetupScript = defaultInnoSetupScript;
            if (!File.Exists(innoSetupScript)) throw new FileNotFoundException("[Installer.CreateFromDirectory]: InnoSetup Compiler Script not found in " + innoSetupScript);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = innoSetupCommandLine;// "cmd.exe";
            startInfo.Arguments = "\"" + innoSetupScript + "\"";// "/C md " + Path.Combine(Environment.GetLogicalDrives()[0], "Test");
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(TIMEOUT);
        }

        public static void MatchConfig()
        {
            //todo: change define fields of the installer script to match with build config data
        }

        public static bool InvalidScript(string installerScriptLocation)
        {
            // todo: check define fields and compare to build config data
            return string.IsNullOrEmpty(installerScriptLocation) || !File.Exists(installerScriptLocation);
        }

        public static void WriteDefault(string installerScriptLocation)
        {
            string template = Path.GetFullPath(Path.Combine(innoSetupFolder, "Examples", "PenseCreTemplate.iss"));
            if (File.Exists(template))
            {
                File.Copy(template, installerScriptLocation, false);
            }
        }
    }
}