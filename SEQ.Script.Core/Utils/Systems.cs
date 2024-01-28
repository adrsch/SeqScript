// MIT License

using Stride.Core;
using Stride.Engine;
using Stride.Engine.Processors;

namespace SEQ.Script.Core
{
    // None of this is good, it shouldn't exist
    public static class Systems
    {
        public static SceneSystem Scene => Sequencer.S.Services.GetSafeServiceAs<SceneSystem>();
        public static ScriptSystem Script => Sequencer.S.Services.GetSafeServiceAs<ScriptSystem>();
    }
}
