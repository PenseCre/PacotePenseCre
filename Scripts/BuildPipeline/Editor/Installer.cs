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
        public static void CreateFromDirectory(string buildLocation, string destinationArchiveFileName)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = @"/C md C:\Test/";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}