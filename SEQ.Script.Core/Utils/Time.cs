// MIT License 

namespace SEQ.Script.Core
{
    // TODO: This is just for porting from unity
    public interface ITimeProvider
    {
       public float clockTime { get; set; }
        public float deltaTime { get; }
        public float time { get; }
        public float unscaledTime { get; }
        public float unscaledDeltaTime { get; }
    }
    public static class Time
    {
        public static ITimeProvider Provider;
        public static float clockTime { get => Provider.clockTime; set => Provider.clockTime = value; }
        public static float deltaTime => Provider.deltaTime;
        public static float time => Provider.time;
        public static float unscaledTime => Provider.unscaledTime;
        public static float unscaledDeltaTime => Provider.unscaledDeltaTime;
    }
}

