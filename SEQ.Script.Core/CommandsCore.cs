// MIT License 

using Stride.Engine;

namespace SEQ.Script.Core
{
    public static class CommandsCore
    {
        public static Dictionary<string, CommandInfo> Commands => new Dictionary<string, CommandInfo>
        {
            {
                "clear", new CommandInfo
                {
                    Exec = async _ => Logger.ClearLogs(),
                }
            },
            {
                "locale", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => await Loc.Locale((string)args[0]),
                    Help = "Set the game locale",
                }
            },
            {
                "log", new CommandInfo
                {
                    Params = [typeof(Channel), typeof(LogPriority), typeof(string)],
                    Exec = async args => Logger.Log((Channel)args[0],(LogPriority)args[1], (string)args[2]),
                }
            },
            {
                "print", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => Logger.Print((string)args[0]),
                    Help = "Print in console log",
                }
            },
            {
                "set", new CommandInfo
                {
                    Params = [typeof(string),typeof(string)],
                    Exec = async args => Cvars.Set((string)args[0], (string)args[1]),
                    Help = "Set the value of a cvar",
                }
            },
            {
                "get", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => Logger.Print(Cvars.Get<string>((string)args[0])),
                    Help = "Print the value of a cvar",
                }
            },
            {
                "new", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => ActorState.AddNew((string)args[0]),
                    Help = "Creates a new actor with the given seqid",
                }
            },
            {
                "species", new CommandInfo
                {
                    Params = [typeof(string), typeof(string)],
                    Exec = async args => { if (Cvars.Current.Actors.TryGetValue((string)args[0], out var a))
                        { a.Species = (string)args[1]; }
                        else { Logger.Log(Channel.Shell, LogPriority.Error, $"Couldnt find actor id {(string)args[0]}"); } },
                    Help = "Sets the species for a given actor",
                }
            },
            {
                "position", new CommandInfo
                {
                    Params = [typeof(string)],
                    OptionalParams = [typeof(float?), typeof(float?), typeof(float?)],
                    Exec = async args => { if (Cvars.Current.Actors.TryGetValue((string)args[0], out var a))
                        { if ((float?)args[1] != null && (float?)args[2] != null && (float?)args[3] != null)
                            {
                                a.Position = new Stride.Core.Mathematics.Vector3(((float?)args[1]).Value, ((float?)args[2]).Value, ((float?)args[3]).Value);
                                a.UpdateWorld();
                            }
                        else
                            {
                                Logger.Print($"Position for {(string)args[0]}:\n{a.Position})");
                            }
                        } else { Logger.Log(Channel.Shell, LogPriority.Error, $"Couldnt find actor id {(string)args[0]}"); }
                    },
                    Help = "Sets the species for a given actor",
                }
            },
            {
                "help", new CommandInfo
                {
                    Exec = async args => Shell.Help(),
                }
            },
            {
                "exec", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => ScriptRunner.Exec((string)args[0]),
                    Help = "Run a seq file"
                }
            },
        };
    }
}
