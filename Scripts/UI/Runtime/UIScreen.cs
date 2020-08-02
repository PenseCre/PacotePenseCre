using UnityEngine;
using PacotePenseCre.Scripts.Utility;

namespace PacotePenseCre.Scripts.UI
{
    [RequireComponent(typeof (CanvasGroup))]
    [System.Serializable]
    public class UIScreen : MonoBehaviour
    {
        public bool isVisible { get; set; }

        private CanvasGroup _canvasGroup;
        private float _fadeSpeed = 1.0f;

        void Awake()
        {
            // self registering 
            WindowManager.Instance.RegisterWindow(this);

            _canvasGroup = GetComponent<CanvasGroup>();
            isVisible = false;
            HideScreen(false);
        }

        public void ShowScreen(bool animate = true)
        {
            if (!animate)
            {
                _canvasGroup.alpha = 1;
            }
            else
            {
                StartCoroutine(AnimationUtility.LerpCanvasAlpha(_canvasGroup, 0, 1, _fadeSpeed));
            }

            Cursor.visible = true;
            isVisible = true;
        }

        public void HideScreen(bool animate = true)
        {
            if (!animate)
            {
                _canvasGroup.alpha = 0;
            }
            else
            {
                StartCoroutine(AnimationUtility.LerpCanvasAlpha(_canvasGroup, 1, 0, _fadeSpeed));
            }

            Cursor.visible = false;
            isVisible = false;
        }
    }
}
