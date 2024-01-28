using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public enum SystemPref
    {
        None = 0,
        Fullscreen,
        RawInput,
        Verbose,
        Sensitivity,
        Vsync,
        Autobhop,
        FOV,
        VolumeMaster,
        VolumeSFX,
        VolumeMusic,
        VolumeUI,
        StickSensitivity,
    }

    public class SystemPrefsUpdatedEvent
    {

    }

    public static class SystemPrefsManager
    {
        public static bool PauseWrite;
        public static Dictionary<SystemPref, string> Prefs = new Dictionary<SystemPref, string>();
        public static Dictionary<Keybind, string> KeybindsMap = new Dictionary<Keybind, string>();

        public static float Sensitivity = 100f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTheSens() { return (38.115f / 0.9337f) * Sensitivity; }

        public static float StickSensitivity = 100f;
        public static void Reset()
        {
            PauseWrite = false;

            Prefs = new Dictionary<SystemPref, string>
            {
                {
                    SystemPref.Fullscreen,
                    "fullscreen true;"
                },
                {
                    SystemPref.RawInput,
                    "rawinput true;"
                },
                {
                    SystemPref.Verbose,
                    "verbose true;"
                },
                {
                    SystemPref.Vsync,
                    "vsync false;"
                },
                {
                    SystemPref.Sensitivity,
                    "sensitivity 0.9337;"
                },
                {
                    SystemPref.StickSensitivity,
                    "sticksensitivity 150;"
                },
                {
                    SystemPref.Autobhop,
                    "autobhop true;"
                },
                {
                    SystemPref.FOV,
                    "fov 110;"
                },
                {
                    SystemPref.VolumeMaster,
                    "volume-master 1;"
                },
                {
                    SystemPref.VolumeSFX,
                    "volume-sfx 1;"
                },
                {
                    SystemPref.VolumeUI,
                    "volume-ui 1;"
                },
                {
                    SystemPref.VolumeMusic,
                    "volume-music 1;"
                }
            };

            KeybindsMap = Keybinds.GetDefaultBinds();
        }

        public static void BeginReadingFromFile()
        {

            PauseWrite = true;
        }

        public static async Task EndReadingFromFile()
        {
            PauseWrite = false;
            await WriteToFile();
        }

        public static async Task WriteToFile()
        {
            await FileUtil.WriteUserPrefs(GetString());
            EventManager.Raise(new SystemPrefsUpdatedEvent());
        }
        static StringBuilder Sb = new StringBuilder();
        static string GetString()
        {
            Sb.Append("//////////////////\n");
            Sb.Append("begin-read-prefs;\n");
            Sb.Append("//////////////////\n");
            foreach (var kvp in Prefs)
            {
                Sb.Append(kvp.Value);
                Sb.Append('\n');
            }
            Sb.Append("//////////////////\n");

            foreach (var kvp in KeybindsMap)
            {
                Sb.Append(kvp.Value);
                Sb.Append('\n');
            }

            Sb.Append("//////////////////\n");
            Sb.Append("end-read-prefs;\n");
            Sb.Append("//////////////////\n");
            return Sb.ToString();
        }

        public static async Task LoadSystemPrefs()
        {
            var p = await FileUtil.GetUserPrefs();
            if (!string.IsNullOrWhiteSpace(p))
            {
                // hack
                Shell.Exec(p.Substring(4));
            }
            //ScriptRunner.RunnerEntity.Add(new LoadPrefsAsync());
        }

        public static async Task ResetSystemPrefs()
        {
            Reset();

            PauseWrite = true;
            foreach (var cmd in Prefs.Values)
            {
                await Shell.ExecAsync(cmd);
            }
            foreach (var kb in KeybindsMap.Values)
            {
               await Shell.ExecAsync(kb);
            }
            await EndReadingFromFile();
        }
        public static async Task Set(SystemPref pref, string cmd)
        {
            Prefs[pref] = cmd;
            if (!PauseWrite) await WriteToFile();
        }
        public static async Task SetBind(Keybind pref, string cmd)
        {
            KeybindsMap[pref] = cmd;
            if (!PauseWrite) await WriteToFile();
        }
    }

    public class LoadPrefsAsync : AsyncScript
    {
        public override async Task Execute()
        {
            var p = await FileUtil.GetUserPrefs();
            if (!string.IsNullOrWhiteSpace(p))
            {
                // hack
                Shell.Exec(p.Substring(4));
            }
        }
    }
}
