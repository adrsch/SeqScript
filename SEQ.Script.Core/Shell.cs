// MIT License 

using Stride.Engine;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace SEQ.Script.Core
{
    // This could be made not static, not too difficult
    public static class Shell
    {
        public static Dictionary<string, CommandInfo> Commands = new Dictionary<string, CommandInfo>();
        public static Dictionary<string, RoutineInfo> Routines = new Dictionary<string, RoutineInfo>();

        public static void Add(Dictionary<string, CommandInfo> commands)
        {
            foreach (var kvp in commands)
            {
                if (Commands.ContainsKey(kvp.Key))
                    Logger.Log(Channel.Shell, LogPriority.Warning, $"Command {kvp.Key} already exists! Overwriting command...");

                Commands[kvp.Key] = kvp.Value;
            }
        }
        /*
         * TODO finish this
        public static void GetFromAttributes()
        {
            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .ToArray();

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();
                var paramInfo = method.GetParameters();
                if (method.IsStatic)
                {
                    Commands.Add(attr.Name, new CommandInfo
                    {
                        Help = attr.Help,
                        Exec = method.ReturnType == typeof(Task)
                        ? async () =>
                        {

                        }
                    });
                }
            }

            var fields = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .SelectMany(t => t.GetFields())
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .ToArray();
            var properties = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .SelectMany(t => t.GetProperties())
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
                .ToArray();
        }
        */

        public static void Init()
        {
            Routines = new Dictionary<string, RoutineInfo>();
            Add(CommandsCore.Commands);
        }

        public static void Exec(string cmds, string debuginfo = null)
        {
            if (string.IsNullOrWhiteSpace(cmds))
                return;
            Sequencer.S.Thread(cmds, debuginfo);
       //     var runAll = GetExecAllAction(cmds);
        //    runAll();
        }
        public static async Task ExecAsync(string cmds, string debuginfo = null)
        {
            if (string.IsNullOrWhiteSpace(cmds))
                return;
            await Sequencer.S.ThreadAsync(cmds, debuginfo);
            //     var runAll = GetExecAllAction(cmds);
            //    runAll();
        }


        /*    public static void Alias(string name, string[] cmds)
            {
                Aliases[name] = () =>
                {
                    foreach (var cmd in cmds)
                        if (!string.IsNullOrWhiteSpace(cmd))
                            Exec(cmd);
                };
            }*/

        static StringBuilder Sb = new StringBuilder();

        public static string GetRestOfStatement(ref int i, string seq)
        {
            Sb.Clear();
            while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
            {
                if (seq[i] == '\\')
                {
                    i++;
                    Sequencer.SkipWhitespaceAndOneNewline(ref i, seq);
                }
                else
                {
                    Sb.Append(seq[i]);
                    i++;
                }
            }
            return Sb.ToString();
        }

        public static string GetNextArg(ref int i, string seq)
        {
            Sb.Clear();

            Sequencer.SkipWhitespace(ref i, seq);

            if (seq[i] == '"')
            {
                i++;

                while (i < seq.Length && seq[i] != '"')
                {
                    Sb.Append(seq[i]);
                    i++;
                }
                i++;
            }
            else
            {
                while (i < seq.Length && !char.IsWhiteSpace(seq[i]) && seq[i] != ';' && seq[i] != ')' && seq[i] != '\n')
                {
                    Sb.Append(seq[i]);
                    i++;
                }
            }
            Sequencer.SkipWhitespace(ref i, seq);
            return Sb.ToString();
        }

        public static bool TryGetCommands(string[] args, out CommandInfo info)
        {
            if (Commands.TryGetValue(args[0].ToLowerInvariant(), out info))
                return true;

            if (Systems.Scene.Root.Commands.TryGetValue(args[0], out var rootc))
            {
                info = rootc;
                return true;
            }


            var split = args[0].Split(':');

            var seqId = (SeqId)split[0];

            if (split.Length > 1)
            {
                if (split.Length == 2 && ActorRegistry.Active.TryGetValue(split[0], out var a))
                {
                    if (a.Entity.Commands.TryGetValue(split[1], out info))
                    {
                        return true;
                    }
                }

                Scene scene = Systems.Scene.Root;
                var i = 0;
                if (Systems.Scene.Scenes.TryGetValue(seqId, out var s))
                {
                    scene = s;
                    i = 1;

                    if (split.Length == 2 &&
                        s.Commands.TryGetValue(split[1], out info))
                        return true;
                }
                Entity ent = scene.GetEntity((SeqId)split[i]);
                while (i < split.Length - 1 && ent != null)
                {
                    ent = ent.GetChild((SeqId)split[i]);
                    i++;
                }
                if (ent == null)
                    return false;

                if (ent.Commands.TryGetValue(split[split.Length - 1], out info))
                    return true;
            }
            return false;
        }

       public static Func<Task> GetRunCommandAction(string[] args)
        {
           // if (Logger.Verbose)
                Logger.Log(Channel.Shell, LogPriority.Info, string.Join(" ", args));

            if (TryGetCommands(args, out var comamndInfo))
            {
                object[] parameters = null;

                var requiredParams = comamndInfo.Params;
                var optional = comamndInfo.OptionalParams;
                if (requiredParams?.Length > 0)
                    parameters = optional != null ? new object[requiredParams.Length + optional.Length] : requiredParams != null ? new object[requiredParams.Length] : new object[0];
                else
                    parameters = new object[0];

                if (args.Length - 1 < requiredParams?.Length)
                {
                    Logger.Log(Channel.Shell, LogPriority.Error, $"Too few arguments.\n{string.Join(" ", args)}\nRequired: {string.Join(" ", requiredParams?.Select(x => x.Name))}");
                    return null;
                }

                for (var i = 0; i < parameters.Length; i++)
                {
                    var paramType = (i < requiredParams?.Length) ? requiredParams[i] : optional[i - requiredParams.Length];

                    if (args.Length > i + 1)
                    {
                        if (TryParse(args[i + 1], paramType, out var result))
                        {
                            parameters[i] = result;
                        }
                        else
                        {
                            Logger.Log(Channel.Shell, LogPriority.Error, $"Could not parse argument {args[i + 1]} as {paramType}");
                            return null;
                        }
                    }
                    else
                    {
                        parameters[i] = paramType.IsClass ? null :
                            Activator.CreateInstance( paramType);
                    }
                }
                
                return () => comamndInfo.Exec(parameters);
            }
            /*if (Aliases.TryGetValue(args[0].ToLowerInvariant(), out var alias) || Aliases.TryGetValue(args[0].ToKebabCase(), out alias))
            {
                return alias;
            }*/

            Logger.Log(Channel.General, LogPriority.Error, $"Could not find command {args[0]}");
            return null;
        }

        public static void Help()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder line = new StringBuilder();
            sb.AppendLine("Commands:");
            foreach (var kv in Commands)
            {
                var name = kv.Key;
                var method = kv.Value;
                line.Append($"   {name}");
                var requiredParams = method.Params;
                if (requiredParams != null)
                    foreach (var param in requiredParams)
                    {
                        if (line.Length + param.Name.Length >= 30 && !string.IsNullOrWhiteSpace(method.Help))
                            line.Append("\n      ");
                        line.Append($" [{param.Name}]");
                    }

                var baseLen = Math.Min(50, line.Length);
                //while (line.Length < 100)
                //line.Append(" ");
                var didNewLine = false;
                if (!string.IsNullOrWhiteSpace(method.Help))
                {
                    var counter = line.Length;
                    foreach (var c in method.Help)
                    {
                        if (counter % 150 == 0)
                        {
                            line.Append("\n");
                            while (counter % 150 < baseLen)
                            {
                                line.Append(" ");
                                counter++;
                            }
                        }
                        while (counter % 150 < 30)
                        {
                            line.Append(didNewLine ? ' ' : "-");
                            counter++;
                        }
                        didNewLine = true;

                        line.Append(c);
                        counter++;
                    }
                    //line.Append(method.Help);
                }

                sb.AppendLine(line.ToString());
                line.Clear();
            }

            Logger.Log(Channel.Shell, LogPriority.Info, sb.ToString());
        }

        public static bool TryParse(string arg, Type t, out object result)
        {
            var converter = TypeDescriptor.GetConverter(t);
            try
            {
                result = converter.ConvertFromInvariantString(arg);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log(Channel.Shell, LogPriority.Error, $"Command.Parse: Could not parse argument {arg} as {t}");
                result = null;
                return false;
            }
        }
    }

    public struct RoutineInfo
    {
        public List<string> Params;
        public Action<string, string[], Action> Exec;
        public SequencerContext Ctx;
    }

    public enum SymbolType
    {
        Command,
        If,
    }
    public struct ParseInfo
    {

    }
}