using SEQ.Script;
using Stride.Audio;
using Stride.Engine;
using SEQ.Script.Core;

namespace SEQ.Script.Audio
{
    public static class ReverbZoneManager
    {
        public static List<ReverbZone> ActiveZones = new List<ReverbZone>();
        public static void ResetZones()
        {
            ActiveZones.Clear();
        }

        public static void UpdateReverbSettings()
        {
            int tobeat = -1;
            foreach (var zone in ActiveZones)
            {
                if (zone.ZonePriority > tobeat)
                {
                    tobeat = zone.ZonePriority;
                    AudioBusController.Inst.SetDry(zone.Dry);
                    AudioBusController.Inst.SetWet(zone.Wet);
                }
            }
            if (tobeat < 0)
            {
                AudioBusController.Inst.SetDry(1f);
                AudioBusController.Inst.SetWet(0f);
            }

            Logger.Log(Channel.Audio, LogPriority.Trace, $"Updated revert settings. Dry {AudioBusController.Inst.Dry} \\ Wet {AudioBusController.Inst.Wet}");
        }
    }

    public class ReverbZone : AreaTrigger
    {
        public int ZonePriority;
        public float Wet = 0f;
        public float Dry = 1f;

        public override TransformComponent GetTargetTransform()
        {
            return SeqCam.S.Transform;
        }

        protected override void OnEnter()
        {
            ReverbZoneManager.ActiveZones.Add(this);
            ReverbZoneManager.UpdateReverbSettings();
        }

        protected override void OnExit()
        {
            ReverbZoneManager.ActiveZones.RemoveAll(x => x == this);
            ReverbZoneManager.UpdateReverbSettings();
        }
    }
}
