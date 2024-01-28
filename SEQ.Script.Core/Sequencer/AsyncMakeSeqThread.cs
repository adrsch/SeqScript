// MIT License

using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script.Core
{
    // TODO: Get rid of this
    public class AsyncMakeSeqThread
        : AsyncScript
    {
        string DebugInfo; string Path; List<string> P = null; Action OnComplete = null;
        public AsyncMakeSeqThread(string debugInfo, string path, List<string> p = null, Action onComplete = null) : base()
        {
            DebugInfo = debugInfo;
            Path = path;
            P = p;
            OnComplete = onComplete;
                
        }
        public override async Task Execute()
        {
            Sequencer.S.Thread(new SequencerContext { DebugInfo = $"{DebugInfo}->{Path}" }, await FileUtil.ReadDataFileAsync(Path), OnComplete);
        }
    }
}
