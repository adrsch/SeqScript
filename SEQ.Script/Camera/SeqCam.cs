using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public class SeqCam : CvarListenerStartup
    {
        [DataMemberIgnore]
        public CameraComponent Camera;

        public static SeqCam S;

        public override void Start()
        {
            if (Camera == null)
                Camera = Entity.Get<CameraComponent>();
            S = this;
        }

        public void BindPosition(RegisteredCamera c)
        {
            Transform.Parent = c.Transform;
            Transform.Position = Vector3.zero;
            Transform.Rotation = Quaternion.Identity;
            Transform.Scale = Vector3.one;
        }

        public void Unbind()
        {
            Transform.Parent = null;
        }

        public static void UpdateFOV()
        {
            if (S != null)
            {
                S.Camera.VerticalFieldOfView = Utils.VFov(S.CurrentFOV);
            }
        }

        protected override string GetCvar()
        {
            return "fov";
        }

        public override void OnValueChanged()
        {
            if (Cvars.TryGet<float>("fov", out var f))
            {
                CurrentFOV = f;
                UpdateFOV();
            }
        }

        public override void OnStart()
        {
        }

        float CurrentFOV = 105;
        public override void BeforeStart()
        {
            base.BeforeStart();
            S.Camera.VerticalFieldOfView = Utils.VFov(CurrentFOV);
        }
    }
}
