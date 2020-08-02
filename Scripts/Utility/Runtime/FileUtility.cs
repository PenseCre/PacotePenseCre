using System;
using System.IO;
using UnityEngine;

namespace PacotePenseCre.Scripts.Utility
{
    /// <summary>
    /// Class to handle file operations
    /// </summary>
    public class FileUtility
    {
        public static string ReadFile(string filePath)
        {
            try
            {
                StreamReader streamReader = new StreamReader(filePath);

                string streamText = streamReader.ReadToEnd();
                streamReader.Close();

                return streamText;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return String.Empty;
            }
        }

        public static void WriteFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static bool CheckFileExists(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Exists && fileInfo != null;
        }
    }
}
