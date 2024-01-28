// MIT License 

using Calcc;
using SEQ.Script.Core;
using Stride.Core.Mathematics;
using System.ComponentModel;

namespace SEQ.Script.Core
{
    // TODO: ActorState should be separate


    // TODO delete this, dont know why it is still here
    public interface ICvarListener
    {
    }

    [Serializable]
    public class ActorState
    {
        public string SeqId;
        public string Species;
        public bool InWorld;
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Rotation;
        public int Quantity;
        public Dictionary<string, string> Vars = new Dictionary<string, string>();
        public List<string> Children = new List<string>();
        public string Parent;

        public T GetVar<T>(string id)
        {
            if (Vars.TryGetValue(id, out var val))
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)val;
                else if (typeof(T) == typeof(bool))
                    return (T)(object)Convert.ToBoolean(val);
                else if (typeof(T) == typeof(int))
                    return (T)(object)Convert.ToInt32(val);
                else if (typeof(T) == typeof(float))
                    return (T)(object)Convert.ToSingle(val);
                else if (typeof(T).IsEnum)
                {
                    if (Enum.TryParse(typeof(T), val,true, out var rt))
                        return (T)rt;
                    else
                        return default;

                }
                else
                {
                    var c = TypeDescriptor.GetConverter(typeof(string));
                    return (T)c.ConvertTo(val, typeof(T));
                }
            }
            return default;
        }

        public Quaternion GetProjectionMeshOrientation()
        {
            return new Quaternion(GetVar<float>("x"), GetVar<float>("y"), GetVar<float>("z"), GetVar<float>("w"));
        }
        public void SetProjectionMeshOrientation(Quaternion q)
        {
            Vars["x"] = q.X.ToString();
            Vars["y"] = q.Y.ToString();
            Vars["z"] = q.Z.ToString();
            Vars["w"] = q.W.ToString();
        }

        public ActorState GetParent()
        {
            if (!string.IsNullOrWhiteSpace(Parent))
            {
                return ActorState.Get(Parent);
            }
            return null;

        }

        public ActorState GetChild(int i)
        {
            if (i < Children.Count)
            {
                return Get(Children[i]);
            }
            return null;
        }

        public void AddChild(string id)
        {
            var ch = Get(id);
            if (ch != null)
            {
                AddInternal(id);

                ch.Parent = SeqId;
                OnChanged();
            }
            else
            {
                Logger.Log(Channel.Data, LogPriority.Error, $"Trying to add a child that doesnt exist: {id} to parent {SeqId}");
            }

        }

        public void AddChild(ActorState ch)
        {
            if (ch != null)
            {
                AddInternal(ch.SeqId);

                ch.Parent = SeqId;
                OnChanged();
            }
            else
            {
                Logger.Log(Channel.Data, LogPriority.Error, $"Trying to add a child that doesnt exist: {ch.SeqId} to parent {SeqId}");
            }

        }

        void AddInternal(string id)
        {
            var i = 0;
            foreach (var c in Children)
            {
                if (string.IsNullOrWhiteSpace(c))
                {
                    Children[i] = id;
                    return;
                }
                i++;
            }
            Children.Add(id);
        }
        public void RemoveChild(string id)
        {
            var ch = Get(id);
            if (ch != null)
            {
                ch.Parent = null;
            }
            else
            {
                Logger.Log(Channel.Data, LogPriority.Warning, $"Trying to remove a child that doesnt exist: {id} from parent {SeqId}");
            }
            var i = 0;
            while (i < Children.Count)
            {
                if (Children[i] == id)
                    Children[i] = "";
                i++;
            }
            OnChanged();
        }

        public string GetNameWithQuantity()
        {
            if (Quantity > 1)

                return $"{Loc.Get(Species)} ({Quantity})";
            else

                return Loc.Get(Species);
        }

        public string GetDescription()
        {
            return Loc.Get($"{Species}-desc");
        }

        public void OnChanged()
        {
            InvokeActorOnChanged();
            if (Cvars.Listeners.Entries.TryGetValue(SeqId, out var laa))
                foreach (var lis in laa)
                    lis.OnValueChanged?.Invoke();
        }

        public void InvokeActorOnChanged()
        {
            if (!string.IsNullOrWhiteSpace(SeqId) && ActorRegistry.Active.TryGetValue(SeqId, out var es))
                es.OnChanged();
        }

        public static ActorState Get(string id)
        {

            if (!string.IsNullOrWhiteSpace(id) && Cvars.Current.Actors.TryGetValue(id, out var es))
                return es;
            return default;
        }

        public static Actor GetWorld(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && ActorRegistry.Active.TryGetValue(id, out var es))
                return es;
            return default;
        }

        public IActorSpecies GetSpecies()
        {
            if (ActorSpeciesRegistry.TryGetSpecies(Species, out var es))
                return es;
            return default;
        }

        public T GetSpecies<T>() where T : IActorSpecies
        {
            if (ActorSpeciesRegistry.TryGetSpecies(Species, out var es))
                return (T)es;
            return default;
        }

        public void DestroyWorld()
        {
            if (ActorRegistry.Active.TryGetValue(SeqId, out var es))
                es.DestroyWorld();
        }

        public void Destroy()
        {
            DestroyWorld();
            if (!string.IsNullOrWhiteSpace(Parent))
            {
                var p = Get(Parent);
                p.RemoveChild(SeqId);
            }
            Cvars.Current.Actors.Remove(SeqId);
        }

        public void UpdateWorld()
        {
            if (ActorRegistry.Active.TryGetValue(SeqId, out var es))
                es.LoadFromState();
        }

        public void AddListener(CvarListenerInfo info)
        {
            if (string.IsNullOrWhiteSpace(SeqId))
                return;
            if (Cvars.Listeners.Entries.TryGetValue(SeqId, out var l))
            {
                l.Add(info);
            }
            else
            {
                Cvars.Listeners.Entries[SeqId] = new List<CvarListenerInfo>();
                Cvars.Listeners.Entries[SeqId].Add(info);
            }

            info.OnValueChanged();
        }

        public void RemoveListener(ICvarListener a)
        {
            if (!string.IsNullOrWhiteSpace(SeqId) && Cvars.Listeners.Entries.TryGetValue(SeqId, out var l))
            {
                l.RemoveAll(x => x.Listener == a);
            }
        }

        public static ActorState AddNew(string seqid)
        {
            var ent = new ActorState { 
                SeqId = !String.IsNullOrWhiteSpace(seqid) ? seqid : System.Guid.NewGuid().ToString()
                };
            Cvars.Current.Actors[ent.SeqId] = ent;
            return ent;

        }
    }

    [Serializable]
    public class CvarMap
    {
        public int Build;
        public DateTime SaveTime;
        public Dictionary<string, string> Entries = new Dictionary<string, string>();
        public Dictionary<string, ActorState> Actors = new Dictionary<string, ActorState>();
    }

    public struct CvarListenerInfo
    {
        public ICvarListener Listener;
        public Action OnValueChanged;
    }
    public class CvarListenerRegistery
    {
        public Dictionary<string, List<CvarListenerInfo>> Entries = new Dictionary<string, List<CvarListenerInfo>>();
    }


    [Serializable]
    public enum CvarTest
    {
        Invalid = 0,
        EqualsVal = 1,
        NotEqualsVal = 2,
        EqualsCvar = 3,
        NotEqualsCvar = 4,
        GreaterThanVal = 5,
        LessThanVal = 6,
        GreaterOrEqualVal = 7,
        LessOrEqualVal = 8,
        GreaterThanCvar = 9,
        LessThanCvar = 10,
        GreaterOrEqualCvar = 11,
        LessOrEqualCvar = 12,
        IsDef = 13,
    }

    public class SetCvarsEvent
    {

    }

    public static class Cvars
    {

        public static CvarMap Current = new CvarMap();
        public static CvarListenerRegistery Listeners = new CvarListenerRegistery();
        public static Dictionary<string, string> Temps = new Dictionary<string, string>();

        public static void Set(string cvar, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null")
            {
                if (Current.Entries.ContainsKey(cvar))
                    Current.Entries.Remove(cvar);

                if (Temps.ContainsKey(cvar))
                    Temps.Remove(cvar);

                var ent = cvar.Split(":");
                if (ent.Length > 1 && Current.Actors.TryGetValue(ent[0], out var estatea))
                {
                    estatea.Vars.Remove(ent[1]);

                    estatea.InvokeActorOnChanged();
                    if (Listeners.Entries.TryGetValue(ent[0], out var laa))
                        foreach (var lis in laa)
                            lis.OnValueChanged?.Invoke();
                }

                if (Listeners.Entries.TryGetValue(cvar, out var la))
                    foreach (var lis in la)
                        lis.OnValueChanged?.Invoke();
                return;
            }
            var ents = cvar.Split(":");
            if (ents.Length > 1)
            {
                if (ents.Length > 2)
                    Logger.Log(Channel.Shell, LogPriority.Error, $"Too many ':' not valid for cvar {cvar}");
                else
                {
                    if (Current.Actors.TryGetValue(ents[0], out var estate))
                    {
                        estate.Vars[ents[1]] = value;

                        estate.InvokeActorOnChanged();
                        if (Listeners.Entries.TryGetValue(ents[0], out var laa))
                            foreach (var lis in laa)
                                lis.OnValueChanged?.Invoke();
                    }
                    else
                    {
                        Logger.Log(Channel.Shell, LogPriority.Error, $"couldn't find entitiy for {cvar} to set val to {value}");
                    }
                }
            }
            else
            {
                if (Temps.ContainsKey(cvar))
                {
                    Temps[cvar] = value;
                }
                else
                {
                    Current.Entries[cvar] = value;
                }
            }

            if (Listeners.Entries.TryGetValue(cvar, out var l))
                foreach (var lis in l)
                    lis.OnValueChanged?.Invoke();
        }

        public static void Add(string cvar, int amt)
        {
            if (!Cvars.Current.Entries.ContainsKey(cvar))
            {
                Cvars.Set(cvar, amt.ToString());
            }
            else if (TryGet<int>(cvar, out var o))
            {
                Cvars.Set(cvar, (o + amt).ToString());
            }
        }

        public static T Eval<T>(string exp)
        {
            var e = new Expression(exp.Replace(':', '_'));
            e.EvaluateParameter += (name, args) =>
            {
                if (Current.Entries.TryGetValue(name, out var val) || Temps.TryGetValue(name, out val))
                {
                    args.Result = val;
                }
                else
                {
                    var ents = name.Split("_");
                    if (ents.Length > 1 && Current.Actors.TryGetValue(ents[0], out var estate))
                    {
                        if (estate.Vars.TryGetValue(ents[1], out var evar))
                        {
                            args.Result = evar;
                        }
                        else
                        {
                            args.Result = false;
                        }
                    }
                    else
                    {
                        args.Result = false;
                    }
                }
            };
            return (T)e.Evaluate();
        }

        public static void EvalAndSet(string cvar, string exp)
        {
            Set(cvar, Eval<object>(exp).ToString());
        }

        public static void SetCurrent(CvarMap cur)
        {
            Current = cur;
            Temps = new Dictionary<string, string>();
            foreach (var kvp in Listeners.Entries)
            {
                foreach (var lis in kvp.Value)
                {
                    lis.OnValueChanged?.Invoke();
                }
            }
            EventManager.Raise(new SetCvarsEvent());
        //    EventRegistrar.DoOnLoadReset();
        }

        public static string Get(string cvar)
        {
            if (Current.Entries.TryGetValue(cvar, out var inf) || Temps.TryGetValue(cvar, out inf))
            {
                return inf;
            }
            var ents = cvar.Split(":");
            if (ents.Length > 1 && Current.Actors.TryGetValue(ents[0], out var estate))
            {
                if (estate.Vars.TryGetValue(ents[1], out var evar))
                {
                    return evar;
                }
            }
            return default(string);
        }

        public static T Get<T>(string cvar)
        {
            if (Current.Entries.TryGetValue(cvar, out var val) || Temps.TryGetValue(cvar, out val))
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)val;
                else if (typeof(T) == typeof(bool))
                    return (T)(object)Convert.ToBoolean(val);
                else if (typeof(T) == typeof(int))
                    return (T)(object)Convert.ToInt32(val);
                else if (typeof(T) == typeof(float))
                    return (T)(object)Convert.ToSingle(val);
                else
                {
                    var c = TypeDescriptor.GetConverter(typeof(string));
                    return (T)c.ConvertTo(val, typeof(T));
                }
            }
            var ents = cvar.Split(":");
            if (ents.Length > 1 && Current.Actors.TryGetValue(ents[0], out var estate))
            {
                if (estate.Vars.TryGetValue(ents[1], out var evar))
                {
                    if (typeof(T) == typeof(string))
                        return (T)(object)evar;
                    else if (typeof(T) == typeof(bool))
                        return (T)(object)Convert.ToBoolean(evar);
                    else if (typeof(T) == typeof(int))
                        return (T)(object)Convert.ToInt32(evar);
                    else if (typeof(T) == typeof(float))
                        return (T)(object)Convert.ToSingle(evar);
                    else
                    {
                        var c = TypeDescriptor.GetConverter(typeof(string));
                        return (T)c.ConvertTo(evar, typeof(T));
                    }
                }
            }
            return default(T);
        }

        public static bool TryGet<T>(string cvar, out T rt)
        {
            if (Current.Entries.TryGetValue(cvar, out var val) || Temps.TryGetValue(cvar, out val))
            {
                rt = Get<T>(cvar);
                return true;
            }
            var ents = cvar.Split(":");
            if (ents.Length > 1 && Current.Actors.TryGetValue(ents[0], out var estate))
            {
                if (estate.Vars.TryGetValue(ents[1], out var evar))
                {
                    rt = Get<T>(cvar);
                    return true;
                }
            }
            rt = default(T);
            return false;
        }

        public static bool TestOld(string cvar, string value, CvarTest test)
        {
            return false;
        }

        public static bool Test(string sb)
        {
            var isbool = Boolean.TryParse(SequencerContext.EvalWithoutCtx(sb), out var passed);
            return isbool && passed;
        }

        public static void ResetAll()
        {
            Current.Entries.Clear();
            Current.Actors.Clear();

        }
    }
}