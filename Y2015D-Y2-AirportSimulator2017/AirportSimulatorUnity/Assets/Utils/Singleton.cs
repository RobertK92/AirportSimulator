using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                T searchResult = Object.FindObjectOfType<T>();
                if (searchResult != null)
                {
                    _instance = searchResult;
                }
                else
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}

