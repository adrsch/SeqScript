using Stride.Engine;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public enum Keybind
    {
        Forward,
        Backward,
        Left,
        Right,
        Jump,
        Interact,
        Slot1, Slot2, Slot3, Slot4,
        Slot5, Slot6, Slot7, Slot8,
        Slot9,
        Slot10,
        Sprint,
        Crouch,
        Console,
        Tab,
        Reload,
        Drop,
    }
    public class Keybinds
    {
        public InputManager Input => Sequencer.S.Input;
        public static Keybinds S;

        public Dictionary<Keybind, Keys> Bindings = new Dictionary<Keybind, Keys>();

        public static Dictionary<Keybind, string> GetDefaultBinds()
        {
           return new Dictionary<Keybind, string>
            {

                {
                    Keybind.Forward,
                    "bind w forward;"
                },
                {
                    Keybind.Left,
                    "bind a left;"
                },
                {
                    Keybind.Backward,
                    "bind s backward;"
                },
                {
                    Keybind.Right,
                    "bind d right;"
                },
                {
                    Keybind.Jump,
                    "bind space jump;"
                },
                {
                    Keybind.Interact,
                    "bind f interact;"
                },
                {
                    Keybind.Sprint,
                    "bind leftshift sprint;"
                },
                {
                    Keybind.Crouch,
                    "bind x crouch;"
                },
                {
                    Keybind.Tab,
                    "bind tab tab;"
                },
                {
                    Keybind.Console,
                    "bind oemtilde console;"
                },
                {
                    Keybind.Reload,
                    "bind r reload;"
                },
                {
                    Keybind.Drop,
                    "bind g drop;"
                },
                {
                    Keybind.Slot1,
                    "bind 1 slot1;"
                },
                {
                    Keybind.Slot2,
                    "bind 2 slot2;"
                },
                {
                    Keybind.Slot3,
                    "bind 3 slot3;"
                },
                {
                    Keybind.Slot4,
                    "bind 4 slot4;"
                },
                {
                    Keybind.Slot5,
                    "bind 5 slot5;"
                },
                {
                    Keybind.Slot6,
                    "bind 6 slot6;"
                },
                {
                    Keybind.Slot7,
                    "bind 7 slot7;"
                },
                {
                    Keybind.Slot8,
                    "bind 8 slot8;"
                },
                {
                    Keybind.Slot9,
                    "bind 9 slot9;"
                },
                {
                    Keybind.Slot10,
                    "bind 0 slot10;"
                }
            };
        }

        static Keys GetKeyForString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Keys.None;
            }
            if (char.IsNumber(key[0]))
            {
                if (Enum.TryParse(typeof(Keys), $"D{key}", true, out var asDig))
                {
                    return (Keys)asDig;
                }
            }
            if (Enum.TryParse(typeof(Keys), key, true, out var asKey))
            {
                return (Keys)asKey;
            }
            //     Logger.Log(Channel.Input, LogPriority.Warning, $"cant get key: {key}");
            return Keys.None;
        }

        public static async Task Bind(string kb, string key)
        {
            if (Enum.TryParse(typeof(Keybind), kb, true, out var asKey))
            {
                S.Bindings[(Keybind)asKey] = GetKeyForString(key);
                await SystemPrefsManager.SetBind((Keybind)asKey, $"bind {kb} {key}");
            }
     //       else
           //     Logger.Log(Channel.Input, LogPriority.Error, $"Could not get keybind type {kb} to set bind as {key}");
        }

        public static bool GetKey(Keybind keybind)
        {
            if (S.Bindings.TryGetValue(keybind, out var key))
            {
                return G.S.Input.IsKeyDown(key);
            }
         //   Logger.Log(Channel.Input, LogPriority.Warning, $"Could not get keybind for {keybind}");

            return false;
        }
        public static bool GetKeyDown(Keybind keybind)
        {
            if (S.Bindings.TryGetValue(keybind, out var key))
            {
                return G.S.Input.IsKeyPressed(key);
            }
       //     Logger.Log(Channel.Input, LogPriority.Warning, $"Could not get keybind for {keybind}");

            return false;
        }
        public static bool GetKeyUp(Keybind keybind)
        {
            if (S.Bindings.TryGetValue(keybind, out var key))
            {
                return G.S.Input.IsKeyReleased(key);
            }
        //    Logger.Log(Channel.Input, LogPriority.Warning, $"Could not get keybind for {keybind}");

            return false;
        }
        /*
        public override void Update()
        {
            if (Keybinds.GetKeyUp(Keybind.Tab))
            {
                if (!IMGUIState.Inst.IsActive())
                    GameStateManager.Push(IMGUIState.Inst);
                else
                {
                    GameStateManager.Remove(IMGUIState.Inst);
                }
            }
            if (GetKeyUp(Keybind.Console))
            {
                BaseWindow.ToggleWindow("console");
            }
        }*/
    }
}
