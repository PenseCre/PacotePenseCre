using Newtonsoft.Json;

namespace PacotePenseCre.Utility
{
    /// <summary>
    /// Class for handling Json Operations
    /// </summary>
    public class JsonUtility
    {
        public static T ConvertJsonToObject<T>(string txt)
        {
            return JsonConvert.DeserializeObject<T>(txt, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }

        public static string ConvertToJson(object obj, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, formatting);
        }
    }
}