namespace UTools
{
    public class Singleton<T> where T : class, new()
    {
        private static T m_Instance;

        public static T Instance => m_Instance ??= new T();
    }
}

