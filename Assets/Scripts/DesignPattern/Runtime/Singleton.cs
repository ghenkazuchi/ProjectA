using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaKien
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] objs = FindObjectsOfType<T>();
                    if (objs.Length > 0)
                    {
                        T instance = objs[0];
                        _instance = instance;
                    }
                    else
                    {
                        GameObject go = new GameObject()    ;
                        go.name = typeof(T).Name;
                        _instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                    }

                }

                return _instance;
            }
        }
    }
}
//using UnityEngine;

//namespace HaKien
//{
//	public class Singleton<T> : MonoBehaviour where T : Component
//	{
//		private static T _instance;

//		public static T Instance
//		{
//			get
//			{
//				if (_instance == null)
//				{
//					Debug.LogError($"{typeof(T).Name} Singleton instance is null. Make sure an object with the {typeof(T).Name} script exists in your starting scene.");
//				}
//				return _instance;
//			}
//		}

//		protected virtual void Awake()
//		{
//			if (_instance == null)
//			{
//				_instance = this as T;

//				DontDestroyOnLoad(this.gameObject);
//			}
//			else if (_instance != this)
//			{
//				Debug.LogWarning($"Duplicate {typeof(T).Name} Singleton detected. Destroying the new one.");
//				Destroy(gameObject);
//			}
//		}
//	}
//}