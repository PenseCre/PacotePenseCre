using System;
using System.Collections.Generic;

namespace PacotePenseCre.Generics
{
    public abstract class AppEvent
    {
        public string EventName { get; set; }
    }

    /// <summary>
    /// TODO: integrate this https://learn.unity.com/tutorial/create-a-simple-messaging-system-with-events?signup=true
    /// or find out how to use current manager and add documentation with examples
    /// </summary>
    public class EventManager : Manager
    {
        #region public variables

        public delegate void EventDelegate<in T>(T e) where T : AppEvent;

        public delegate void EventCreationDelegate<in T>(T e) where T : AppEvent;

        #endregion

        #region private variables

        private readonly Dictionary<Type, Delegate> _delegates = new Dictionary<Type, Delegate>();

        #endregion

        #region public methods

        public void AddEventListener<T>(EventDelegate<T> listener) where T : AppEvent
        {
            Delegate d;
            if (_delegates.TryGetValue(typeof(T), out d))
                _delegates[typeof(T)] = Delegate.Combine(d, listener);
            else
                _delegates[typeof(T)] = listener;
        }

        public void RemoveEventListener<T>(EventDelegate<T> listener) where T : AppEvent
        {
            Delegate d;
            if (!_delegates.TryGetValue(typeof(T), out d))
                return;

            Delegate currentDel = Delegate.Remove(d, listener);
            if (currentDel == null)
                _delegates.Remove(typeof(T));
            else
                _delegates[typeof(T)] = currentDel;
        }

        public void RaiseEvent<T>(T e) where T : AppEvent
        {
            if (e == null)
                throw new ArgumentNullException("e");

            Delegate d;
            if (!_delegates.TryGetValue(typeof(T), out d))
                return;

            EventDelegate<T> callback = d as EventDelegate<T>;
            if (callback != null)
                callback(e);
        }

        #endregion
    }

}
