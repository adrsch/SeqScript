using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Script
{
    public interface ITouchModule : ISeqModule
    {
        bool Initialized { get; }
        bool TouchEnabled { get; }

        void OpenKeyboard(string name, bool multiline);
    }
}
