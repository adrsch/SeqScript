// MIT License 

using Calcc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Stride.Engine;
using Stride.Core;
using System.Threading.Tasks;
using Stride.Core.Mathematics;using SEQ.Script.Core;

namespace SEQ.Script.Core
{
    // TODO: Rewrite this using Antlr, get rid of NCalc aka Calcc
    // Everything about this entire file is wrong

    public class SequencerScope
    {
        public Dictionary<string, string> Variables;

        public bool Contains(string name)
        {
            return Variables != null && Variables.ContainsKey(name);
        }

        public bool TryGet(string name, out string value)
        {
            value = default;
            return Variables != null && Variables.TryGetValue(name, out value);
        }
        public void Set(string name, string value)
        {
            if (Variables == null)
                Variables = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(value) || value == "null")
                Variables.Remove(name);
            else
                Variables[name] = value;
        }
    }
    public class SequencerContext
    {
        public SequencerContext Clone()
        {
            var ctx = new SequencerContext();
            ctx.DebugInfo = DebugInfo;
            foreach (var scope in Stack)
            {
                var clone = new SequencerScope();
                if (scope.Variables != null)
                {
                    clone.Variables = new Dictionary<string, string>();
                    foreach (var kvp in scope.Variables)
                    {
                        clone.Variables[kvp.Key] = kvp.Value;
                    }
                }
                ctx.Stack.AddLast(clone);
            }

            return ctx;
        }

        public string DebugInfo;
        public LinkedList<SequencerScope> Stack = new LinkedList<SequencerScope>();

        public SequencerContext()
        {
            Stack.AddFirst(new SequencerScope());
        }
        public void AddDepth()
        {
            Stack.AddFirst(new SequencerScope());
        }

        public void RemoveDepth()
        {
            if (Stack.Count == 0)
            {
                Error("2 less depth than u should have wtf");
            }
            else
            {
                Stack.RemoveFirst();

                if (Stack.Count == 0)
                {
                    Error("you probably hhave a } w/o a correspodning {");
                }
            }
        }

        public void Error(string message)
        {
            Logger.Log(Channel.Sequencer, LogPriority.Error, $"{DebugInfo}: {message}");
        }

        public void Local(string cvar, string value)
        {
            Stack.First.Value.Set(cvar, value);
        }

        public void Set(string cvar, string value)
        {
            foreach (var scope in Stack)
            {
                if (scope.Contains(cvar))
                {
                    scope.Set(cvar, value);
                    return;
                }
            }

            Cvars.Set(cvar, value);
        }

        public string Eval(string exp)
        {
            var e = new Expression(exp.Replace(':', '_'));
            e.EvaluateParameter += (name, args) =>
            {
                foreach (var scope in Stack)
                {
                    if (scope.TryGet(name, out var d))
                    {
                        args.Result = d;
                        return;
                    }
                }
                if (Cvars.Current.Entries.TryGetValue(name, out var data) || Cvars.Temps.TryGetValue(name, out data))
                {
                    args.Result = data;
                }
                else
                {
                    var ents = name.Split("_");
                    if (ents.Length > 1 && Cvars.Current.Actors.TryGetValue(ents[0], out var estate))
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
            return e.Evaluate().ToString();
        }

        public static string EvalWithoutCtx(string exp)
        {
            var e = new Expression(exp.Replace(':', '_'));
            e.EvaluateParameter += (name, args) =>
            {
                if (Cvars.Current.Entries.TryGetValue(name, out var data) || Cvars.Temps.TryGetValue(name, out data))
                {
                    args.Result = data;
                }
                else
                {
                    var ents = name.Split("_");
                    if (ents.Length > 1 && Cvars.Current.Actors.TryGetValue(ents[0], out var estate))
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
            return e.Evaluate().ToString();
        }
    }
    public class Sequencer : StartupScript
    {
        public bool ContinueReceived;

        public StringBuilder Sb = new StringBuilder();

        public static Sequencer S;

        public override void Start()
        {
            S = this;
            ScriptRunner.RunnerEntity = Entity;
        }

        public void PlayRoutine(SequencerContext ctx, string seq, Action onComplete = null)
        {
            var r = new SequencerRoutine();
            r.Init(ctx, seq, onComplete);
            Entity.Add(r);
        }


        public static void DoTween(Color init, Color end, float duration, Action<Color> set, Action onEnd = null) => S.PlayTween(init, end, duration, set, onEnd);

        public void PlayTween(Color init, Color end, float duration, Action<Color> set, Action onEnd = null)
        {
            var r = new TweenRoutine
            { 
                tweenType = TweenType.colorTween,
                initC = init,
                endC = end,
                duration = duration,
                setC = set,
                onEnd = onEnd
            };
            Entity.Add(r);
        }

        public static void DoTween(float init, float end, float duration, Action<float> set, Action onEnd = null) => S.PlayTween(init, end, duration, set, onEnd);
        public void PlayTween(float init, float end, float duration, Action<float> set, Action onEnd = null)
        {
            var r = new TweenRoutine
            {
                tweenType = TweenType.floatTween,
                initF = init,
                endF = end,
                duration = duration,
                setF = set,
                onEnd = onEnd
            };
            Entity.Add(r);
        }

        public void Thread(string seq, string debuginfo = null)
        {
            var ctx = new SequencerContext();
            ctx.DebugInfo = debuginfo;
            PlayRoutine(ctx, seq);
        }

        public async Task ThreadAsync(string seq, string debuginfo = null)
        {
            var ctx = new SequencerContext();
            ctx.DebugInfo = debuginfo;
          //  PlayRoutine(ctx, seq);
            await ExecuteAsync(ctx, seq);
        }

        public void Thread(SequencerContext ctx, string seq, Action onComplete)
        {
            PlayRoutine(ctx, seq, onComplete);
        }

        public static void Continue()
        {
            S.ContinueReceived = true;
        }

        public static bool Match(string seq, int i, string match)
        {
            var position = i;
            foreach (var c in match)
            {
                if (position >= seq.Length || seq[position] != c)
                    return false;

                position++;
            }
            return true;
        }

        public static bool EatType(string seq, ref int i, out Type type)
        {
            if (Eat(seq, ref i, "int"))
            {
                type = typeof(int);
                return true;
            }
            if (Eat(seq, ref i, "float"))
            {
                type = typeof(float);
                return true;
            }
            if (Eat(seq, ref i, "string"))
            {
                type = typeof(string);
                return true;
            }
            if (Eat(seq, ref i, "bool"))
            {
                type = typeof(bool);
                return true;
            }

            type = default;
            return false;
        }

        public static bool Eat(string seq, ref int i, string match)
        {
            var position = i;
            foreach (var c in match)
            {
                if (position >= seq.Length || seq[position] != c)
                    return false;

                position++;
            }
            i = position;
            return true;
        }


        public static void SkipWhitespace(ref int i, string seq)
        {
            while (i < seq.Length && (
                seq[i] == ' ' || seq[i] == '\t' || seq[i] == '\r' || seq[i] == '\v' || seq[i] == '\f' || seq[i] == '\0')
                )
                i++;
            if (i < seq.Length && seq[i] == '\\')
            {
                i++;
                SkipWhitespaceAndOneNewline(ref i, seq);
            }
        }


        public static void SkipWhitespaceAndParen(ref int i, string seq)
        {
            while (i < seq.Length && (
                seq[i] == ' ' || seq[i] == '\t' || seq[i] == '\r' || seq[i] == '\v' || seq[i] == '\f' || seq[i] == '\0' || seq[i] == '(' || seq[i] == '\n')
                )
                i++;
        }

        public static void SkipWhitespaceAndOneNewline(ref int i, string seq)
        {
            while (i < seq.Length && (
                seq[i] == ' ' || seq[i] == '\t' || seq[i] == '\r' || seq[i] == '\v' || seq[i] == '\f' || seq[i] == '\0')
                )
                i++;
            if (seq[i] == '\n')
            {
                i++;
                SkipWhitespace(ref i, seq);
            }
        }

        public static void SkipWhitespaceAndNewlines(ref int i, string seq)
        {
            while (i < seq.Length && (
                seq[i] == ' ' || seq[i] == '\t' || seq[i] == '\r' || seq[i] == '\v' || seq[i] == '\f' || seq[i] == '\n' || seq[i] == '\0')
                )
                i++;
        }
        public static void SkipBlock(ref int i, string seq)
        {
            var depth = 0;
            while (i < seq.Length && (depth > 0 || seq[i] != '}'))
            {
                if (seq[i] == '{')
                    depth++;
                else if (seq[i] == '}')
                {
                    depth--;
                    if (depth < 0)
                        Logger.Log(Channel.Sequencer, LogPriority.Error, $"error at pos {i} why r their more }} than {{ ");
                }
                i++;
            }
        }

        public static void SkipOnError(ref int i, string seq)
        {
            while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                i++;
        }

        public async Task ExecuteAsync(SequencerContext ctx, string seq, Action onComplete = null)
        {

            for (var i = 0; i < seq.Length; i++)
            {
                var c = seq[i];
                var hasNext = i < seq.Length - 1;

                // comments
                if (c == '/' && hasNext && seq[i + 1] == '/')
                {
                    while (i < seq.Length && seq[i] != '\n')
                        i++;

                    continue;
                }

                //ignore whiterspace
                if (char.IsWhiteSpace(c))
                    continue;

                // async functions
                if (Sequencer.Eat(seq, ref i, "+"))
                {
                    var path = Shell.GetNextArg(ref i, seq);
                    var execArgs = new List<string>();

                    while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                    {
                        execArgs.Add(ctx.Eval(Shell.GetNextArg(ref i, seq)));
                    }

                    ScriptRunner.Run($"{ctx.DebugInfo}-[nonblocking]", path, execArgs, null);
                    continue;
                }

                //format
                if (c == '>')
                {
                    i++;
                    var aa = new List<string>();
                    aa.Add("format");
                    aa.Add(Shell.GetNextArg(ref i, seq));
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    aa.Add(Shell.GetNextArg(ref i, seq));
               //     G.ScriptSystem.AddTask(Shell.GetRunCommandAction(aa.ToArray()));
                    var toRun = Shell.GetRunCommandAction(aa.ToArray());
                    if (toRun != null)
                        await toRun.Invoke();
                    continue;
                }

                //wait for signal
                if (c == '-' && hasNext && seq[i + 1] == '-')
                {
                    i++;
                    Sequencer.S.ContinueReceived = false;
                    while (!Sequencer.S.ContinueReceived)
                    {
                        await Script.NextFrame();
                    }
                    continue;
                }

                //wait time periods
                if (c == '-' && hasNext)
                {
                    i++;
                    Sequencer.SkipWhitespace(ref i, seq);
                    if (seq[i] != '.' && !char.IsDigit(seq[i]))
                    {
                        var path = Shell.GetNextArg(ref i, seq);
                        var execArgs = new List<string>();

                        while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                        {
                            execArgs.Add(ctx.Eval(Shell.GetNextArg(ref i, seq)));
                        }
                        ScriptRunner.Run(
                            ctx.DebugInfo,
                            path,
                            execArgs,
                            () =>
                            {
                                Sequencer.S.Thread(ctx, seq.Substring(i), onComplete);
                            });
                        return;
                    }
                    else
                    {
                        Sequencer.S.Sb.Clear();
                        while (i < seq.Length && (seq[i] == '.' || char.IsDigit(seq[i])))
                        {
                            Sequencer.S.Sb.Append(seq[i]);
                            i++;
                        }
                        if (float.TryParse(Sequencer.S.Sb.ToString(), out var parsedwait))
                        {
                            await Task.Delay((int)(parsedwait * 1000));
                            //  yield return new WaitForSeconds(parsedwait);
                        }
                        else
                            Logger.Log(Channel.Sequencer, LogPriority.Warning, $"error at pos {i} parsing as float {Sequencer.S.Sb.ToString()}:{seq.Substring(i)}");

                        continue;
                    }
                }
                /*
                // choice
                if (seq.Length > i + 5 && seq[i] == 'b' && seq[i + 1] == 'r' && seq[i + 2] == 'a' && seq[i + 3] == 'n' && seq[i + 4] == 'c' && seq[i + 5] == 'h')
                {
                    // disgard choice
                    i = i + 6;

                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    var sb = new StringBuilder();
                    // wait for choice def stuff
                    while (seq.Length > i + 8
                        && !(
                        seq[i] == 'e' 
                        && seq[i + 1] == 'n' 
                        && seq[i + 2] == 'd' 
                        && seq[i + 3] == 'b' 
                        && seq[i + 4] == 'r' 
                        && seq[i + 5] == 'a'
                        && seq[i + 6] == 'n'
                        && seq[i + 7] == 'c'
                        && seq[i + 8] == 'h'
                    ))
                    {
                        sb.Append(seq[i]);
                        i++;
                    }
                    i += 8;
                    var branchArgs = new List<string>();
                    branchArgs.Add("branch");
                    branchArgs.Add(sb.ToString());
                    if (branchArgs.Count > 0)
                    {
                        Shell.GetRunCommandAction(branchArgs.ToArray())?.Invoke();
                    }

                    continue;
                }
                */
                if (Sequencer.Eat(seq, ref i, "opt "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    // TODO
                    //     StorySequencer.Choice();
                    //     StorySequencer.SetChoiceCommand(() =>
                    //    {
                    //
                    //      ScriptRunner.Run(
                    //        ctx.DebugInfo,
                    //      name);
                    //  });
                }

                // skip failed conditions
                if (Sequencer.Eat(seq, ref i, "else "))
                {
                    while (i < seq.Length && seq[i] != '{')
                    {
                        i++;
                    }
                    i++; // skip {
                    Sequencer.SkipBlock(ref i, seq);
                    continue;
                }

                // conditionals
                if (Sequencer.Eat(seq, ref i, "if "))
                {
                    Sequencer.SkipWhitespace(ref i, seq);
                    Sequencer.S.Sb.Clear();
                    while (i < seq.Length && seq[i] != '{')
                    {
                        Sequencer.S.Sb.Append(seq[i]);
                        i++;
                    }
                    var isbool = Boolean.TryParse(ctx.Eval(Sequencer.S.Sb.ToString()), out var passed);
                    if (passed)
                    {
                        ctx.AddDepth();
                        continue;
                    }
                    i++;
                    Sequencer.SkipBlock(ref i, seq);
                    i++;
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    Sequencer.Eat(seq, ref i, "else");
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    i--;
                    continue;
                }

                // vars
                if (Sequencer.Eat(seq, ref i, "local "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    ctx.Local(name, ctx.Eval(exp));
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "temp "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    Cvars.Temps[name] = ctx.Eval(exp);
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "set "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    ctx.Set(name, ctx.Eval(exp));
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "unset "))
                {
                    var name = Shell.GetNextArg(ref i, seq);

                    ctx.Set(name, "");
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "global "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    Cvars.Set(name, ctx.Eval(exp));
                    continue;
                }

                // funcs
                if (Sequencer.Eat(seq, ref i, "seq "))
                {
                    Sequencer.SkipWhitespace(ref i, seq);
                    Sequencer.S.Sb.Clear();
                    while (i < seq.Length && seq[i] != '(' && !char.IsWhiteSpace(seq[i]))
                    {
                        Sequencer.S.Sb.Append(seq[i]);
                        i++;
                    }
                    var name = Sequencer.S.Sb.ToString();
                    Sequencer.SkipWhitespaceAndParen(ref i, seq);
                    var p = new List<string>();

                    while (i < seq.Length && seq[i] != ')')
                    {
                        Sequencer.S.Sb.Clear();
                        while (i < seq.Length && seq[i] != ')' && seq[i] != ',' && !char.IsWhiteSpace(seq[i]))
                        {
                            Sequencer.S.Sb.Append(seq[i]);
                            i++;
                        }
                        if (!string.IsNullOrWhiteSpace(Sb.ToString()))
                        {
                            p.Add(Sb.ToString());
                        }
                        if (seq[i] == ',')
                            i++;
                        Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    }
                    i++;
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);

                    if (seq[i] != '{')
                    {
                        ctx.Error($"the hel is the fucntion i see no {{ for {name} i see {seq[i]}");
                        continue;
                    }
                    i++;
                    Sb.Clear();

                    var depth = 0;
                    while (i < seq.Length && (depth > 0 || seq[i] != '}'))
                    {
                        if (seq[i] == '{')
                            depth++;
                        else if (seq[i] == '}')
                        {
                            depth--;
                            if (depth < 0)
                                Logger.Log(Channel.Sequencer, LogPriority.Error, $"error at pos {i} why r their more }} than {{ ");
                        }
                        Sb.Append(seq[i]);
                        i++;
                    }
                    i++;

                    var commands = Sb.ToString();
                    var clone = ctx.Clone();
                    Shell.Routines[name] = new RoutineInfo
                    {
                        Params = p,
                        Exec = (debug, args, onComplete) =>
                        {
                            clone.DebugInfo = $"{debug}->{name}";
                            var execstring = "";
                            var argI = 0;
                            foreach (var arg in p)
                            {
                                execstring = execstring + $"local {arg} '{args[argI]}'\n";
                            }
                            execstring += commands;
                            Sequencer.S.PlayRoutine(clone, execstring, onComplete);
                        },
                    };

                    continue;
                }

                if (c == '{')
                {
                    ctx.AddDepth();
                    continue;
                }
                if (c == '}')
                {
                    ctx.RemoveDepth();
                    continue;
                }


                Sequencer.SkipWhitespace(ref i, seq);
                var args = new List<string>();
                while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                {
                    args.Add(Shell.GetNextArg(ref i, seq));
                }
                if (args.Count > 0)
                {
                    //  G.ScriptSystem.AddTask(Shell.GetRunCommandAction(args.ToArray()));//?.Invoke();
                    var toRun = Shell.GetRunCommandAction(args.ToArray());
                    if (toRun != null)
                        await toRun.Invoke();
                }
            }

            //   onComplete?.Invoke();
        }
    }

    public class SequencerRoutine : AsyncScript
    {
        SequencerContext ctx;
        string seq;
        Action onComplete = null;
        public void Init(SequencerContext Ctx, string Seq, Action OnComplete = null) { ctx = Ctx; seq = Seq; onComplete = OnComplete; }

        StringBuilder Sb => Sequencer.S.Sb;
        public override async Task Execute()
        {

            for (var i = 0; i < seq.Length; i++)
            {
                var c = seq[i];
                var hasNext = i < seq.Length - 1;

                // comments
                if (c == '/' && hasNext && seq[i + 1] == '/')
                {
                    while (i < seq.Length && seq[i] != '\n')
                        i++;

                    continue;
                }

                //ignore whiterspace
                if (char.IsWhiteSpace(c))
                    continue;

                // async functions
                if (Sequencer.Eat(seq, ref i, "+"))
                {
                    var path = Shell.GetNextArg(ref i, seq);
                    var execArgs = new List<string>();

                    while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                    {
                        execArgs.Add(ctx.Eval(Shell.GetNextArg(ref i, seq)));
                    }

                    ScriptRunner.Run($"{ctx.DebugInfo}-[nonblocking]", path, execArgs, null);
                    continue;
                }

                //format
                if (c == '>')
                {
                    i++;
                    var aa = new List<string>();
                    aa.Add("format");
                    aa.Add(Shell.GetNextArg(ref i, seq));
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    aa.Add(Shell.GetNextArg(ref i, seq));
                    //G.ScriptSystem.AddTask(Shell.GetRunCommandAction(aa.ToArray()));
                    var toRun = Shell.GetRunCommandAction(aa.ToArray());
                    if (toRun != null)
                        await toRun.Invoke();

                    continue;
                }

                //wait for signal
                if (c == '-' && hasNext && seq[i + 1] == '-')
                {
                    i++;
                    Sequencer.S.ContinueReceived = false;
                    while (!Sequencer.S.ContinueReceived)
                    {
                        await Script.NextFrame();
                    }
                    continue;
                }

                //wait time periods
                if (c == '-' && hasNext)
                {
                    i++;
                    Sequencer.SkipWhitespace(ref i, seq);
                    if (seq[i] != '.' && !char.IsDigit(seq[i]))
                    {
                        var path = Shell.GetNextArg(ref i, seq);
                        var execArgs = new List<string>();

                        while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                        {
                            execArgs.Add(ctx.Eval(Shell.GetNextArg(ref i, seq)));
                        }
                        ScriptRunner.Run(
                            ctx.DebugInfo,
                            path,
                            execArgs,
                            () =>
                            {
                                Sequencer.S.Thread(ctx, seq.Substring(i), onComplete);
                            });
                        return;
                    }
                    else
                    {
                        Sequencer.S.Sb.Clear();
                        while (i < seq.Length && (seq[i] == '.' || char.IsDigit(seq[i])))
                        {
                            Sequencer.S.Sb.Append(seq[i]);
                            i++;
                        }
                        if (float.TryParse(Sequencer.S.Sb.ToString(), out var parsedwait))
                        {
                            await Task.Delay((int)(parsedwait * 1000));
                            //  yield return new WaitForSeconds(parsedwait);
                        }
                        else
                            Logger.Log(Channel.Sequencer, LogPriority.Warning, $"error at pos {i} parsing as float {Sequencer.S.Sb.ToString()}:{seq.Substring(i)}");

                        continue;
                    }
                }
                /*
                // choice
                if (seq.Length > i + 5 && seq[i] == 'b' && seq[i + 1] == 'r' && seq[i + 2] == 'a' && seq[i + 3] == 'n' && seq[i + 4] == 'c' && seq[i + 5] == 'h')
                {
                    // disgard choice
                    i = i + 6;

                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    var sb = new StringBuilder();
                    // wait for choice def stuff
                    while (seq.Length > i + 8
                        && !(
                        seq[i] == 'e' 
                        && seq[i + 1] == 'n' 
                        && seq[i + 2] == 'd' 
                        && seq[i + 3] == 'b' 
                        && seq[i + 4] == 'r' 
                        && seq[i + 5] == 'a'
                        && seq[i + 6] == 'n'
                        && seq[i + 7] == 'c'
                        && seq[i + 8] == 'h'
                    ))
                    {
                        sb.Append(seq[i]);
                        i++;
                    }
                    i += 8;
                    var branchArgs = new List<string>();
                    branchArgs.Add("branch");
                    branchArgs.Add(sb.ToString());
                    if (branchArgs.Count > 0)
                    {
                        Shell.GetRunCommandAction(branchArgs.ToArray())?.Invoke();
                    }

                    continue;
                }
                */
                if (Sequencer.Eat(seq, ref i, "opt "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    // TODO
               //     StorySequencer.Choice();
               //     StorySequencer.SetChoiceCommand(() =>
                //    {
                //
                  //      ScriptRunner.Run(
                    //        ctx.DebugInfo,
                      //      name);
                  //  });
                }

                // skip failed conditions
                if (Sequencer.Eat(seq, ref i, "else "))
                {
                    while (i < seq.Length && seq[i] != '{')
                    {
                        i++;
                    }
                    i++; // skip {
                    Sequencer.SkipBlock(ref i, seq);
                    continue;
                }

                // conditionals
                if (Sequencer.Eat(seq, ref i, "if "))
                {
                    Sequencer.SkipWhitespace(ref i, seq);
                    Sequencer.S.Sb.Clear();
                    while (i < seq.Length && seq[i] != '{')
                    {
                        Sequencer.S.Sb.Append(seq[i]);
                        i++;
                    }
                    var isbool = Boolean.TryParse(ctx.Eval(Sequencer.S.Sb.ToString()), out var passed);
                    if (passed)
                    {
                        ctx.AddDepth();
                        continue;
                    }
                    i++;
                    Sequencer.SkipBlock(ref i, seq);
                    i++;
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    Sequencer.Eat(seq, ref i, "else");
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    i--;
                    continue;
                }

                // vars
                if (Sequencer.Eat(seq, ref i, "local "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    ctx.Local(name, ctx.Eval(exp));
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "temp "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    Cvars.Temps[name] = ctx.Eval(exp);
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "set "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    ctx.Set(name, ctx.Eval(exp));
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "unset "))
                {
                    var name = Shell.GetNextArg(ref i, seq);

                    ctx.Set(name, "");
                    continue;
                }
                if (Sequencer.Eat(seq, ref i, "global "))
                {
                    var name = Shell.GetNextArg(ref i, seq);
                    var exp = Shell.GetRestOfStatement(ref i, seq);

                    Cvars.Set(name, ctx.Eval(exp));
                    continue;
                }

                // funcs
                if (Sequencer.Eat(seq, ref i, "seq "))
                {
                    Sequencer.SkipWhitespace(ref i, seq);
                    Sequencer.S.Sb.Clear();
                    while (i < seq.Length && seq[i] != '(' && !char.IsWhiteSpace(seq[i]))
                    {
                        Sequencer.S.Sb.Append(seq[i]);
                        i++;
                    }
                    var name = Sequencer.S.Sb.ToString();
                    Sequencer.SkipWhitespaceAndParen(ref i, seq);
                    var p = new List<string>();

                    while (i < seq.Length && seq[i] != ')')
                    {
                        Sequencer.S.Sb.Clear();
                        while (i < seq.Length && seq[i] != ')' && seq[i] != ',' && !char.IsWhiteSpace(seq[i]))
                        {
                            Sequencer.S.Sb.Append(seq[i]);
                            i++;
                        }
                        if (!string.IsNullOrWhiteSpace(Sb.ToString()))
                        {
                            p.Add(Sb.ToString());
                        }
                        if (seq[i] == ',')
                            i++;
                        Sequencer.SkipWhitespaceAndNewlines(ref i, seq);
                    }
                    i++;
                    Sequencer.SkipWhitespaceAndNewlines(ref i, seq);

                    if (seq[i] != '{')
                    {
                        ctx.Error($"the hel is the fucntion i see no {{ for {name} i see {seq[i]}");
                        continue;
                    }
                    i++;
                    Sb.Clear();

                    var depth = 0;
                    while (i < seq.Length && (depth > 0 || seq[i] != '}'))
                    {
                        if (seq[i] == '{')
                            depth++;
                        else if (seq[i] == '}')
                        {
                            depth--;
                            if (depth < 0)
                                Logger.Log(Channel.Sequencer, LogPriority.Error, $"error at pos {i} why r their more }} than {{ ");
                        }
                        Sb.Append(seq[i]);
                        i++;
                    }
                    i++;

                    var commands = Sb.ToString();
                    var clone = ctx.Clone();
                    Shell.Routines[name] = new RoutineInfo
                    {
                        Params = p,
                        Exec = (debug, args, onComplete) =>
                        {
                            clone.DebugInfo = $"{debug}->{name}";
                            var execstring = "";
                            var argI = 0;
                            foreach (var arg in p)
                            {
                                execstring = execstring + $"local {arg} '{args[argI]}'\n";
                            }
                            execstring += commands;
                            Sequencer.S.PlayRoutine(clone, execstring, onComplete);
                        },
                    };

                    continue;
                }

                if (c == '{')
                {
                    ctx.AddDepth();
                    continue;
                }
                if (c == '}')
                {
                    ctx.RemoveDepth();
                    continue;
                }


                Sequencer.SkipWhitespace(ref i, seq);
                var args = new List<string>();
                while (i < seq.Length && seq[i] != ';' && seq[i] != '\n')
                {
                    args.Add(Shell.GetNextArg(ref i, seq));
                }
                if (args.Count > 0)
                {
                 //   G.ScriptSystem.AddTask(Shell.GetRunCommandAction(args.ToArray()));//?.Invoke();
                    var toRun = Shell.GetRunCommandAction(args.ToArray());
                    if (toRun != null)
                        await toRun.Invoke();
                }
            }

         //   onComplete?.Invoke();
        }
    }

    public enum TweenType
    {
        floatTween,
        colorTween,
    }
    public class TweenRoutine : AsyncScript
    {
        public TweenType tweenType;
        public float initF;
        public float endF;
        public Color initC;
        public Color endC;
        public float duration;
        public Action<Color> setC;
        public Action<float> setF;
        public Action onEnd = null;

        public override async Task Execute()
        {
            switch (tweenType)
            {
                case TweenType.colorTween:
                    {
                        var endTime = Time.time + duration;
                        var startTime = Time.time;
                        while (Time.time < endTime)
                        {
                            var progress = (Time.time - startTime) / duration;
                            setC?.Invoke(Color.Lerp(initC, endC, progress));
                            await Script.NextFrame();
                        }

                        setC?.Invoke(endC);
                        await Script.NextFrame();
                        onEnd?.Invoke();
                    }
                    break;

                case TweenType.floatTween:
                    {
                        var endTime = Time.time + duration;
                        var startTime = Time.time;
                        while (Time.time < endTime)
                        {
                            var progress = (Time.time - startTime) / duration;
                            setF?.Invoke(MathUtil.Lerp(initF, endF, progress));
                            await Script.NextFrame();
                        }

                        setF?.Invoke(endF);
                        await Script.NextFrame();
                        onEnd?.Invoke();
                    }
                    break;

            }
        }
    }
}