using UnityEngine;

namespace PacotePenseCre.Configuration
{
    /// <summary>
    /// Types of hardware sensors that we can implement some initial setup and spawning depending on the type.
    /// This might be not the best way to do, so that's not in use yet.
    /// </summary>
    public class Sensor
    {
        public string Name;
        public string Version;
        public Type Type;
    }
    public enum Type { Camera, Microphone, Presence, Movement, InfraredRemote, Midi, Giroscope, Accelerometer }
}