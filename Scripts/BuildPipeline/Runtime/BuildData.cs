using System.IO;
using UnityEngine;

namespace PacotePenseCre.BuildPipeline
{
    /// <summary>
    /// Base class from which build json files derive from
    /// </summary>
    public abstract class BuildData
    {
        /// <summary>
        /// Overwrite this for each child class implementation
        /// </summary>
        public static BuildData DefaultValues;
        /// <summary>
        /// Overwrite this for each child class implementation
        /// </summary>
        public static readonly string FileName = "buildData.json";
    }

    /// <summary>
    /// Base class from which build scriptable object files derive from
    /// </summary>
    ///<remarks>
    /// One day C# will support static fields in interfaces, and this will be the default c# verion in unity.<br></br>
    /// When that happens, this class can be refactored to be more tidy:<br></br>
    /// <code>
    /// public class BuildData_InCSharp6AndNewer : IBuildData_ForCSharp6AndNewer
    /// {
    ///     public static string FileName => "";
    /// }
    /// public class BuildDataSO_InCSharp6AndNewer : ScriptableObject, IBuildData_ForCSharp6AndNewer
    /// {
    ///     public static string FileName => "";
    /// }
    /// public interface IBuildData_ForCSharp6AndNewer
    /// {
    ///     public static abstract string FileName { get; }
    /// }
    /// </code>
    ///</remarks>
    public abstract class DataSO : ScriptableObject
    {
        /// <summary>
        /// Overwrite this for each child class implementation
        /// </summary>
        public static DataSO DefaultValues;
        //public static BuildDataSO DefaultValues { get => CreateInstance<BuildDataSO>(); }

        /// <summary>
        /// Overwrite this for each child class implementation
        /// </summary>
        public static readonly string FileName = "buildDataSO.asset";

        /// <summary>
        /// Editor/Build Project Root Folder - Do not overwrite this. Only works in desktop builds
        /// </summary>
        public static readonly string BaseEditorPathProject = Directory.GetParent(Application.streamingAssetsPath).Parent.FullName;
        /// <summary>
        /// Absolute path where scriptable objects will be created from this package - Do not overwrite this
        /// </summary>
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "Data", "Editor"));
        /// <summary>
        /// Path where scriptable objects will be created from this package, relative to Project Root - Do not overwrite this
        /// </summary>
        public static readonly string BasePathRelative = "Assets" + Path.AltDirectorySeparatorChar + "Data" + Path.AltDirectorySeparatorChar + "Editor" + Path.AltDirectorySeparatorChar;


        /// <summary>
        /// Shorten the input path by removing its project root folder (<see cref="DataSO.BaseEditorPathProject"/>) from it for easier reading.
        /// </summary>
        public static string ShortenPath(string input)
        {
            string ret = "" + input; // "" + prevent this from being a pointer (just in case)
            bool inputPathIsInsideDefaultBasePath = ret.Replace("/", "\\").StartsWith(BaseEditorPathProject.Replace("/", "\\"));
            if (inputPathIsInsideDefaultBasePath)
            {
                // chop the path's defaultBasePath from input path string
                ret = ret.Substring(BaseEditorPathProject.Length + 1, ret.Length - BaseEditorPathProject.Length - 1);
            }
            return ret;
        }

        /// <summary>
        /// Expand the input path adding the project root folder to it (<see cref="DataSO.BaseEditorPathProject"/>) where applicable (only if input doesn't exist).
        /// </summary>
        public static string ExpandPath(string input)
        {
            return ExpandPath(input, true);
        }
        /// <summary>
        /// Expand the input path adding the project root folder to it (<see cref="DataSO.BaseEditorPathProject"/>) where applicable.
        /// </summary>
        public static string ExpandPath(string input, bool checkFileExists)
        {
            string ret = "" + input; // "" + prevent this from being a pointer (just in case)
            bool inputPathIsInsideDefaultBasePath = ret.Replace("/", "\\").StartsWith(BaseEditorPathProject.Replace("/", "\\"));
            if (!inputPathIsInsideDefaultBasePath)
            {
                if (!checkFileExists || !File.Exists(ret))
                {
                    // append BaseEditorPathProject to beginning of the input path
                    ret = Path.Combine(BaseEditorPathProject, ret);
                }
            }
            return ret;
        }
    }
}