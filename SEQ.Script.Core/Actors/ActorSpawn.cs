// MIT License 

using Stride.Engine;
using Stride.Physics;

namespace SEQ.Script.Core
{
    public class ActorSpawn : StartupScript
    {
        public string IdOverride;
        public string Species;
        public int Quantity = 1;
        public Dictionary<string, string> Vars = new Dictionary<string, string>();


        public ActorState GetState()
        {
            var s = new ActorState
            {
                Species = Species,
                Quantity = Quantity,
                InWorld = true,
                Position = Transform.WorldPosition,
                Rotation = Transform.WorldRotation,
            };
            if (!String.IsNullOrWhiteSpace(IdOverride))
                s.SeqId = IdOverride;
            else
            {
                s.SeqId = $"{Species}{Random.Shared.Next(99)}";
                while (Cvars.Current.Actors.ContainsKey(s.SeqId))
                {
                    s.SeqId = $"{s.SeqId}{Random.Shared.Next(128)}";
                }
            }
            foreach (var kvp in Vars)
            {
                s.Vars[kvp.Key] = kvp.Value;
            }
            return s;
        }

        public string SpawnEvent = "";
        bool SpawnAtStart => string.IsNullOrWhiteSpace(SpawnEvent);
        public override void Start()
        {
            base.Start();
            if (SpawnAtStart)
            {
                DoSpawn();
                ActorSpeciesRegistry.S.ResetSpawns += () =>
                {
                    DoSpawn();
                    CurrentActive = null;
                };
            }
            else
            {
                if (Entity.Get<ModelComponent>() is ModelComponent m)
                {
                    m.Enabled = false;
                }
                EventManager.AddListener(SpawnEvent, DoSpawn);
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            EventManager.RemoveListener(SpawnEvent, DoSpawn);
        }

        Actor CurrentActive;
        void DoSpawn()
        {
            if (CurrentActive != null)
            {
                CurrentActive.DestroyWorld();
            }
            var state = GetState();
            var actor = ActorSpeciesRegistry.S.GetWorldEnt(state);
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"Spawning {state.SeqId} of species {state.Species}");
            actor.Transform.Position = Transform.GetWorldPosition(true);
            actor.Transform.Rotation = Transform.GetWorldRotation(true);
            actor.Transform.Scale = Transform.Scale;
            actor.DestroyEvent += () =>
            {
                if (CurrentActive == actor)
                {
                    CurrentActive = null;
                }
            };
            CurrentActive = actor;
            foreach (var rb in actor.Entity.GetAllInChildren<RigidbodyComponent>())
            {
                rb.UpdatePhysicsTransformation();
            }
            if (Entity.Get<ModelComponent>() is ModelComponent m)
            {
                m.Enabled = false;
            }
        }

    }
}
