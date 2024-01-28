using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public static class CommandsTemplates
    {
        public static Dictionary<string, CommandInfo> Commands => new Dictionary<string, CommandInfo>
        {
            {
                "start", new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => TemplateManager.S.StartTemplate((string)args[0]),
                    Help = "Set the template used"
                }
            },
            {
                "end", new CommandInfo
                {
                    Exec = async args => Template.End(),
                    Help = "Ends currently active template used"
                }
            },
            {
                "format", new CommandInfo
                {
                    Params = [typeof(string), typeof(string)],
                    Exec = async args => Template.Current?.Format((string)args[0], (string)args[1]),
                    Help = "Ends currently active template used"
                }
            },
            {
                "continue", new CommandInfo
                {
                    Exec = async args => Sequencer.Continue(),
                    Help = "Continues a script"
                }
            },
        };
    }
}
