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
        public static readonly string packageRoot = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Installer).Assembly).resolvedPath;

        public static void CreateFromDirectory(string buildLocation, string destinationArchiveFileName)
        {
            string innoSetupCommandLine = Path.Combine(packageRoot, "Utilities~", "InnoSetupPortable", "ISCC.exe");
            if (!File.Exists(innoSetupCommandLine)) throw new FileNotFoundException("[Installer.CreateFromDirectory]: InnoSetup CommandLine (ISCC.exe) not found in " + innoSetupCommandLine);
            
            string innoSetupScript = Path.GetFullPath(Path.Combine(buildLocation, "..", "..", "..", "Installer-InnoSetupCompilerScript.iss"));
            if (!File.Exists(innoSetupScript)) throw new FileNotFoundException("[Installer.CreateFromDirectory]: InnoSetup Compiler Script not found in " + innoSetupScript);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = innoSetupCommandLine;// "cmd.exe";
            startInfo.Arguments = "\"" + innoSetupScript + "\"";// "/C md " + Path.Combine(Environment.GetLogicalDrives()[0], "Test");
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(5000);
        }
    }
}