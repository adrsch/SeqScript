using Stride.Engine;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public class ObjectVisibilityZone : AreaTrigger
    {
        public List<ModelComponent> HideInZone = new List<ModelComponent>();
        public List<ModelComponent> ShowInZone = new List<ModelComponent>();

        public override TransformComponent GetTargetTransform()
        {
            return SeqCam.S.Transform;
        }

        protected override void OnEnter()
        {
            foreach (var mc in HideInZone)
            {
                mc.Enabled = false;
            }

            foreach (var mc in ShowInZone)
            {
                mc.Enabled = true;
            }
        }

        protected override void OnExit()
        {
            foreach (var mc in HideInZone)
            {
                mc.Enabled = true;
            }

            foreach (var mc in ShowInZone)
            {
                mc.Enabled = false;
            }
        }
    }
}
