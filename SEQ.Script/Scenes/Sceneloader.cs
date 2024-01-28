

using SEQ.Script.Core;
using Stride.Core;
using Stride.Core.Serialization;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEQ.Script
{
    // TODO: his shouldn't be a singleton
    public class Sceneloader : StartupScript
    {
        public static Sceneloader Instance;
        public string SeqId;
        SeqId AsSeqId => new SeqId(SeqId);
        [DataContract]
        public class SceneListEntry
        {
            public string SeqId;
            public UrlReference<Scene> Reference;
            [DataMemberIgnore]
            public Scene Scene;
            [DataMemberIgnore]
            public List<Entity> LinkedEnts = new List<Entity>();
        }

        public List<SceneListEntry> List = new List<SceneListEntry>();

        public event Action OnSceneChange;

        public async Task LoadScene(string id)
        {
            foreach (var s in List)
            {
                if (s.SeqId == id)
                {
                    s.Scene = await Content.LoadAsync(s.Reference);
                    s.Scene.Parent = Entity.Scene;
                    OnSceneChange?.Invoke();
                }
            }
        }

        public async Task MergeScene(string id)
        {
            foreach (var s in List)
            {
                if (s.SeqId == id)
                {
                    s.Scene = await Content.LoadAsync(s.Reference);
                    s.Scene.Parent = Entity.Scene;
                    foreach (var e in s.Scene.Entities)
                    {
                        e.Scene = Entity.Scene;
                        s.LinkedEnts.Add(e);
                    }

                    Content.Unload(s.Scene);
                    s.Scene = null;

                    OnSceneChange?.Invoke();
                }
            }
        }

        public void UnloadScene(string id)
        {
            foreach (var s in List)
            {
                if (s.SeqId == id)
                {
                    foreach (var e in s.LinkedEnts)
                    {
                        e.Destroy();
                    }
                    s.Scene.Parent = null;
                    Content.Unload(s.Scene);
                    s.Scene = null;

                    OnSceneChange?.Invoke();
                }
            }
        }



        public override void Cancel()
        {
            if (Systems.Scene != null)
            {
                Systems.Scene.Remove(AsSeqId);
            }
            base.Cancel();
        }

        public override void Start()
        {
            base.Start();
            Instance = this;
            Systems.Scene.Add(AsSeqId, Entity.Scene);

            Entity.Scene.Commands.Add("load",
                new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => await LoadScene((string)args[0]),
                    Help = "Loads a scene as a child"
                });

            Entity.Scene.Commands.Add("merge",
                new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => await MergeScene((string)args[0]),
                    Help = "Loads a scene, dumps all content into main scene, and unloads it"
                });

            Entity.Scene.Commands.Add("unload",
                new CommandInfo
                {
                    Params = [typeof(string)],
                    Exec = async args => UnloadScene((string)args[0]),
                });
        }
    }
}
