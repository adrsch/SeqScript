// MIT License 

using Newtonsoft.Json;

namespace SEQ.Script.Core
{

    public static class Loc
    {
        static Dictionary<string, string> Lookup;
        static Dictionary<string, Action<string>> Bindings = new Dictionary<string, Action<string>>();
        public static string CurrentLocale;
        public static void Reset()
        {
            Lookup = null;
            Bindings.Clear();
            CurrentLocale = "";
        }

        public static async Task Locale(string loc)
        {
            CurrentLocale = loc;
            /* var path = $"{Application.dataPath}/Lang/{loc}.json";
             if (!File.Exists(path))
             {
                 Logger.Log(Channel.Shell, LogPriority.Error, $"Could not find localizer file {loc} at path {path}");
                 return;
             }*/
          //  Task.Run(async () =>
            //{
                var json = await FileUtil.ReadDataFileAsync($"Lang/{loc}.json");
                Lookup = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            //}).Start();
            foreach (var kvp in Bindings)
            {
                kvp.Value?.Invoke(Get(kvp.Key));
            }
        }

        public static bool Exists(string key)
        {
            return Lookup.ContainsKey(key);
        }

        public static string Get(string key)
        {
            if (Lookup == null)
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Localizer: Called with null localizer key");
                return "";
            }

            if (Lookup.TryGetValue(key, out var val))
                return val;

            Logger.Log(Channel.General, LogPriority.Error, $"Localizer key {key} not found in current locale {CurrentLocale}");
            return "";
        }

        public static void Bind(string key, Action<string> onLoadLocale)
        {
            if (Bindings.ContainsKey(key))
                Bindings[key] += onLoadLocale;
            else
                Bindings[key] = onLoadLocale;
            if (Lookup != null)
            {
                onLoadLocale?.Invoke(Get(key));
            }
        }
    }
}