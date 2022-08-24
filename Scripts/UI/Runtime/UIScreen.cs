using UnityEngine;
using PacotePenseCre.Utility;

namespace PacotePenseCre.UI
{
    [RequireComponent(typeof (CanvasGroup))]
    [System.Serializable]
    public class UIScreen : MonoBehaviour
    {
        public bool isVisible { get; set; }

        private CanvasGroup _canvasGroup;
        private float _fadeSpeed = 1.0f;
        [SerializeField] private bool _toggleBlockRaycasts = true;
        [SerializeField] private bool _hideCursor = true;

        void Awake()
        {
            // self registering 
            WindowManager.Instance.RegisterWindow(this);
            _canvasGroup = GetComponent<CanvasGroup>();
            isVisible = false;
            HideScreen(false);
        }

        private void OnDestroy()
        {
            WindowManager.Instance.UnRegisterWindow(this);
        }

        public void ShowScreen(bool animate = true)
        {
            if (!animate)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
            }
            else
            {
                StartCoroutine(AnimationUtility.LerpCanvasAlpha(_canvasGroup, 0, 1, _fadeSpeed, () => MakeScreenBlockRaycasts(true)));
            }

            if (_hideCursor)
            {
                Cursor.visible = true;
            }
            isVisible = true;
        }

        public void HideScreen(bool animate = true)
        {
            if (!animate)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                StartCoroutine(AnimationUtility.LerpCanvasAlpha(_canvasGroup, 1, 0, _fadeSpeed, () => MakeScreenBlockRaycasts(false)));
            }

            if (_hideCursor)
            {
                Cursor.visible = false;
            }
            isVisible = false;
        }

        public void MakeScreenBlockRaycasts(bool blockRaycasts)
        {
            if (!_toggleBlockRaycasts) return;
            _canvasGroup.blocksRaycasts = blockRaycasts;
        }
    }
}
