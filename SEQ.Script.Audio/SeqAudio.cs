// MIT License

using SEQ.Script.Core;

namespace SEQ.Script.Audio
{
    public static class SeqAudio
    {
        public static void Init()
        {
            ReverbZoneManager.ResetZones();
            Shell.Add(CommandsAudio.Commands);
        }
    }
}
