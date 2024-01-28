// MIT License

using SEQ.Script.Core;
using Stride.Engine;

namespace SEQ.Script
{
    // TODO: This should not be adding scripts to an entity
    public static class ScriptRunner 
    {
        public static Entity RunnerEntity;

        // This is a bit confusing of a name since running off a file is implicit
        public static void Exec(string path)
        {
            RunnerEntity.Add(new AsyncExecScript(path));
        }

        public static async Task ExecAsync(string path)
        {
            Shell.Exec(await FileUtil.ReadDataFileAsync(path));
        }

        public static void Run(string debugInfo, string path, List<string> p = null, Action onComplete = null)
        {
            if (FileUtil.DataFileExists(path))
            {
                RunnerEntity.Add(new AsyncMakeSeqThread(debugInfo, path, p, onComplete));
           //     Task.Run(async () =>
            //    {
              //      Sequencer.Inst.Thread(new SequencerContext { DebugInfo = $"{debugInfo}->{path}" }, await Utils.ReadDataFileAsync(path), onComplete);
               // }).Start();
            }
            else if (Shell.Routines.TryGetValue(path, out var func))
            {
                if (p != null)
                    func.Exec(debugInfo, p.ToArray(), onComplete);
                else
                    func.Exec(debugInfo, default(string[]), onComplete);
            }
            else
            {
                var fail = path;
                if (p != null)
                    foreach (var aa in p)
                        fail += $" {aa}";
                Sequencer.S.Thread(new SequencerContext { DebugInfo = $"{debugInfo}->{path}" }, fail, onComplete);
            }
        }


        /*      [SEQ.Command("Define an alias for a list of commands")]
              public static void Alias(string name, string a, string b = null, string c = null, string d = null, string e = null, string f = null, string g = null, string h = null, string i = null, string j = null, string k = null, string l = null, string m = null, string n = null)
              {
                  Shell.Alias(name, new string[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n });
              }
          }*/
    }
}