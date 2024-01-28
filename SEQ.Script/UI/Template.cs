using Stride.Core;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using Stride.UI;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public class EndTemplateEvent
    {
    }
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<ImageTemplateInfo>))]
    public class ImageTemplateInfo
    {
        public string Name;
        public ImageElement El;
        public SpriteFromTexture SpriteFromTex = new SpriteFromTexture();
    }

    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<Template>))]
    public class Template : IGameStateController
    {
        public string Name;
        public List<TextDisplay> Fields = new List<TextDisplay>();
        public List<string> ContinueButtons = new();
        public List<ImageTemplateInfo> Images = new();
        UIComponent UI;
        UIElement Root;
        public void Init(UIComponent ui)
        {
            UI = ui;

            Root = ui.Page.RootElement.FindName(Name);
            Root.Visibility = Visibility.Collapsed;
            foreach (var field in Fields)
            {
                field.Init(Root);
            }
            foreach (var b in ContinueButtons)
            {
                var el = Root.FindName(b);
                el.TouchUp += (s, e) => OnContinuePressed();
            }
            foreach (var img in Images)
            {
                img.El = Root.FindVisualChildOfType<ImageElement>(img.Name);
                img.El.Source = img.SpriteFromTex;
                img.El.Visibility = Visibility.Collapsed;
            }
        }

        public void SetImage(string name, Texture tex)
        {
            foreach (var img in Images)
            {
                if (img.Name == name)
                {
                    if (tex == null)
                    {
                        img.El.Visibility -= Visibility.Collapsed;
                    }
                    else
                    {
                        img.SpriteFromTex.Texture = tex;
                        img.El.Source = img.SpriteFromTex;
                        img.El.Visibility = Visibility.Visible;
                    }
                    return;
                }
            }
        }

        public void Update(float dt)
        {
            if (!this.IsActive()) return;
            foreach (var field in Fields)
            {
                field.Update(dt);
            }

            if (Keybinds.GetKeyDown(Keybind.Jump) || Keybinds.GetKeyDown(Keybind.Interact))
            {
                OnContinuePressed();
            }
        }

        public void Format(string r, string text)
        {
            foreach (var field in Fields)
            {
                if (field.Ref == r)
                {
                    field.text = text;
                }
            }
        }

        void OnContinuePressed()
        {
            bool shouldGoToNext = true;
            foreach (var field in Fields)
            {
                if (field.UseConsole && !field.ConsoleTweener.AtEnd)
                {
                    shouldGoToNext = false;
                    field.ConsoleTweener.Skip();
                }
            }
            if (shouldGoToNext)
                Sequencer.Continue();
        }
        
        public void Start()
        {
            if (Current == this)
                return;

            foreach (var f in Fields)
                f.text = string.Empty;
            if (Current != null)
            {
                End();
            }
            Current = this;
            GameStateManager.Push(this);
        }

        public static Template Current;

        public static void End()
        {
            if (Current != null)
            {
                GameStateManager.Remove(Current);
                Current = null;
            }
            EventManager.Raise(new EndTemplateEvent());
        }

        public InteractionState GetInteractionState()
        {
            return InteractionState.GUI;
        }

        public PointerState GetPointerState()
        {
            return PointerState.GUI;
        }

        public void OnGainControl()
        {

            Root.Visibility = Visibility.Visible;
        }

        public void OnLoseControl()
        {
            Root.Visibility = Visibility.Collapsed;
        }
    }
}
