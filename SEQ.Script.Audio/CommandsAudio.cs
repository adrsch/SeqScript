using Stride.Audio;
using Stride.Engine;
using SEQ.Script.Core;

namespace SEQ.Script.Audio
{
    public static class CommandsAudio
    {
        public static Dictionary<string, CommandInfo> Commands => new Dictionary<string, CommandInfo>
        {
            {
                "wetsound", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => AudioBusController.Inst.SetWet((float)args[0]),
                    Help = "Temporary override, will only last until a trigger is entered or exited"
                }
            },
                        {
                "drysound", new CommandInfo
                {
                    Params = [typeof(float)],
                    Exec = async args => AudioBusController.Inst.SetDry((float)args[0]),
                    Help = "Temporary override, will only last until a trigger is entered or exited"
                }
            },
        };
    }
}
