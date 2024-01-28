using Stride.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Script
{
    public interface ISeqModule
    {
        int Priority { get; }
        void Init();
        void Exit();
    }

    public interface ISeqDrawModule
    {
        void Draw(GameTime gameTime);
    }

    public interface ISeqUpdateModule
    {
        void Update(GameTime gameTime);
    }
}
