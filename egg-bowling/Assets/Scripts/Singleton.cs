using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            // Try to find an existing object
            if (_instance == null)
            {
                var objs = FindObjectsOfType<T>();
                if (objs.Length > 0)
                {
                    _instance = objs[0];
                    if (objs.Length > 1)
                        Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }
                else
                {
                    var obj = new GameObject
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    _instance = obj.AddComponent<T>();
                }
            }


            return _instance;
        }
    }
}


public class SingletonPersistent<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set;  }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}