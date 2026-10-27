using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;

using UnityEditor;
using UnityEditor.Build.Reporting;
using PacotePenseCre.BuildPipeline;

namespace PacotePenseCre.Editor.BuildPipeline
{
    public class Build : MonoBehaviour
    {
        public delegate void BuildCallback();

        public static bool RunBuild(string[] buildScenes, BuildInfo buildInfo, BuildConfig buildConfig, BuildTarget target, BuildOptions options, BuildCallback callback)
        {
            BuildZipOperation.EnsureCanBuild();
            string sceneName = buildScenes[0].Substring(buildScenes[0].LastIndexOf(@"/") + 1).Replace(".unity", "");
            string buildLocation = GetBuildDirectory(target, sceneName, buildInfo.Release);
            string buildFile = buildInfo.ApplicationName + ".exe";

            Debug.Log(string.Format("[BUILD] [{0}] Started @ {1:MM/dd/yy hh:mm:ss}", target.ToString(), DateTime.Now));

            CreateBuildFolder(buildLocation, buildFile, target);
            bool ret = BuildUnityProject(buildScenes, buildInfo, buildConfig, target, options, buildLocation, buildFile);

            Debug.Log(string.Format("[BUILD] [{0}] Completed @ {1:MM/dd/yy hh:mm:ss}.", target.ToString(), DateTime.Now));

            callback();
            return ret;
        }

