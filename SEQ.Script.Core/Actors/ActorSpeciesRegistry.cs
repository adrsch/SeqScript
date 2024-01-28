// MIT License

using Stride.Core;
using Stride.Engine;

namespace SEQ.Script.Core
{
    public interface IActorSpecies
    {
        public string Species { get; set; }
        public Prefab Prefab { get; set; }
    }

    public abstract class ActorSpeciesRegistry : StartupScript
    {
        public static ActorSpeciesRegistry S;
        [DataMemberIgnore]
        public Dictionary<string, IActorSpecies> Species = new Dictionary<string, IActorSpecies>();

        public event Action ResetSpawns;

        public void DoResetSpawns() { ResetSpawns?.Invoke(); }

        public override void Start()
        {
            S = this;
            base.Start();
            foreach (var s in GetSpecies())
            {
                Species[s.Species] = s;
            }
        }

        public abstract List<IActorSpecies> GetSpecies();

        public static bool TryGetSpecies(string id, out IActorSpecies species)
        {
            var success = S.Species.TryGetValue(id, out species);
            return success;
        }

        public Actor GetWorldEnt(ActorState ent)
        {
            if (TryGetSpecies(ent.Species, out var species))
            {
                // TODO: hack
                var ents = species.Prefab.Instantiate();
                var world = ents[0].Get<Actor>();
                Entity.Scene.Entities.Add(ents[0]);
                ents[0].Transform.Parent = null;
                world.SetState(ent);
                world.OnCreation();
                return world;
            }
            Logger.Log(Channel.Gameplay, LogPriority.Error, $"No species found for {ent.SeqId} {ent.Species}");
            return null;
        }

        public static Actor FromState(ActorState ent) => S.GetWorldEnt(ent);

        public static Actor SpawnNewWorld(string species)
        {
            if (TryGetSpecies(species, out var s))
            {
                var ent = new ActorState { SeqId = System.Guid.NewGuid().ToString(), Species = species };
                Cvars.Current.Actors[ent.SeqId] = ent;
                return S.GetWorldEnt(ent);
            }
            return default;
        }

        public static ActorState SpawnNewState(string species)
        {
            if (TryGetSpecies(species, out var s))
            {
                var ent = new ActorState { SeqId = System.Guid.NewGuid().ToString(), Species = species };
                Cvars.Current.Actors[ent.SeqId] = ent;
                return (ent);
            }
            return default;
        }

        public bool TryRecycle(Actor w)
        {
            return true;
        }
    }
}