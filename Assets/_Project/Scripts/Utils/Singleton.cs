using UnityEngine;

namespace GoldAndGoblins.Utils
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                // Unity fake-null: a destroyed object compares equal to null.
                // Lazy FindObjectOfType recovers from domain-reload / Awake-order quirks.
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                }
                return instance;
            }
            private set => instance = value;
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
