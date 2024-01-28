using Stride.Core.Mathematics;
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
    public abstract class AreaTrigger : AsyncScript
    {
        public int UpdateDelay = 100;

        public override async Task Execute()
        {
            while (true)
            {
                if (GetTargetTransform() != null)
                {
                    var ib = CheckInBounds(GetTargetTransform().WorldPosition);
                    if (ib && !targetInTrigger)
                    {
                        OnEnter();
                    }
                    else if (!ib && targetInTrigger)
                    {
                        OnExit();
                    }
                    targetInTrigger = ib;
                }
                else if (targetInTrigger)
                {
                    OnExit();
                    targetInTrigger = false;
                }
                await Task.Delay(UpdateDelay);
            }
        }

        public abstract TransformComponent GetTargetTransform();

        public bool CheckInBounds(Vector3 p)
        {
            return p.x > Transform.WorldPosition.x - ToEdge * Transform.Scale.x
                && p.x < Transform.WorldPosition.x + ToEdge * Transform.Scale.x
                && p.y > Transform.WorldPosition.y - ToEdge * Transform.Scale.y
                && p.y < Transform.WorldPosition.y + ToEdge * Transform.Scale.y
                && p.z > Transform.WorldPosition.z - ToEdge * Transform.Scale.z
                && p.z < Transform.WorldPosition.z + ToEdge * Transform.Scale.z;
        }

        float ToEdge = 5f;

        bool targetInTrigger;
        protected abstract void OnEnter();
        protected abstract void OnExit();
    }
}
