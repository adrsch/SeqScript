// MIT License 

using Stride.Engine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public abstract class CvarListenerBase : SyncScript, ICvarListener
    {
        protected abstract string GetCvar();

        public abstract void OnValueChanged();

        public virtual void BeforeStart()
        {

        }

        public override void Start()
        {
            BeforeStart();
            var cvar = GetCvar();
            if (!string.IsNullOrWhiteSpace(cvar))
            {
                if (Cvars.Listeners.Entries.TryGetValue(cvar, out var l))
                {
                    l.Add(new CvarListenerInfo
                    {
                        Listener = this,
                        OnValueChanged = () => OnValueChanged(),
                    });
                }
                else
                {
                    Cvars.Listeners.Entries[cvar] = new List<CvarListenerInfo>();
                    Cvars.Listeners.Entries[cvar].Add(new CvarListenerInfo
                    {
                        Listener = this,
                        OnValueChanged = () => OnValueChanged(),
                    });
                }

                OnValueChanged();
            }
            OnStart();
        }

        public abstract void OnStart();

        public override void Cancel()
        {
            if (Cvars.Listeners.Entries.TryGetValue(GetCvar(), out var l))
            {
                l.RemoveAll(x => x.Listener == this);
            }
        }
    }
    public abstract class CvarListenerStartup : StartupScript, ICvarListener
    {
        protected abstract string GetCvar();

        public abstract void OnValueChanged();

        public virtual void BeforeStart()
        {

        }

        public override void Start()
        {
            BeforeStart();
            var cvar = GetCvar();
            if (!string.IsNullOrWhiteSpace(cvar))
            {
                if (Cvars.Listeners.Entries.TryGetValue(cvar, out var l))
                {
                    l.Add(new CvarListenerInfo
                    {
                        Listener = this,
                        OnValueChanged = () => OnValueChanged(),
                    });
                }
                else
                {
                    Cvars.Listeners.Entries[cvar] = new List<CvarListenerInfo>();
                    Cvars.Listeners.Entries[cvar].Add(new CvarListenerInfo
                    {
                        Listener = this,
                        OnValueChanged = () => OnValueChanged(),
                    });
                }

                OnValueChanged();
            }
            OnStart();
        }

        public abstract void OnStart();

        public override void Cancel()
        {
            if (Cvars.Listeners.Entries.TryGetValue(GetCvar(), out var l))
            {
                l.RemoveAll(x => x.Listener == this);
            }
        }
    }

    public abstract class AysncCvarListenerBase : AsyncScript, ICvarListener
    {
        protected abstract string GetCvar();

        public abstract void OnValueChanged();


        public override async Task Execute()
        {
            await BeforeInit();
            var cvar = GetCvar();
            if (!string.IsNullOrWhiteSpace(cvar))
            {
                if (Cvars.Listeners.Entries.TryGetValue(cvar, out var l))
                {
                    l.Add(new CvarListenerInfo
                    {
                        Listener = this,
                        OnValueChanged = () => OnValueChanged(),
                    });
                }
                else
                {
                    Cvars.Listeners.Entries[cvar] = new List<CvarListenerInfo>();
                    Cvars.Listeners.Entries[cvar].Add(new CvarListenerInfo
                    {
                        Listener = this,
                        OnValueChanged = () => OnValueChanged(),
                    });
                }

                OnValueChanged();
            }
            await AfterInit();
        }

        public abstract Task BeforeInit();

        public abstract Task AfterInit();


        public override void Cancel()
        {
            if (Cvars.Listeners.Entries.TryGetValue(GetCvar(), out var l))
            {
                l.RemoveAll(x => x.Listener == this);
            }
        }
    }
}