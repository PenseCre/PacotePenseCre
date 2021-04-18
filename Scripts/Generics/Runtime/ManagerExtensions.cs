using System.Linq;
using UnityEngine;

namespace PacotePenseCre.Generics
{
    public static class ManagerExtensions
    {
        /// <summary>
        /// Sort the array of managers by their <see cref="LoadOrder"/>, from 0 to float.max,<br></br>
        /// leaving the ones with default <see cref="isDefaultLoadOrder"/> values at the end.
        /// </summary>
        /// <param name="managersInTheScene">Array of Managers in the scene</param>
        /// <returns>Sorted array</returns>
        public static Manager[] Sort(this Manager[] managersInTheScene)
        {
            if (managersInTheScene == null || managersInTheScene.Length == 0)
            {
                Debug.LogWarning("No managers in the scene found to sort.");
                return null;
            }

            return managersInTheScene
                .Where(x => !x.IsDefaultLoadOrder)
                .OrderBy(x => x.LoadOrder)
                .Concat(managersInTheScene.Where(x => x.IsDefaultLoadOrder))
                .ToArray();
        }
    }
}