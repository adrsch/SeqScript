// MIT License

using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script.Core
{
    // TODO: Get rid of this
    public class AsyncExecScript : AsyncScript
    {
        string Command;
        public AsyncExecScript(string cmd) : base()
        {
            Command = cmd;
        }
        public override async Task Execute()
        {
            await ScriptRunner.ExecAsync(Command);
        }
    }
}
