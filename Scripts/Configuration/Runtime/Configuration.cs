using UnityEngine;

namespace PacotePenseCre.Configuration
{
    /// <summary>
    /// Set of parameters per project following the structure of our proposals presentation
    /// </summary>
    public class Configuration
    {
        public string Idea;
        public string Format;
        public string[] Functionalities;
        public string[] Interactions;
        
        public string Name;
        public string Description;

        public float ScreenWidth;
        public float ScreenHeight;
        public float DistanceFromGround;

        //public Sensor[] Sensors;
    }
}