// MIT License

namespace SEQ.Script.Core
{
    public static class EventManager
    {
        static Dictionary<string, Action> StringActions = new Dictionary<string, Action>();
        static Dictionary<Type, object> GenericActions = new Dictionary<Type, object>();

        public static void Reset()
        {
            GenericActions.Clear();
            StringActions.Clear();
        }

        // Generics

        public static void AddListener<T>(Action<T> action)
        {
            if (!GenericActions.ContainsKey(typeof(T)))
                GenericActions[typeof(T)] = action;
            else
            {
                var evtAction = (Action<T>)GenericActions[typeof(T)];
                evtAction += action;
                GenericActions[typeof(T)] = evtAction;
            }
        }

        public static void RemoveListener<T>(Action<T> action)
        {
            if (GenericActions.TryGetValue(typeof(T), out var boxed))
            {
                var evtAction = (Action<T>)boxed;
                evtAction -= action;
                GenericActions[typeof(T)] = evtAction;
            }
        }

        public static void Raise<T>(T evt)
        {
            if (GenericActions.TryGetValue(typeof(T), out var boxed))
            {
                var evtAction = (Action<T>)boxed;
                evtAction?.Invoke(evt);
            }
        }

        // Strings

        public static void AddListener(string message, Action action)
        {
            if (!StringActions.ContainsKey(message))
                StringActions[message] = action;
            else
            {
                var evtAction = StringActions[message];
                evtAction += action;
                StringActions[message] = evtAction;
            }
        }

        public static void RemoveListener(string message, Action action)
        {
            if (StringActions.ContainsKey(message))
            {
                var evtAction = StringActions[message];
                evtAction -= action;
                StringActions[message] = evtAction;
            }
        }

        public static void Raise(string message)
        {
            if (StringActions.ContainsKey(message))
            {
                var evtAction = StringActions[message];
                evtAction?.Invoke();
            }
        }
    }
}
