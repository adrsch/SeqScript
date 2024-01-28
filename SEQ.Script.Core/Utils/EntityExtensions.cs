// MIT License 

using Stride.Core.Mathematics;
using Stride.Engine;

namespace SEQ.Script.Core
{
    // Maybe this shouldn't be here
    public static class EntityExtensions
    {

        #region get things
        public static T GetInParent<T>(this Entity ent) where T : EntityComponent
        {
            var cur = ent;
            while (cur != null)
            {
                if (cur.Get<T>() is T asT)
                    return asT;
                cur = cur.Transform.Parent.Entity;
            }

            return default;
        }

        // TODO: search pattern iffy
        public static T GetInChildren<T>(this Entity ent) where T : EntityComponent
        {
            if (TryGetInChildren<T>(ent, out var asT))
                return asT;
            else
                return default;

        }

        static bool TryGetInChildren<T>(this Entity ent, out T asT) where T : EntityComponent
        {
            asT = ent.Get<T>();
            if (asT != null)
                return true;

            foreach (var c in ent.GetChildren())
                if (c.TryGetInChildren<T>(out asT))
                    return true;
            return false;
        }

        public static List<T> GetAllInChildren<T>(this Entity ent) where T : EntityComponent
        {
            var list = new List<T>();
            AddAllInChildren<T>(ent, list);
            return list;
        }

        static void AddAllInChildren<T>(Entity ent, List<T> list) where T : EntityComponent
        {
            list.AddRange(ent.GetAll<T>());
            foreach (var e in ent.GetChildren())
                AddAllInChildren<T>(e, list);
        }


        public static T GetInterface<T>(this Entity ent)
        {
            foreach (var c in ent.Components)
                if (c is T asT)
                    return asT;

            return default;
        }

        public static List<T> GetInterfacesInChildren<T>(this Entity ent)
        {
            var list = new List<T>();
            AddInterfacesInChildren<T>(ent, list);
            return list;
        }

        static void AddInterfacesInChildren<T>(Entity ent, List<T> list)
        {
            foreach (var c in ent.Components)
                if (c is T asT)
                    list.Add(asT);

            foreach (var e in ent.GetChildren())
                AddInterfacesInChildren<T>(e, list);
        }

        public static List<T> GetInterfacesInParent<T>(this Entity ent)
        {
            var list = new List<T>();
            AddInterfacesInParent<T>(ent, list);
            return list;
        }

        static void AddInterfacesInParent<T>(Entity ent, List<T> list)
        {
            foreach (var c in ent.Components)
                if (c is T asT)
                    list.Add(asT);

            if (ent.GetParent() is Entity p)
                AddInterfacesInParent<T>(p, list);
        }

        public static T GetInterfaceInParent<T>(this Entity ent)
        {
            foreach (var c in ent.Components)
                if (c is T asT)
                    return asT;

            var p = ent.GetParent();
            while (p != null)
            {
                foreach (var c in p.Components)
                    if (c is T asT)
                        return asT;
                p = p.GetParent();
            }

            return default;
        }
#endregion


        #region lifecycle

        public static void Destroy(this Entity ent)
        {
            ent?.EntityManager?.Remove(ent);
            return;
        }
        public static void SetParentAndZero(this Entity ent, Entity parent)
        {
            ent.SetParent(parent);
            ent.Transform.Rotation = Quaternion.Identity;
            ent.Transform.Position = Vector3.zero;
        }

        public static T InstantiateTemporary<T>(this Prefab p, Scene scene, int ms) where T : EntityComponent
        {
            var ent = p.InstantiateSingle(scene);
            Systems.Script.AddTask(async () =>
            {
                await Task.Delay(ms);

                ent?.EntityManager?.Remove(ent);
                return;
            });
            return ent?.Get<T>();
        }

        public static Entity InstantiateTemporary(this Prefab p, Scene scene, int ms)
        {
            var ent = p.InstantiateSingle(scene);
            Systems.Script.AddTask(async () =>
            {
                await Task.Delay(ms);
                ent?.EntityManager?.Remove(ent);
                return;
            });
            return ent;
        }

        public static T InstantiateSingle<T>(this Prefab p, Scene scene) where T : EntityComponent
        {
            var ents = p.InstantiateSingle(scene);
            return ents?.Get<T>();
        }

        public static Entity InstantiateSingle(this Prefab p)
        {
            var ents = p.Instantiate();
            if (ents.Count != 1)
            {
                Logger.Log(Channel.Gameplay, LogPriority.Warning, $"InstantiateSingle: Found {ents.Count} ents!");
            }
            if (ents.Count > 0)
                return ents[0];
            return null;
        }

        public static Entity InstantiateSingle(this Prefab p, ScriptComponent sc)
        {
            var e = p.InstantiateSingle();
            sc.Entity.Scene.Entities.Add(e);
            return e;
        }

        public static Entity InstantiateSingle(this Prefab p, Scene scene)
        {
            var e = p.InstantiateSingle();
            scene.Entities.Add(e);
            return e;
        }

        public static List<Entity> Instantiate(this Prefab p, Scene scene)
        {
            var ents = p.Instantiate();
            foreach (var e in ents)
                scene.Entities.Add(e);

            return ents;
        }
        #endregion
    }
}
