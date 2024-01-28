using System.Collections;
using System.Collections.Generic;
using SEQ;
using Stride.Engine;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Core.Shaders.Ast;
using Stride.Engine.Events;
using Stride.Graphics;
using Stride.Physics;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public enum CameraTypes
    {
        Base,
        Default,
        Cutscene,
    }

    public class CameraPositioner
    {
        public static CameraPositioner Inst = new CameraPositioner();
        public RegisteredCamera CurrentCamera;
        public string CurrentId;
        public Dictionary<string, RegisteredCamera> Cameras = new Dictionary<string, RegisteredCamera>();

        public RegisteredCamera BaseCamera;
        public RegisteredCamera DefaultCamera;

        public void OnInit()
        {
            Inst = this;
        }

        public static void SetCamera(string id)
        {
            if (Utils.ParsesAsNil(id))
            {
                if (Inst.DefaultCamera != null)
                {
                    Inst.CurrentCamera = Inst.DefaultCamera;
                    Inst.DefaultCamera.EnableCamera();
                }
                else if (Inst.BaseCamera != null)
                {
                    Inst.CurrentCamera = Inst.BaseCamera;
                    Inst.BaseCamera.EnableCamera();
                }
                else
                    Logger.Log(Channel.Gameplay, LogPriority.Error, "no default or base cameras found");
            }
            else if (Inst.CurrentCamera.SeqRef == id)
                return;
            else if (Inst.Cameras.TryGetValue(id, out var c))
            {
                Inst.CurrentCamera = c;
                c.EnableCamera();
            }
            else
                Logger.Log(Channel.Shell, LogPriority.Error, $"Could not find camera with id {id}");
        }

        public void ClearCurrent()
        {
            SeqCam.S.Unbind();
            if (Inst.DefaultCamera != null && Inst.CurrentCamera != Inst.DefaultCamera)
            {
                Inst.CurrentCamera = DefaultCamera;
                Inst.DefaultCamera.EnableCamera();
            }
            else if (Inst.BaseCamera != null && Inst.CurrentCamera != Inst.BaseCamera)
            {
                Inst.CurrentCamera = BaseCamera;
                Inst.BaseCamera.EnableCamera();
            }
            else
            {
                Logger.Log(Channel.Gameplay, LogPriority.Error, "no default or base cameras found");
                Inst.CurrentCamera = null;
            }
        }
        public static void RegisterCamera(string id, RegisteredCamera camera)
        {
            Inst.Cameras[id] = camera;
            if (camera.CameraType == CameraTypes.Base)
            {
                Inst.BaseCamera = camera;
                if (Inst.CurrentCamera == null)
                {
                    camera.EnableCamera();
                    Inst.CurrentCamera = camera;
                }
            }
            else if (camera.CameraType == CameraTypes.Default)
            {
                Inst.DefaultCamera = camera;
                if (Inst.CurrentCamera == null || Inst.CurrentCamera.CameraType == CameraTypes.Base)
                {
                    camera.EnableCamera();
                    Inst.CurrentCamera = camera;
                }
            }
        }
        public static void UnregisterCamera(string id)
        {
            Inst.Cameras.Remove(id);
        }
    }

    public class RegisteredCamera : StartupScript
    {
        public string SeqRef;
        public CameraTypes CameraType = CameraTypes.Cutscene;
        // Start is called before the first frame update
        public override void Start()
        {
            Register();
        }

        void Register()
        {
            if (!string.IsNullOrWhiteSpace(SeqRef))
                if (CameraPositioner.Inst != null)
                    CameraPositioner.RegisterCamera(SeqRef, this);
        }

        public void EnableCamera()
        {
            SeqCam.S.BindPosition(this);
        }

        public override void Cancel()
        {
            if (!string.IsNullOrWhiteSpace(SeqRef))
            if (CameraPositioner.Inst != null)
            {
                if (CameraPositioner.Inst.CurrentCamera == this)
                    CameraPositioner.Inst.ClearCurrent();

                CameraPositioner.UnregisterCamera(SeqRef);
            }
            base.Cancel();
        }
    }
}