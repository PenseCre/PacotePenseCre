using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacotePenseCre.Generics;

namespace PacotePenseCre.UI
{
    public class WindowManager : Singleton<WindowManager>
    {
        private Dictionary<string, UIScreen> _registeredWindows = new Dictionary<string, UIScreen>();

        public void RegisterWindow(UIScreen window)
        {
            if (!_registeredWindows.ContainsValue(window) && !_registeredWindows.ContainsValue(window))
            {
                _registeredWindows.Add(window.GetType().Name, window);
            }
            else
            {
                Debug.Log("Window Key OR Value has already been registered");
            }
        }

        public void ShowWindow(string windowName, bool animate = true)
        {
            if (!IsRegistered(windowName))
                return;

            _registeredWindows[windowName].ShowScreen(animate);
        }

        public void HideWindow(string windowName, bool animate = true)
        {
            if (!IsRegistered(windowName))
                return;

            _registeredWindows[windowName].HideScreen(animate);
        }

        private bool IsRegistered(string windowName)
        {
            return _registeredWindows.ContainsKey(windowName);
        }

    }
}