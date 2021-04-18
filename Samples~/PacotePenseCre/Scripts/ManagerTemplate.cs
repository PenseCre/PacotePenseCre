using System;
using System.Collections;
using UnityEngine;
using PacotePenseCre.Generics;

/// <summary>
/// Use this as a template as to how to make a manager class in a new file.
/// </summary>
public class ManagerTemplate : Manager
{
    // Only copy this when you need to use your manager as if it was a singleton. (plot twist: It won't be!)
    public static ManagerTemplate Instance
    {
        get
        {
            lock (_lock)
            {
                if (!registry.ContainsKey(typeof(ManagerTemplate)))
                {
                    var found = FindObjectOfType<ManagerTemplate>();
                    if (found != null)
                    {
                        registry.Add(found.GetType(), found);
                        return found;
                    }
                    var ret = new GameObject(typeof(ManagerTemplate).Name).AddComponent<ManagerTemplate>();
                    return ret;
                }
                return (ManagerTemplate)registry[typeof(ManagerTemplate)];
            }
        }
    }

    protected override int loadOrder => 1;

    /// <summary>
    /// Called from ApplicationManager
    /// </summary>
    public override IEnumerator Init()
    {
        // init my stuff here

        return base.Init();
    }

    public void MyTemplateFunc()
    {
        throw new NotImplementedException();
        // This is how you can call this function from elsewhere in the code.
        // ManagerTemplate.Instance.MyTemplateFunc();
    }
}