        public static void DeleteAfterBuild(string[] buildScenes, BuildInfo buildInfo, BuildTarget target, string toDeleteString)
        {
            BuildZipOperation.EnsureCanBuild();
            if (string.IsNullOrWhiteSpace(toDeleteString)) return;

            string sceneName = buildScenes[0].Substring(buildScenes[0].LastIndexOf(@"/") + 1).Replace(".unity", "");
            string buildLocation = GetBuildDirectory(target, sceneName, buildInfo.Release);
            if (!Directory.Exists(buildLocation)) return;

            string dataFolder = Path.Combine(buildLocation, buildInfo.ApplicationName + "_Data");
            if (!Directory.Exists(dataFolder))
            {
                var dataDirs = Directory.GetDirectories(buildLocation, "*_Data");
                if (dataDirs.Length > 0)
                {
                    dataFolder = dataDirs[0];
                }
            }
            string streamingAssetsPath = Path.Combine(dataFolder, "StreamingAssets");

            var items = ParseQuotedItems(toDeleteString);
            string streamingToken = "[StreamingAssets]";

            foreach (var rawItem in items)
            {
                if (string.IsNullOrWhiteSpace(rawItem)) continue;

                string item = rawItem.Trim();
                bool isStreamingAssets = item.StartsWith(streamingToken, StringComparison.OrdinalIgnoreCase);

                string targetDir;
                string patternOrSubPath;

                if (isStreamingAssets)
                {
                    string sub = item.Substring(streamingToken.Length).TrimStart('/', '\\');
                    targetDir = streamingAssetsPath;
                    patternOrSubPath = sub;
                }
                else
                {
                    targetDir = buildLocation;
                    patternOrSubPath = item.TrimStart('/', '\\');
                }

                if (string.IsNullOrEmpty(patternOrSubPath))
                {
                    if (Directory.Exists(targetDir))
                    {
                        try
                        {
                            FileTools.MakeFilesWritable(targetDir);
                            Directory.Delete(targetDir, true);
                            Debug.Log(string.Format("[BUILD] Deleted directory: {0}", targetDir));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(string.Format("[BUILD] Failed to delete directory {0}: {1}", targetDir, ex.Message));
                        }
                    }
                    continue;
                }

                string fullPath = Path.Combine(targetDir, patternOrSubPath);
                bool hasWildcard = patternOrSubPath.Contains("[*]") || patternOrSubPath.Contains("*");

                if (!hasWildcard)
                {
                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            File.SetAttributes(fullPath, FileAttributes.Normal);
                            File.Delete(fullPath);
                            Debug.Log(string.Format("[BUILD] Deleted file: {0}", fullPath));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(string.Format("[BUILD] Failed to delete file {0}: {1}", fullPath, ex.Message));
                        }
                    }
                    else if (Directory.Exists(fullPath))
                    {
                        try
                        {
                            FileTools.MakeFilesWritable(fullPath);
                            Directory.Delete(fullPath, true);
                            Debug.Log(string.Format("[BUILD] Deleted directory: {0}", fullPath));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(string.Format("[BUILD] Failed to delete directory {0}: {1}", fullPath, ex.Message));
                        }
                    }
                }
                else
                {
                    string searchDir = Path.GetDirectoryName(fullPath);
                    string pattern = Path.GetFileName(fullPath);

                    if (!Directory.Exists(searchDir)) continue;

                    bool isBuildRoot = string.Equals(Path.GetFullPath(searchDir).TrimEnd('\\', '/'), Path.GetFullPath(buildLocation).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);

                    foreach (string file in Directory.GetFiles(searchDir, "*", SearchOption.TopDirectoryOnly))
                    {
                        string fileName = Path.GetFileName(file);
                        if (isBuildRoot && string.Equals(fileName, buildInfo.ApplicationName + ".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (MatchesWildcard(fileName, pattern))
                        {
                            try
                            {
                                File.SetAttributes(file, FileAttributes.Normal);
                                File.Delete(file);
                                Debug.Log(string.Format("[BUILD] Deleted file: {0}", file));
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning(string.Format("[BUILD] Failed to delete file {0}: {1}", file, ex.Message));
                            }
                        }
                    }

                    foreach (string dir in Directory.GetDirectories(searchDir, "*", SearchOption.TopDirectoryOnly))
                    {
                        string dirName = Path.GetFileName(dir);
                        if (isBuildRoot && dirName.EndsWith("_Data", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (MatchesWildcard(dirName, pattern))
                        {
                            try
                            {
                                FileTools.MakeFilesWritable(dir);
                                Directory.Delete(dir, true);
                                Debug.Log(string.Format("[BUILD] Deleted directory: {0}", dir));
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning(string.Format("[BUILD] Failed to delete directory {0}: {1}", dir, ex.Message));
                            }
                        }
                    }
                }
            }
        }

        public static List<string> ParseQuotedItems(string input)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(input)) return list;

            int firstQuote = input.IndexOf('"');
            if (firstQuote == -1)
            {
                string trimmed = input.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    list.Add(trimmed);
                return list;
            }

            int start = -1;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '"')
                {
                    if (start == -1)
                    {
                        start = i + 1;
                    }
                    else
                    {
                        string item = input.Substring(start, i - start).Trim();
                        if (!string.IsNullOrEmpty(item))
                        {
                            list.Add(item);
                        }
                        start = -1;
                    }
                }
            }
            return list;
        }

        public static bool MatchesWildcard(string input, string pattern)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern))
                return false;

            string normalizedPattern = pattern.Replace("[*]", "*");
            if (!normalizedPattern.Contains("*"))
            {
                return string.Equals(input, normalizedPattern, StringComparison.OrdinalIgnoreCase);
            }

            string regexPattern = "^" + Regex.Escape(normalizedPattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>Schedules background archiving. Observe BuildZipOperation.Status for completion.</summary>
        public static void Zip(string[] buildScenes, BuildInfo buildInfo, BuildTarget target, BuildConfig buildConfig)
        {
            var requests = CreateZipRequests(buildScenes, buildInfo, target, buildConfig);
            if (requests.Count > 0) BuildZipOperation.Start(requests);
        }

        public static List<BuildZipRequest> CreateZipRequests(string[] buildScenes, BuildInfo buildInfo, BuildTarget target, BuildConfig buildConfig)
        {
            var requests = new List<BuildZipRequest>();
            if (buildConfig == null) throw new ArgumentNullException("buildConfig");
            if (!buildConfig.ArchiveToZip) return requests;
            if (target != BuildTarget.StandaloneWindows64) throw new NotSupportedException("This build tool currently supports Windows 64-bit ZIP output.");
            if (buildScenes == null || buildScenes.Length == 0 || buildInfo == null) throw new ArgumentException("ZIP creation requires scenes and build information.");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            int count = buildConfig.OneBuildPerScene ? buildScenes.Length : 1;
            for (int i = 0; i < count; i++)
            {
                string sceneName = Path.GetFileNameWithoutExtension(buildScenes[i].Replace('\\', '/'));
                string buildLocation = GetBuildDirectory(target, sceneName, buildInfo.Release);
                string appName = buildConfig.SkipApplicationName ? string.Empty : buildInfo.ApplicationName.Replace(" ", string.Empty) + "_";
                string scenePrefix = buildConfig.OneBuildPerScene ? sceneName + "_" : string.Empty;
                string fileName = appName + scenePrefix + "v" + buildInfo.GetVersionName + "_" + timestamp + ".zip";
                if (fileName != Path.GetFileName(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    throw new ArgumentException("Application/scene name cannot contain path separators or invalid filename characters.");
                string destinationFullPath = Path.Combine(Directory.GetParent(buildLocation).Parent.Parent.Parent.FullName, fileName);
                requests.Add(new BuildZipRequest(buildLocation, destinationFullPath, fileName));
            }
            return requests;
        }
        public static void PrepareInstallerScript(string[] buildScenes, BuildInfo buildInfo, BuildTarget target, BuildConfig buildConfig)
        {
            // .iss wip
        }

        public static void MakeInstaller(string[] buildScenes, BuildInfo buildInfo, BuildTarget target, BuildConfig buildConfig, string guid)
        {
            BuildZipOperation.EnsureCanBuild();
            if (!buildConfig.MakeInstaller) return;
            string script = BuildConfig.ExpandPath(buildConfig.InstallerScriptLocation);

            if (buildConfig.OneBuildPerScene)
            {
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    string sceneName = buildScenes[i].Substring(buildScenes[i].LastIndexOf(@"/") + 1).Replace(".unity", "");
                    string buildLocation = GetBuildDirectory(target, sceneName, buildInfo.Release);
                    string appName = buildConfig.SkipApplicationName ? string.Empty : buildInfo.ApplicationName.Replace(" ", string.Empty) + "_";
                    string fileName = appName + sceneName + "_v" + buildInfo.GetVersionName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                    string destinationFullPath = Path.Combine(Directory.GetParent(buildLocation).Parent.Parent.Parent.FullName, fileName + ".exe");
                    Debug.Log(string.Format("[BUILD] [{0}] Making installer {1} into {2}", target.ToString(), buildLocation, destinationFullPath));
                    
                    if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
                    {
                        var managedVariables = new InstallerScriptManagedVariables()
                        {
                            buildLocation = buildLocation,
                            buildLocation_relative = BuildConfig.ShortenPath(buildLocation, "../"),
                            destinationFullPath = destinationFullPath,
                            applicationName = buildInfo.ApplicationName,
                            versionName = buildInfo.GetVersionName,
                            companyName = buildInfo.CompanyName,
                            fileName = fileName,
                            guid = guid
                        };
                        Installer.CreateFromDirectory(script, managedVariables, buildConfig.OverwriteGuid);
                    }
                }
            }
            else
            {
                string sceneName = buildScenes[0].Substring(buildScenes[0].LastIndexOf(@"/") + 1).Replace(".unity", "");
                string buildLocation = GetBuildDirectory(target, sceneName, buildInfo.Release);
                string appName = buildConfig.SkipApplicationName ? string.Empty : buildInfo.ApplicationName.Replace(" ", string.Empty) + "_";
                string fileName = appName + "v" + buildInfo.GetVersionName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                string destinationFullPath = Path.Combine(Directory.GetParent(buildLocation).Parent.Parent.Parent.FullName, fileName + ".exe");
                Debug.Log(string.Format("[BUILD] [{0}] Making installer {1} into {2}", target.ToString(), buildLocation, destinationFullPath));

                if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
                {
                    var managedVariables = new InstallerScriptManagedVariables()
                    {
                        buildLocation = buildLocation,
                        buildLocation_relative = BuildConfig.ShortenPath(buildLocation, "../"),
                        destinationFullPath = destinationFullPath,
                        applicationName = buildInfo.ApplicationName,
                        versionName = buildInfo.GetVersionName,
                        companyName = buildInfo.CompanyName,
                        fileName = fileName,
                        guid = guid
                    };
                    Installer.CreateFromDirectory(script, managedVariables, buildConfig.OverwriteGuid);
                }
            }
        }

        private static string GetBuildDirectory(BuildTarget target, string sceneName, bool isRelease)
        {
            string buildLocation = String.Empty;
            if (target == BuildTarget.StandaloneWindows64)
            {
                buildLocation += "Builds/Windows/";
                buildLocation += isRelease ? "Release/" : "Debug/";
                buildLocation += sceneName + "/";
            }
            return Path.GetFullPath(buildLocation);
        }

        private static void CreateBuildFolder(string buildLocation, string buildFile, BuildTarget target)
        {
            DirectoryInfo dirBuild = new DirectoryInfo(buildLocation);

            // if the directory exists, delete it.
            if (dirBuild.Exists)
            {
                Debug.Log("[BUILD] Cleaning Directory.");

                FileTools.MakeFilesWritable(dirBuild);

                if (target == BuildTarget.StandaloneWindows64)
                {
                    if (File.Exists(buildFile))
                        File.Delete(buildFile);
                }
                else
                {
                    Directory.Delete(buildLocation, true);
                }
            }

            dirBuild.Create();

            Debug.Log("[BUILD] Created: " + Path.GetFullPath(buildLocation));
        }

        private static bool BuildUnityProject(string[] buildScenes, BuildInfo buildInfo, BuildConfig buildConfig, BuildTarget target, BuildOptions options, string buildLocation, string buildFile)
        {
            SetupPlayerSettings(buildInfo, buildConfig);
            BuildReport result = UnityEditor.BuildPipeline.BuildPlayer(buildScenes, buildLocation + buildFile, target, options);
            // error logs
            if (result && result.summary.totalErrors > 0)
            {
                Debug.LogError(result.summary.result);
                return false;
            }
            else
            {
                Debug.Log("[BUILD] Unity Build Successful.");
                return true;
            }
        }

        private static void SetupPlayerSettings(BuildInfo buildInfo, BuildConfig buildConfig)
        {
            if (!buildInfo.Release)
            {
                new DebugPlayerSettings().ApplySettings(buildInfo.ApplicationName, buildInfo.CompanyName, buildInfo.GetVersionName, buildConfig.buildSettings);
            }
            else
            {
                new ReleasePlayerSettings().ApplySettings(buildInfo.ApplicationName, buildInfo.CompanyName, buildInfo.GetVersionName, buildConfig.buildSettings);
            }
        }

        public static void ClearLog()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));

            Type type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}
