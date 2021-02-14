using UnityEngine;
using System.Collections;

namespace PacotePenseCre.Utility
{
    /// <summary>
    /// Class to handle Animation Coroutines
    /// </summary>
    public class AnimationUtility
    {
        public static IEnumerator LerpCanvasAlpha(CanvasGroup obj, float from, float to, float time)
        {
            float step = 0;
            for (float t = 0; t <= 1; t += Time.deltaTime / time)
            {
                step += Time.deltaTime / time;
                obj.alpha = Mathf.Lerp(from, to, step);

                yield return null;
            }

            obj.alpha = to;
        }
    }
}
