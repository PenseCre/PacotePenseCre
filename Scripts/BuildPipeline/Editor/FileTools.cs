using System.IO;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public static class FileTools
    {
        public static void MakeFilesWritable(string directory)
        {
            MakeFilesWritable(new DirectoryInfo(directory));
        }

        public static void MakeFilesWritable(DirectoryInfo directory)
        {
            if (!directory.Exists)
                return;

            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                file.IsReadOnly = false;
            }
        }
    }
}


