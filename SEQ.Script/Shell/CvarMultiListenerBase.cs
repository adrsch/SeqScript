// MIT License 

using System.Collections;
using System.Collections.Generic;
using Stride.Engine;
using System;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public struct CvarMultiListenerInfo
    {
        public string Cvar;
        public Action OnValueChanged;
    }
    public abstract class CvarMultiListenerNoMB : ICvarListener
    {
        protected abstract List<CvarMultiListenerInfo> GetCvars();

        public void DoStart()
        {
            foreach (var inf in GetCvars())
            {
                if (!string.IsNullOrWhiteSpace(inf.Cvar))
                {
                    if (Cvars.Listeners.Entries.TryGetValue(inf.Cvar, out var l))
                    {
                        l.Add(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = inf.OnValueChanged,
                        });
                    }
                    else
                    {
                        Cvars.Listeners.Entries[inf.Cvar] = new List<CvarListenerInfo>();
                        Cvars.Listeners.Entries[inf.Cvar].Add(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = inf.OnValueChanged,
                        });
                    }

                    inf.OnValueChanged();
                }
            }
        }

        public void DoDestroy()
        {
            foreach (var inf in GetCvars())
            {
                if (Cvars.Listeners.Entries.TryGetValue(inf.Cvar, out var l))
                {
                    l.RemoveAll(x => x.Listener == this);
                }
            }
        }
    }
    public abstract class CvarMultiListenerBase : SyncScript, ICvarListener
    {
        protected abstract List<CvarMultiListenerInfo> GetCvars();

        public override void Start()
        {
            foreach (var inf in GetCvars())
            {
                if (!string.IsNullOrWhiteSpace(inf.Cvar))
                {
                    if (Cvars.Listeners.Entries.TryGetValue(inf.Cvar, out var l))
                    {
                        l.Add(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = inf.OnValueChanged,
                        });
                    }
                    else
                    {
                        Cvars.Listeners.Entries[inf.Cvar] = new List<CvarListenerInfo>();
                        Cvars.Listeners.Entries[inf.Cvar].Add(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = inf.OnValueChanged,
                        });
                    }

                    inf.OnValueChanged();
                }
            }
        }

        public override void Cancel()
        {
            foreach (var inf in GetCvars())
            {
                if (Cvars.Listeners.Entries.TryGetValue(inf.Cvar, out var l))
                {
                    l.RemoveAll(x => x.Listener == this);
                }
            }
        }
    }

    public abstract class AsyncCvarMultiListenerBase : AsyncScript, ICvarListener
    {
        protected abstract List<CvarMultiListenerInfo> GetCvars();

        public override async Task Execute()
        {
            await BeforeInit();
            foreach (var inf in GetCvars())
            {
                if (!string.IsNullOrWhiteSpace(inf.Cvar))
                {
                    if (Cvars.Listeners.Entries.TryGetValue(inf.Cvar, out var l))
                    {
                        l.Add(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = inf.OnValueChanged,
                        });
                    }
                    else
                    {
                        Cvars.Listeners.Entries[inf.Cvar] = new List<CvarListenerInfo>();
                        Cvars.Listeners.Entries[inf.Cvar].Add(new CvarListenerInfo
                        {
                            Listener = this,
                            OnValueChanged = inf.OnValueChanged,
                        });
                    }

                    inf.OnValueChanged();
                }
            }
            await AfterInit();
        }

        public abstract Task BeforeInit();

        public abstract Task AfterInit();

        public override void Cancel()
        {
            foreach (var inf in GetCvars())
            {
                if (Cvars.Listeners.Entries.TryGetValue(inf.Cvar, out var l))
                {
                    l.RemoveAll(x => x.Listener == this);
                }
            }
        }
    }
}