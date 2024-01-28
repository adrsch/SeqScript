using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Engine;
 using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{

    public enum PointerState
    {
        Disabled,
        Free,
        Locked,
        GUI,
    }
    [System.Flags]
    public enum InteractionState
    {
        None = 0,
        GUI = 1 << 0,
        World = 1 << 1,
        Overlay = 1 << 2,

        Standard = 7,


    }
    /*
    public enum MovementState
    {
        Disabled,
        Character,
        Orbit,
    }
    */
    public enum DesktopState
    {
        Hidden,
        Displayed,
    }

    public enum CharacterState
    {
        Pilot,
        Mecha,
    }

    public class GameStateEvent
    {
        public IGameStateController State;
    }

    public interface IStateController
    {
        void OnGainControl();
        void OnLoseControl();
    }

    public interface IStateFlow : IStateController
    {
        bool ShouldBeActive();
        Task Update();
    }

    public class StateManager<T> where T : IStateController
    {

    }

    public class StateQueue<T> where T : class, IStateController
    {
        public readonly Queue<T> Queue = new Queue<T>();
        public T Active = null;

        public void RemoveAll<U>() where U : T
        {
            
        }

        public void Enqueue(T ctr)
        {
            if (ctr == null)
            {
                Logger.Log(Channel.General, LogPriority.Error, "Attempting to enqueue null state controller");
            }
            if (Queue.Contains(ctr))
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Attempt to enqueue statecontroller already in stack");
            }
            else
            {
                Queue.Enqueue(ctr);
                if (Queue.Count == 1)
                {
                    Active = ctr;
                    ctr.OnGainControl();
                }
                OnEnqueue(Active);
            }
        }

        public void ForceTo(T ctr)
        {
            if (Active == ctr)
                return;
            if (Queue.Contains(ctr))
            {
                while (Queue.Peek() != ctr)
                {
                    var popped = Queue.Dequeue();
                    popped.OnLoseControl();
                    Queue.Peek().OnGainControl();
                }
                Active = ctr;
                OnForceTo(ctr);
            }
            else
            {
                Enqueue(ctr);
            }
        }


        public void Dequeue(T ctr)
        {
            if (Queue.Peek() == ctr)
            {
                var popped = Queue.Dequeue();
                popped.OnLoseControl();
                if (Queue.Count > 0)
                {
                    Active = Queue.Peek();
                    Queue.Peek().OnGainControl();
                    OnDequeue(Active);
                }
                else
                {
                    Active = null;
                    OnDequeue(Active);
                }
            }
            else
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Attempt to dequeue statecontroller that is not current");
            }
        }

        public void Dequeue()
        {
            var popped = Queue.Dequeue();
            popped.OnLoseControl();
            if (Queue.Count > 0)
            {
                Active = Queue.Peek();
                Queue.Peek().OnGainControl();
                OnDequeue(Active);
            }
            else
            {
                Active = null;
                OnDequeue(null);
            }
        }

        public void Clear()
        {
            if (Queue.Count > 0)
                Queue.Peek().OnLoseControl();
            Queue.Clear();
            Active = null;
        }

        protected virtual void OnEnqueue(T ctr)
        {

        }
        protected virtual void OnForceTo(T ctr)
        {

        }
        protected virtual void OnDequeue(T ctr)
        {

        }
    }
    /*
    public class StateMachine<T> where T : class, IStateController
    {
        public T Active;

        public void Push(T ctr)
        {
            if (ctr == null)
            {
                Logger.Log(Channel.General, LogPriority.Error, "Attempting to push null state controller");
            }
            if (Stack.Contains(ctr))
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Attempt to push statecontroller already in stack");
            }
            else
            {
                if (Stack.Count > 0)
                {
                    Stack.First.Value.OnLoseControl();
                }
                Stack.AddFirst(ctr);
                //  Active = ctr;
                ctr.OnGainControl();
                OnPush(ctr);
            }
        }

        public void ForceTo(T ctr)
        {
            if (Active == ctr)
                return;
            if (Stack.Contains(ctr))
            {
                while (Stack.Count > 0 && Stack.First.Value != ctr)
                {
                    var popped = Stack.First.Value;
                    Stack.RemoveFirst();
                    popped.OnLoseControl();
                    Stack.First.Value.OnGainControl();
                }
                //Active = ctr;
                OnForceTo(ctr);
            }
            else
            {
                Push(ctr);
            }
        }

        T Peek()
        {
            if (Stack.Count == 0)
                return null;
            return Stack.First.Value;
        }

        public void Pop(T ctr)
        {
            if (Peek() == ctr)
            {
                var popped = Stack.First.Value;
                Stack.RemoveFirst();
                popped.OnLoseControl();
                if (Stack.Count > 0)
                {
                    //  Active = Stack.First.Value;
                    Stack.First.Value.OnGainControl();
                    OnPop(Stack.First.Value);
                }
                else
                {
                    //   Active = null;
                    OnPop(null);
                }
            }
            else
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Attempt to pop statecontroller that is not current");
            }
        }

        public void Remove(T ctr)
        {
            if (Peek() == ctr)
            {
                Pop();
            }
            else if (Stack.Contains(ctr))
            {
                Stack.Remove(ctr);
            }
        }

        public void Pop()
        {
            var popped = Stack.First.Value;
            Stack.RemoveFirst();
            popped.OnLoseControl();
            if (Stack.Count > 0)
            {
                //   Active = Stack.First.Value;
                Stack.First.Value.OnGainControl();
                OnPop(Stack.First.Value);
            }
            else
            {
                //   Active = null;
                OnPop(null);
            }
        }

        public void Clear()
        {
            while (Active != null)
                Pop();
        }

        protected virtual void OnPush(T ctr)
        {

        }
        protected virtual void OnForceTo(T ctr)
        {

        }
        protected virtual void OnPop(T ctr)
        {

        }
    }
    */

    public class StateStack<T> where T : class, IStateController
    {
        public readonly LinkedList<T> Stack = new LinkedList<T>();
        public T Active => Peek();

        public void Push(T ctr)
        {
            if (ctr == null)
            {
                Logger.Log(Channel.General, LogPriority.Error, "Attempting to push null state controller");
            }
            if (Stack.Contains(ctr))
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Attempt to push statecontroller already in stack");
            }
            else
            {
                if (Stack.Count > 0)
                {
                   Stack.First.Value.OnLoseControl();
                }
                Stack.AddFirst(ctr);
              //  Active = ctr;
                ctr.OnGainControl();
                OnPush(ctr);
            }
        }

        public  void ForceTo(T ctr)
        {
            if (Active == ctr)
                return;
            if (Stack.Contains(ctr))
            {
                while (Stack.Count > 0 && Stack.First.Value != ctr)
                {
                    var popped = Stack.First.Value;
                    Stack.RemoveFirst();
                    popped.OnLoseControl();
                    Stack.First.Value.OnGainControl();
                }
                //Active = ctr;
                OnForceTo(ctr);
            }
            else
            {
                Push(ctr);
            }
        }

        T Peek()
        {
            if (Stack.Count == 0)
                return null;
            return Stack.First.Value;
        }

        public  void Pop(T ctr)
        {
            if (Peek() == ctr)
            {
                var popped = Stack.First.Value;
                Stack.RemoveFirst();
                popped.OnLoseControl();
                if (Stack.Count > 0)
                {
                  //  Active = Stack.First.Value;
                    Stack.First.Value.OnGainControl();
                    OnPop(Stack.First.Value);
                }
                else
                {
                 //   Active = null;
                    OnPop(null);
                }
            }
            else
            {
                Logger.Log(Channel.General, LogPriority.Warning, "Attempt to pop statecontroller that is not current");
            }
        }

        public void Remove(T ctr)
        {
            if (Peek() == ctr)
            {
                Pop();
            }
            else if (Stack.Contains(ctr))
            {
                Stack.Remove(ctr);
            }
        }

        public void Pop()
        {
            var popped = Stack.First.Value;
            Stack.RemoveFirst();
            popped.OnLoseControl();
            if (Stack.Count > 0)
            {
             //   Active = Stack.First.Value;
                Stack.First.Value.OnGainControl();
                OnPop(Stack.First.Value);
            }
            else
            {
             //   Active = null;
                OnPop(null);
            }
        }

        public void Clear()
        {
            while (Active != null)
                Pop();
        }

        protected virtual void OnPush(T ctr)
        {

        }
        protected virtual void OnForceTo(T ctr)
        {

        }
        protected virtual void OnPop(T ctr)
        {

        }
    }

    public interface IGameStateController : IStateController
    {
        PointerState GetPointerState();
        InteractionState GetInteractionState();
    }

    public static class StateControllerExtensions
    {
        public static bool IsActive(this IGameStateController controller)
        {
            return controller != null && GameStateManager.Inst.StateStack.Active == controller;
        }
        public static bool IsBuried(this IGameStateController controller)
        {
            return controller != null && GameStateManager.Inst.StateStack.Active != controller && GameStateManager.Inst.StateStack.Stack.Contains(controller);
        }
        public static bool IsActiveOrBuried(this IGameStateController controller)
        {
            return controller != null && GameStateManager.Inst != null && GameStateManager.Inst.StateStack != null && (GameStateManager.Inst.StateStack.Active == controller || GameStateManager.Inst.StateStack.Stack.Contains(controller));
        }
    }

    public class GameStateStack : StateStack<IGameStateController>
    {
        protected override void OnPush(IGameStateController ctr)
        {
            base.OnPush(ctr);
            EventManager.Raise<GameStateEvent>(new GameStateEvent { State = Active });
        }

        protected override void OnForceTo(IGameStateController ctr)
        {
            base.OnForceTo(ctr);
            EventManager.Raise<GameStateEvent>(new GameStateEvent { State = Active });
        }

        protected override void OnPop(IGameStateController ctr)
        {
            base.OnPop(ctr);
            EventManager.Raise<GameStateEvent>(new GameStateEvent { State = Active });
        }
    }

    public class DefaultGameState : IGameStateController
    {
        public InteractionState GetInteractionState()
        {
            return InteractionState.GUI;
        }

        public PointerState GetPointerState()
        {
            return PointerState.GUI;
        }

        public void OnGainControl()
        {
        }

        public void OnLoseControl()
        {
        }
    }

    public class GameStateManager
    {
        public GameStateStack StateStack = new GameStateStack();
        public IGameStateController Active => StateStack.Active;

        public static void Push(IGameStateController ctr)
        {
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] current state: {Inst.Active?.GetType().Name} pushing: {ctr.GetType().Name}");
            Inst.StateStack.Push(ctr);
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] new state: {Inst.Active?.GetType().Name}");
        }

        public static void ForceTo(IGameStateController ctr)
        {
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] current state: {Inst.Active?.GetType().Name} forcing to: {ctr.GetType().Name}");
            Inst.StateStack.ForceTo(ctr);
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] new state: {Inst.Active?.GetType().Name}");

        }

        public static void Remove(IGameStateController ctr)
        {
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] current state: {Inst.Active?.GetType().Name} popping: {ctr.GetType().Name}");
            Inst.StateStack.Remove(ctr);
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] new state: {Inst.Active?.GetType().Name}");
        }

        public static void Pop(IGameStateController ctr)
        {
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] current state: {Inst.Active?.GetType().Name} popping: {ctr.GetType().Name}");
            Inst.StateStack.Pop(ctr);
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] new state: {Inst.Active?.GetType().Name}");
        }
        public static void ForcePop(IGameStateController ctr)
        {
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] current state: {Inst.Active?.GetType().Name} popping: {ctr.GetType().Name}");
            Inst.StateStack.ForceTo(ctr);
            Inst.StateStack.Pop(ctr);
            Logger.Log(Channel.Gameplay, LogPriority.Trace, $"[GameState] new state: {Inst.Active?.GetType().Name}");
        }

        public static GameStateManager Inst => G.S.StateManager;

        public GameStateManager()
        {
            StateStack.Push(new DefaultGameState());
        }
    }
}