// MIT License 

using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace SEQ.Script.Core
{
    public static class ActorRegistry
    {
        public static Dictionary<string, Actor> Active = new Dictionary<string, Actor>();

        public static void Register(Actor entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.State.SeqId))
            {
                Active[entity.State.SeqId] = entity;
            }
            Cvars.Current.Actors[entity.State.SeqId] = entity.State;
        }
        public static void Remove(Actor entity)
        {

            if (!string.IsNullOrWhiteSpace(entity.State.SeqId))
            {
                Active.Remove(entity.State.SeqId);
                Cvars.Current.Actors[entity.State.SeqId] = entity.State;
            }
        }

        public static void OnSaveGame()
        {

        }

        public static void OnLoadGame()
        {

        }

        public static void ResetAll()
        {
            foreach (var actor in Active.Values)
            {
                actor.DestroyWorld();
            }

            Active.Clear();
        }

    }

    public interface IActorComponent
    {
        void OnCreation(Actor actor);
    }

    public class Actor : ScriptComponent
    {
        public event Action<Vector3, Quaternion> OnPositionChangedAction;
        public event Action OnValueChanged;

        public event Action PrepareSave;
        public event Action DestroyEvent;

        [DataMemberIgnore]
        public ActorState State;
        //[DataMemberIgnore]
        //public List<Zone> Zones = new List<Zone>();
        public float Mass;

        public event Action<float> ActiveUpdate;

        // not implemented yet
        //public event Action<float> InactiveUpdate;

        public PhysicsComponent MainPhysicsComponent;

        List<ModelComponent> ModelComponents;
        List<PhysicsComponent> PhysicsComponents;

        void DisableModel()
        {
            foreach (var mc in ModelComponents)
            {
                mc.Enabled = false;
            }
        }
        void EnableModel()
        {
            foreach (var mc in ModelComponents)
            {
                mc.Enabled = true;
            }
        }
        public void DisablePhysics()
        {
            foreach (var mc in PhysicsComponents)
            {
                mc.Enabled = false;
            }
        }
        public void EnablPhysics()
        {
            foreach (var mc in PhysicsComponents)
            {
                mc.Enabled = true;
            }
        }

        public void UpdateActive(float dt)
        {
            ActiveUpdate?.Invoke(dt);
        }

        // not implemented yet
        //public void UpdateInactive(float dt)
        //{
        //    InactiveUpdate?.Invoke(dt);
        //}

        public void OnEnterZone()
        {
            EnableModel();
            EnablPhysics();
        }

        public void OnExitZone()
        {
            DisableModel();
            DisablePhysics();

        }

        public TransformComponent DropTransform;
        public void SetState(ActorState ent)
        {
            State = ent;
            ent.InWorld = true;
            Transform.Position = ent.Position;
            Transform.Rotation = ent.Rotation;
            ActorRegistry.Register(this);
        }


        public void LoadFromState(bool destroyOld = false)
        {
            if (Cvars.Current.Actors.TryGetValue(State.SeqId, out var s))
            {
                State = s;
                if (MainPhysicsComponent != null)
                {
                    var matrix = MainPhysicsComponent.PhysicsWorldTransform;
                    matrix.TranslationVector = s.Position;
                    MainPhysicsComponent.PhysicsWorldTransform = matrix;
                }
                else
                {
                    Transform.Position = s.Position;
                    Transform.Rotation = s.Rotation;
                }
                OnPositionChangedAction?.Invoke(s.Position, s.Rotation);
                OnValueChanged?.Invoke();

                if (destroyOld && !s.InWorld)
                {
                    DestroyWorld();
                }
            }
        }

        public void SetPositionAndRotation(Vector3 pos, Quaternion rot)
        {
            Transform.Position = pos;
            Transform.Rotation = rot;
            State.Position = pos;
            State.Rotation = rot;
            if (MainPhysicsComponent != null)
            {
                var matrix = MainPhysicsComponent.PhysicsWorldTransform;
                matrix.TranslationVector = pos;
                MainPhysicsComponent.PhysicsWorldTransform = matrix;
            }
            OnPositionChangedAction?.Invoke(pos, rot);
        }

        public void OnCreation()
        {
            ActorRegistry.Register(this);
            foreach (var comp in Entity.GetInterfacesInChildren<IActorComponent>())
            {
                comp.OnCreation(this);
            }
            if (Cvars.Current.Actors.ContainsKey(State.SeqId))
            {
                LoadFromState(true);
            }
            else
            {
                Cvars.Current.Actors[State.SeqId] = State;
            }

            Entity.Commands.Add("move", new CommandInfo
            {
                Params = [typeof(float), typeof(float), typeof(float)],
                Exec = async args => Transform.Position = new Vector3((float)args[0], (float)args[1], (float)args[2]),
            });

            ModelComponents = Entity.GetAllInChildren<ModelComponent>();
            PhysicsComponents = Entity.GetAllInChildren<PhysicsComponent>();

        }

        public void OnChanged()
        {
            OnValueChanged?.Invoke();
        }

        public void DestroyWorld()
        {
            DestroyEvent?.Invoke();
            State.InWorld = false;
            ActorRegistry.Remove(this);
            //  if (Entity.Transform.Parent != null) Entity.Transform.Parent = null;
           //Entity.Scene = null;
            Entity.EntityManager.Remove(Entity);
          //  Cancel();
            //Entity.Scene.Entities.Remove(Entity);
        }

        public void SaveState()
        {
            State.Position = Transform.Position;
            State.Rotation = Transform.Rotation;
            PrepareSave?.Invoke();
            Cvars.Current.Actors[State.SeqId] = State;
        }
    }
}
