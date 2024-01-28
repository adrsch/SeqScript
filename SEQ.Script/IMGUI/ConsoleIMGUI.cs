/*using Stride.Core;
using StrideCommunity.ImGuiDebug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using static StrideCommunity.ImGuiDebug.ImGuiExtension;
using static ImGuiNET.ImGui;
using Stride.UI.Controls;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ
{
    public class ConsoleIMGUI : BaseWindow
    {
        static LinkedList<string> History = new LinkedList<string>();
        int historySpot = -1;
        public ConsoleIMGUI(IServiceRegistry services) : base(services)
        {
            Logger.OnLogUpdated += UpdateLog;
            UpdateLog();
            if (!IMGUIState.Inst.IsActive())
                GameStateManager.Push(IMGUIState.Inst);
        }

        protected override void OnDestroy()
        {
            Logger.OnLogUpdated -= UpdateLog;
            BaseWindow.Windows.Remove("console");
        }

        protected override void OnDraw(bool collapsed)
        {
            if (collapsed)
                return;
            using (Child())
            {
                Text(_text);
                if (InputText("", ref editText, 256, ImGuiInputTextFlags.None))
                {

                }
            }
        }
        string _text = "";
        string editText = "";
        void UpdateLog()
        {
            _text = Logger.GetLogs(new LogDisplayOptions
            {
                channelFilter = Channel.All,
                priorityFilter = LogPriority.All,
                showTimeStamp = true,
            });
        }

        void MoveToHistory()
        {
            if (historySpot < 0)
            {
                historySpot = -1;

                editText = "";
            }
            else if (historySpot >= History.Count)
            {
                historySpot = History.Count - 1;
            }
            else
            {
                editText = History.ElementAt(historySpot);
            }
        }
    }
}
*/