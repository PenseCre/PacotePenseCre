using UnityEngine;

namespace PacotePenseCre.Scripts.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class CalibrationConfiguration
    {
        public string Type;
        public string Description;
              
        public float ScreenWidth;
        public float ScreenHeight;
        public float DistanceFromGround;

        public Vector3 KinectPosition;
        public Vector3 KinectRotation;

        public float KinectDepth;
    }
}