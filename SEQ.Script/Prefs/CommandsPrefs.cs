using Stride.Audio;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public static class CommandsPrefs
    {
        public static Dictionary<string, CommandInfo> Commands => new Dictionary<string, CommandInfo>
        {
            {
                "begin-read-prefs", new CommandInfo
                {
                    Exec = async _ => SystemPrefsManager.BeginReadingFromFile(),
                }
            },
            {
                "end-read-prefs", new CommandInfo
                {
                    Exec = async _ => await SystemPrefsManager.EndReadingFromFile(),
                }
            },
            {
                "load-system-prefs", new CommandInfo
                {
                    Exec = async _ => await SystemPrefsManager.LoadSystemPrefs(),
                    Help = "loads default prefs and user if exists",
                }
            },
            {
                "reset-system-prefs", new CommandInfo
                {
                    Exec = async _ => await SystemPrefsManager.ResetSystemPrefs(),
                    Help = "Resets system prefs",
                }
            },
            {
                "bind", new CommandInfo
                {
                    Params = [typeof(string), typeof(string)],
                    Exec = async args => await Keybinds.Bind((string)args[1], (string)args[0]),
                }
            },
            {
                "fullscreen", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.Fullscreen, $"fullscreen {(bool)args[0]};");
                        G.S.SetFullscreen ((bool)args[0]);
                    },
                }
            },
            {
                "rawinput", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.RawInput, $"rawinput {(bool)args[0]};");
                        G.S.RawInput = ((bool)args[0]);
                    },
                }
            },
            {
                "fov", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.FOV, $"fov {(float)args[0]};");
                        Cvars.Set("fov", ((float)args[0]).ToString());
                        // TODO might not need this line after refactor
                        SeqCam.UpdateFOV();
                    },
                }
            },
            {
                "sensitivity", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.Sensitivity, $"sensitivity {(float)args[0]};");
                        SystemPrefsManager.Sensitivity = (float)args[0];
                    },
                }
            },
            {
                "sticksensitivity", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.StickSensitivity, $"sticksensitivity {(float)args[0]};");
                        SystemPrefsManager.StickSensitivity = (float)args[0];
                    },
                }
            },
            {
                "vsync", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.Vsync, $"vsync {(bool)args[0]};");
                        G.S.SetVsync ((bool)args[0]);
                    },
                }
            },
            {
                "verbose", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.Verbose, $"verbose {(bool)args[0]};");
                        Logger.Verbose = ((bool)args[0]);
                    },
                }
            },
            {
                "volume-master", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.VolumeMaster, $"volume-master {(float)args[0]};");
                        AudioBusController.Inst.SetMaster ((float)args[0]);
                    },
                }
            },
            {
                "volume-sfx", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.VolumeSFX, $"volume-sfx {(float)args[0]};");
                        AudioBusController.Inst.SetSfx ((float)args[0]);
                    },
                }
            },

            {
                "volume-music", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.VolumeMusic, $"volume-music {(float)args[0]};");
                        AudioBusController.Inst.SetMusic ((float)args[0]);
                    },
                }
            },

            {
                "volume-ui", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args =>
                    {
                        await SystemPrefsManager.Set(SystemPref.VolumeUI, $"volume-ui {(float)args[0]};");
                        AudioBusController.Inst.SetUI ((float)args[0]);
                    },
                }
            },
            {
                "set-display", new CommandInfo
                {
                    Params = [typeof(int)],
                    Exec = async args => G.S.SetCurrentOutput ((int)args[0]),
                }
            },
        };
    }
}
