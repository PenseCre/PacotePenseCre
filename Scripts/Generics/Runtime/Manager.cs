using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PacotePenseCre.Generics
{
    /// <summary>
    /// Base class from which all the manager scripts should inherit from, for consistency, scalability and automatically loading. <br></br>
    /// This is not a singleton, but follow <see cref="ManagerTemplate"/> to see a working example of how to make a manager.
    /// </summary>
    public class Manager : MonoBehaviour, IManager
    {
        #region Registry
        protected static object _lock = new object();
        /// <summary>
        /// Dictionary with all the managers in the scene with their respective child type as key.<br></br>
        /// Only the first manager of each type will be here. If you have duplicates, the following copies won't be here
        /// </summary>
        public static Dictionary<Type, Manager> registry = new Dictionary<Type, Manager>();
        protected void Register()
        {
            lock (_lock)
                if (registry != null && !registry.ContainsKey(GetType())) registry.Add(GetType(), this);
        }
        private void Awake()
        {
            Register();
        }
        #endregion

        #region Load Order
        /// <summary>
        /// Use this to set the sequence of the app manager
        /// </summary>
        public int LoadOrder { get { return loadOrder; } }
        /// <summary>
        /// Use this to ignore the default load order on this manager
        /// </summary>
        public bool IsDefaultLoadOrder { get { return isDefaultLoadOrder; } }
        /// <summary>
        /// Use this to set the sequence of the app manager.
        /// </summary>
        protected virtual int loadOrder => 0;

        /// <summary>
        /// Use this to ignore the default load order on this manager
        /// </summary>
        protected virtual bool isDefaultLoadOrder => (loadOrder <= 0);
        #endregion

        public virtual IEnumerator Init()
        {
            yield return null;
        }
    }
}