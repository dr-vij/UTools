using UnityEngine;

namespace UTools
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (((object)m_Instance) == null)
                {
                    var instances = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (instances.Length == 0)
                        Debug.LogError("Singleton object was not found");
                    else if (instances.Length > 1)
                        Debug.LogError("More then 1 singleton object was found");
                    else
                        m_Instance = instances[0];
                }

                return m_Instance;
            }
        }
    }
}