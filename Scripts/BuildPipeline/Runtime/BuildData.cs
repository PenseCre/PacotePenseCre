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
    public abstract class BuildDataSO : ScriptableObject
    {
        /// <summary>
        /// Overwrite this for each child class implementation
        /// </summary>
        public static BuildDataSO DefaultValues;
        //public static BuildDataSO DefaultValues { get => CreateInstance<BuildDataSO>(); }

        /// <summary>
        /// Overwrite this for each child class implementation
        /// </summary>
        public static readonly string FileName = "buildDataSO.asset";

        /// <summary>
        /// Do not overwrite this
        /// </summary>
        public static readonly string BasePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.streamingAssetsPath, "..", "Data", "Editor"));
        public static readonly string BasePathRelative = "Assets" + System.IO.Path.AltDirectorySeparatorChar + "Data" + System.IO.Path.AltDirectorySeparatorChar + "Editor" + System.IO.Path.AltDirectorySeparatorChar;
    }
}