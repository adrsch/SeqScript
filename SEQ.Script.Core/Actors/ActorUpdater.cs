// MIT License

using Stride.Engine;

namespace SEQ.Script.Core
{
    // Active and inactive states aren't implemented yet
    public class ActorUpdater : SyncScript
    {
        public override void Update()
        {
            foreach (var a in ActorRegistry.Active.Values)
            {
                // TODO
                a.UpdateActive(Time.deltaTime);
                /*
                var hasUpdated = false;
                foreach (var z in a.Zones)
                {
                    if (z.IsZoneActive)
                    {
                        a.UpdateActive(Time.deltaTime);
                        hasUpdated = true;
                        break;
                    }
                }
                if (!hasUpdated)
                {
                    a.UpdateInactive(Time.deltaTime);
                }*/
            }
        }
    }
}
