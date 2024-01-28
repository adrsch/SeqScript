using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;
using Stride.Engine.Design;
using Stride.Games;
using Stride.Graphics;

namespace SEQ.Script
{
    public abstract class G : Game
    {
        public static G S;

        public bool DebugMode;
        public static ScriptSystem ScriptSystem => S.Script;
        //  public SeqSettings SeqSettings;
        public GameStateManager StateManager = new();
        public void Init()
        {
            S = this;
           // SeqSettings = LoadSettings<SeqSettings>(SeqSettings.AssetUrl);
            Manager.Init();
            OnInit();

            LoadModules();
            foreach (var m in Modules)
                m.Init();
        }

        protected override void Update(GameTime gameTime)
        {
            //     if (IMGUI == null)
            //     IMGUI = new ImGuiSystem(Services, GraphicsDeviceManager);

            base.Update(gameTime);
        }

        List<ISeqModule> Modules = new List<ISeqModule>();

        void LoadModules()
        {
            var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ISeqModule).IsAssignableFrom(p) && p.IsClass);

            foreach (var moduleType in moduleTypes)
            {
                var inst = (ISeqModule)Activator.CreateInstance(moduleType);
                Modules.Add(inst);
            }
            Modules.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        public T GetModule<T>() where T : ISeqModule
        {
            foreach (var m in Modules)
                if (m is T)
                    return (T)m;
            return default;
        }

        public abstract void OnInit();

        public void DoExit()
        {
            foreach (var m in Modules)
                m.Exit();

            Exit();
        }

        public int CurrentOutput;
        public void SetCurrentOutput(int x)
        {
            CurrentOutput = x;
            if (CurrentOutput < 0) CurrentOutput = 0;
            if (CurrentOutput >= GraphicsDevice.Adapter.Outputs.Length)
                CurrentOutput = GraphicsDevice.Adapter.Outputs.Length;
        }

        public bool IsFullscreen;

        public void SetVsync(bool vsync)
        {
            GraphicsDevice.Presenter.PresentInterval = vsync ? Stride.Graphics.PresentInterval.One : Stride.Graphics.PresentInterval.Immediate;
        }

        bool hasSetSizeHandler;
        public void SetFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            var w = GraphicsDevice.Adapter.Outputs[CurrentOutput].CurrentDisplayMode.Width;
            var h = GraphicsDevice.Adapter.Outputs[CurrentOutput].CurrentDisplayMode.Height;
            Window.PreferredFullscreenSize = new Int2(w, h);
            Window.AllowUserResizing = true;
            Window.FullscreenIsBorderlessWindow = true;
            Window.Position = new Int2(0, 0);
            Window.SetSize(new Int2(w, h));
            Window.IsBorderLess = fullscreen;
            if (!hasSetSizeHandler)
            {
                Window.ClientSizeChanged += OnClientSizeChanged;
                hasSetSizeHandler = true;
            }
        }

        void OnClientSizeChanged(object sender, EventArgs e)
        {
            //var w = Window.ClientBounds.Width;
            //var h = Window.ClientBounds.Height;
            SeqCam.UpdateFOV();
        }

        protected override Task LoadContent()
        {
            if (IsFullscreen)
            {
                var w = GraphicsDevice.Adapter.Outputs[CurrentOutput].CurrentDisplayMode.Width;
                var h = GraphicsDevice.Adapter.Outputs[CurrentOutput].CurrentDisplayMode.Height;
                Window.PreferredFullscreenSize = new Int2(w, h);
                Window.AllowUserResizing = true;
                Window.FullscreenIsBorderlessWindow = true;
                Window.Position = new Int2(0, 0);
                Window.SetSize(new Int2(w, h));
                Window.IsBorderLess = true;
            }
            else
            {

                Window.IsBorderLess = false;
            }
            //MinimizedMinimumUpdateRate.MinimumElapsedTime = new TimeSpan(0);
            return base.LoadContent();
        }

        public bool RawInput
        {
            get => Input.UseRawInput;
            set => Input.UseRawInput = value;
        }


        public T LoadSettings<T>(string url) where T: class, new()
        {
            if (Content.Exists(GameSettings.AssetUrl))
            {
                var settings = Content.Load<T>(url);
                if (settings != null)
                    return settings;
            }

            Logger.Log(Channel.Data, LogPriority.Warning, $"Could not get settings file {url}, using default");
            return new T();
        }
    }
}
