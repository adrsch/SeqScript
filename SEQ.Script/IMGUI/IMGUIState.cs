using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public class IMGUIState : IGameStateController
    {
        public static IMGUIState Inst = new IMGUIState();
        public InteractionState GetInteractionState()
        {
            return InteractionState.GUI;
        }

        public PointerState GetPointerState()
        {
            return PointerState.GUI;
        }

        public void OnGainControl()
        {
        }

        public void OnLoseControl()
        {
        }
    }
}
