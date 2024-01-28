using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public class TimeProvider : ITimeProvider
    {
        public float clockTime
        {
            get
            {
                return G.S.SceneSystem.renderContext.Time.ClockTime;
            }
            set
            {
                if (G.S.SceneSystem.renderContext.Time != null)
                    G.S.SceneSystem.renderContext.Time.ClockTime = value;
            }
        }
        public float deltaTime
        {
            get
            {
                return (float)G.S.UpdateTime.Elapsed.TotalSeconds;
            }
        }
        public float time
        {
            get
            {
                return (float)G.S.UpdateTime.Total.TotalSeconds;
            }
        }
        public float unscaledTime
        {
            get
            {
                return (float)G.S.UpdateTime.Total.TotalSeconds;
            }
        }

        public float unscaledDeltaTime
        {
            get
            {
                return (float)G.S.UpdateTime.Elapsed.TotalSeconds;
            }
        }

    }
}

