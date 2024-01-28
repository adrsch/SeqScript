using Microsoft.VisualBasic;
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
    public static class CommandsGame
    {
        public static Dictionary<string, CommandInfo> Commands => new Dictionary<string, CommandInfo>
        {
            {
                "quit", new CommandInfo
                {
                    Exec = async args => {// Logger.Log(Channel.General, LogPriority.Info, "exit code 0 weeeeeeeee");
                    //    ConsoleUIBase.Inst.WriteToFile();

                    G.S.DoExit();
                    },
                }
            },
            {
                "camera", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => CameraPositioner.SetCamera((string)args[0]),
                    Help = "Set the camera to the camera in the scene with the provided id"
                }
            },
            #region saving
            {
                "newsave", new CommandInfo
                {
                    Exec = async args => SaveUtils.NewSave(),
                }
            },
            {
                "autosave", new CommandInfo
                {
                    Exec = async args => SaveUtils.Save(Constants.AutosaveFile),
                }
            },
            {
                "savegame", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => SaveUtils.Save((string)args[0]),
                }
            },
            {
                "loadgame", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => SaveUtils.Load((string)args[0]),
                }
            },
            #endregion
            #region debug tools
            {
                "debugtext", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args => Sequencer.S.DebugText.Enabled = (bool)args[0],
                    Help = "Show builtin stride debug text"
                }
            },
            {
                "debugmode", new CommandInfo
                {
                    Params = [typeof(bool)],
                    Exec = async args => Manager.DebugMode = (bool)args[0],
                    Help = "Show various debug info and lets you interact very far away"
                }
            },

#endregion
        };
    }
}
