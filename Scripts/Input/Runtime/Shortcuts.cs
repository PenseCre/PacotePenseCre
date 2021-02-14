using UnityEngine;


namespace PacotePenseCre.Input
{
    using Input = UnityEngine.Input;
    public class Shortcuts : MonoBehaviour
    {
        private bool _hasQuit = false;

        void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                QuitApplication();
            }
        }

        private void QuitApplication()
        {
            if (!_hasQuit)
            {
                Application.Quit();
                _hasQuit = true;
            }
        }
    }
}