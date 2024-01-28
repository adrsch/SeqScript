// MIT License

using Stride.Engine;
using Stride.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using Stride.Engine.Design;
using Stride.Graphics;

namespace SEQ.Script
{
    public static class Manager
    {
        public static bool DebugMode;

        public static void Init()
        {
            DebugMode = false;

            // hacks
            Time.Provider = new TimeProvider();

            Keybinds.S = new Keybinds();
            SystemPrefsManager.Reset();
            Loc.Reset();
            Shell.Init();
            Shell.Add(CommandsGame.Commands);
            Shell.Add(CommandsPrefs.Commands);
            Shell.Add(CommandsTemplates.Commands);
            Shell.Add(CommandsAudio.Commands);

            ReverbZoneManager.ResetZones();

            G.S.Script.AddTask(async () =>
            {
                // Wait for first update to complete
                await G.S.Script.NextFrame();

                // TODO use settings
                ScriptRunner.Exec("startup.seq");
            });

        }
    }
}
