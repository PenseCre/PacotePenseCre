using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacotePenseCre.BuildPipeline
{
    /// <summary>
    /// See <see cref="UnityEditor.PlayerSettings"/>
    /// </summary>
    [Serializable] //[BuildSetting]
    public class BuildSetting
    {
        [SerializeField] private string key;
        [SerializeField] private BuildSettingSupported type;
        [SerializeField] private bool _bool;
        [SerializeField] private float _float;
        [SerializeField] private int _int;
        [SerializeField] private string _string;
        [SerializeField] private Texture2D _texture2D;
        [SerializeField] private Vector2 _vector2;
        [SerializeField] private Vector3 _vector3;
        /// <summary>
        /// Used for editor property drawer serialization
        /// </summary>
        [SerializeField] private object value;

        public BuildSetting(string _key, object _value)
        {
            key = _key;
            value = _value;
            _bool = default;
            _float = default;
            _int = default;
            _string = default;
            _texture2D = default;
            _vector2 = default;
            _vector3 = default;

            var typeGot = _value.GetType();
            if (typeGot.IsAssignableFrom(typeof(bool)))
                type = BuildSettingSupported._bool;
            if (typeGot.IsAssignableFrom(typeof(float)))
                type = BuildSettingSupported._float;
            if (typeGot.IsAssignableFrom(typeof(int)))
                type = BuildSettingSupported._int;
            if (typeGot.IsAssignableFrom(typeof(string)))
                type = BuildSettingSupported._string;
            if (typeGot.IsAssignableFrom(typeof(Texture2D)))
                type = BuildSettingSupported._texture2D;
            if (typeGot.IsAssignableFrom(typeof(Vector2)))
                type = BuildSettingSupported._vector2;
            if (typeGot.IsAssignableFrom(typeof(Vector3)))
                type = BuildSettingSupported._vector3;

            switch (type) // could be replaced by system reflection
            {
                case BuildSettingSupported._bool:
                    _bool = (bool)_value;
                    break;
                case BuildSettingSupported._float:
                    _float = (float)_value;
                    break;
                case BuildSettingSupported._int:
                    _int = (int)_value;
                    break;
                case BuildSettingSupported._string:
                    _string = (string)_value;
                    break;
                case BuildSettingSupported._texture2D:
                    _texture2D = (Texture2D)_value;
                    break;
                case BuildSettingSupported._vector2:
                    _vector2 = (Vector2)_value;
                    break;
                case BuildSettingSupported._vector3:
                    _vector3 = (Vector3)_value;
                    break;
                default:
                    Debug.LogWarning("Unsupported build setting type. Falling back to string.");
                    _string = (string)_value;
                    break;
            }
        }

        /// <summary>
        /// Name of the setting
        /// </summary>
        public string Key => key;

        /// <summary>
        /// Type of the setting - only supported types are allowed here.<br></br>
        /// Enums are treated as strings and parsed accordingly where needed.
        /// </summary>
        public BuildSettingSupported Type => type;

        /// <summary>
        /// See <see cref="BuildSettingSupported"/> for supported types.
        /// Usage example with Enums:
        /// <code>
        /// var myEnumOutput = UnityEditor.PlayerSettings.colorSpace;
        /// if (Enum.TryParse((string)myBuildSetting.Value, out myEnumOutput)) UnityEditor.PlayerSettings.colorSpace = myEnumOutput;
        /// </code>
        /// </summary>
        public object Value
        {
            //Todo: replace switch with system reflection method to get 
            get
            {
                switch (type)
                {
                    case BuildSettingSupported._bool:
                        return _bool;
                    case BuildSettingSupported._float:
                        return _float;
                    case BuildSettingSupported._int:
                        return _int;
                    case BuildSettingSupported._string:
                        return _string;
                    case BuildSettingSupported._texture2D:
                        return _texture2D;
                    case BuildSettingSupported._vector2:
                        return _vector2;
                    case BuildSettingSupported._vector3:
                        return _vector3;
                    default:
                        return value;
                }
            }
        }
    }
        public enum BuildSettingSupported
        {
            _bool = 0,
            _float = 1,
            _int = 2,
            _string = 3,
            _texture2D = 4,
            _vector2 = 5,
            _vector3 = 6,
        }
}