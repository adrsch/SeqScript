// MIT License

using System.Globalization;
using System.Text;

namespace SEQ.Script.Core
{
    [System.Flags]
    public enum Channel
    {
        General = 1 << 0,
        Shell = 1 << 1,
        Audio = 1 << 2,
        Input = 1 << 3,
        Filesystem = 1 << 4,
        Gameplay = 1 << 5,
        Data = 1 << 6,
        AI = 1 << 7,
        Sequencer = 1 << 8,
        UI = 1 << 9,
        Modules = 1 << 10,
        Physics = 1 << 11,

        All = int.MaxValue
    }

    public static class LoggerExtensions
    {
        public static string GetTag(this Channel channel)
        {
            switch (channel)
            {
                default:
                case Channel.General:
                    return "";
                case Channel.Shell:
                    return "> ";
                case Channel.Audio:
                    return "[Audio] ";
                case Channel.Input:
                    return "[Input] ";
                case Channel.Gameplay:
                    return "[Gameplay] ";
                case Channel.Filesystem:
                    return "[Filesystem] ";
                case Channel.Data:
                    return "[Data] ";
                case Channel.AI:
                    return "[AI] ";
                case Channel.UI:
                    return "[UI] ";
                case Channel.Sequencer:
                    return "[Seq] ";
                case Channel.Physics:
                    return "[Physics] ";
            }
        }

        public static string GetOpenTag(this LogPriority priority)
        {
            switch (priority)
            {
                default:
                case LogPriority.Info:
                    return "";
                case LogPriority.Warning:
                    return "**DANGER**";
                case LogPriority.Error:
                    return "!!! ERROR !!! ";
            }
        }

        public static string GetCloseTag(this LogPriority priority)
        {
            switch (priority)
            {
                default:
                case LogPriority.Info:
                    return "";
                case LogPriority.Warning:
                    return "**DANGER**";
                case LogPriority.Error:
                    return " !!! ERROR !!!";
            }
        }
    }

    public enum LogPriority
    {
        Info = 1 << 0,
        Warning = 1 << 1,
        Error = 1 << 2,
        Trace = 1 << 3,

        All = int.MaxValue
    }

    public class LogDisplayOptions
    {
        public Channel channelFilter = Channel.All;
        public LogPriority priorityFilter = LogPriority.All;
        public bool showTimeStamp = true;
        public string timeFormat = "T";
    }

    public class LogEntry
    {
        public Channel channel;
        public LogPriority priority;
        public string message;
        public DateTime time;

        public void Append(LogDisplayOptions opts, ref StringBuilder sb)
        {
            if ((channel & opts.channelFilter) == 0 || (priority & opts.priorityFilter) == 0)
                return;

            if (opts.showTimeStamp)
                sb.Append($"[{time.ToString(opts.timeFormat, DateTimeFormatInfo.InvariantInfo)}] ");

            sb.Append(priority.GetOpenTag());
            sb.Append(channel.GetTag());
            sb.Append(message);
            sb.Append(priority.GetCloseTag());

            sb.Append('\n');
        }
    }

    // TODO: get channels actually working, add log file, etc
    public static class Logger
    {
        static List<LogEntry> History = new List<LogEntry>();
        static Dictionary<LogPriority, int> MessageCounts = new Dictionary<LogPriority, int>();

        public static event System.Action OnLogUpdated;
        public static event System.Action<LogEntry> OnLog;

        public static void Reset()
        {
            History.Clear();
            MessageCounts.Clear();
            OnLogUpdated = null;
            OnLog = null;
        } 
        public static async Task ClearLogs()
        {
            History.Clear();
            MessageCounts.Clear();
            OnLogUpdated?.Invoke();
        }

        public static int GetCount(LogPriority priority)
        {
            if (!MessageCounts.ContainsKey(priority))
                MessageCounts[priority] = 0;

            return MessageCounts[priority];
        }

        public static string GetLogs(LogDisplayOptions opts)
        {
            var sb = new StringBuilder();

            foreach (var entry in History)
                entry.Append(opts, ref sb);

            return sb.ToString();
        }

        public static List<LogEntry> GetLogEntries()
        {
            return History;
        }

        public static bool Verbose = false;
        public static void Log(Channel channel, LogPriority priority, string message)
        {
            //  if (priority == LogPriority.Error)
            //        Debug.LogError($"[{channel}] {message}");
            //    else if (priority == LogPriority.Warning)
            //       Debug.LogWarning($"[{channel}] {message}");
            //  else
            //     Debug.Log($"[{channel}] {message}");
            if (!Verbose && priority == LogPriority.Trace)
                return;

            var entry = new LogEntry { channel = channel, priority = priority, message = message, time = DateTime.Now };
            if (!MessageCounts.ContainsKey(priority))
                MessageCounts[priority] = 0;
            MessageCounts[priority]++;
            History.Add(entry);
            OnLogUpdated?.Invoke();
            OnLog?.Invoke(entry);
            PrintLog(entry);
            
        }
        static void PrintLog(LogEntry entry)
        {
       //     if (entry.priority == LogPriority.Error)
        //        Debug.LogError($"[{entry.channel}] {entry.message}");
         //   else if (entry.priority == LogPriority.Warning)
          //      Debug.LogWarning($"[{entry.channel}] {entry.message}");
           // else
             //   Debug.Log($"[{entry.channel}] {entry.message}");
        }

        public static void Print(string arg)
        {
            Log(Channel.General, LogPriority.Info, arg);
        }
        /*
        [Command("Empty the log")]
        public static void Clear()
        {
            History.Clear();
            MessageCounts.Clear();
            OnLogUpdated?.Invoke();
        }
        */
    }
}