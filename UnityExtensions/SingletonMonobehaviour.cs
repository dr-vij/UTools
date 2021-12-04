using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T mInstance;

        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var instances = FindObjectsOfType<T>();
                    if (instances.Length == 0)
                        Debug.LogError("Singltone object was not found");
                    else if (instances.Length > 1)
                        Debug.LogError("More then 1 singleton object was found");
                    else
                        mInstance = instances[0];
                }
                return mInstance;
            }
        }
    }
}