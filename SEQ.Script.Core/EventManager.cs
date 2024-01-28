// MIT License

namespace SEQ.Script.Core
{
    public static class EventManager
    {
        static Dictionary<Type, object> Actions = new Dictionary<Type, object>();
        static Dictionary<string, Type> Types = new Dictionary<string, Type>();

        public static void Reset()
        {
            Actions.Clear();
            Types.Clear();
        }

        public static void AddListener<T>(Action<T> action)
        {
            Types[typeof(T).Name] = typeof(T);

            if (!Actions.ContainsKey(typeof(T)))
                Actions[typeof(T)] = action;
            else
            {
                var evtAction = (Action<T>)Actions[typeof(T)];
                evtAction += action;
                Actions[typeof(T)] = evtAction;
            }
        }

        public static void RemoveListener<T>(Action<T> action)
        {
            if (Actions.TryGetValue(typeof(T), out var boxed))
            {
                var evtAction = (Action<T>)boxed;
                evtAction -= action;
                Actions[typeof(T)] = evtAction;
            }
        }

        public static void Raise<T>(T evt)
        {
            if (Actions.TryGetValue(typeof(T), out var boxed))
            {
                var evtAction = (Action<T>)boxed;
                evtAction?.Invoke(evt);
            }
        }
    }
}
