using Newtonsoft.Json.Linq;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<TextDisplay>))]
    [CategoryOrder(10, "Refs", Expand = ExpandRule.Always)]
    [CategoryOrder(20, "Info", Expand = ExpandRule.Always)]
    [CategoryOrder(30, "Shadow", Expand = ExpandRule.Auto)]
    [CategoryOrder(40, "Animator", Expand = ExpandRule.Auto)]
    public class TextDisplay
    {
        public enum TextBaseType
        {
            TextBlock,
            ScrollingText,
        }

        [Display(category: "Refs", order: -10)]
        public string Ref;
        [Display(category: "Refs", order: -5)]
        public TextBaseType TextType;

        [Display(category: "Info", order: -11)]
        public string Prefix = "";
        [Display(category: "Info", order: -10)]
        public string Locstring;
        [Display(category: "Info", order: -4)]
        public string Suffix = "";
        [Display(category: "Info", order: 0)]
        public bool PrefixSuffixOnEmpty;

        [Display(category: "Shadow", order: -10)]
        public bool UseShadow;
        int ShadowDistance = 2;
        public Color ShadowColor = Color.Black;

        [Display(category: "Animator", order: -10)]
        public bool UseConsole;
        [Display(category: "Animator", order: -5)]
        public ConsoleTweener ConsoleTweener = new ConsoleTweener();


        UIElement Parent;
        TextBlock Block;
        TextBlock BlockShadow;
        TextBlock BlockShadow2;
#if false
        TextBlock BlockShadow3;
        TextBlock BlockShadow4;
#endif
        ScrollingText Scrolling;
       // ScrollingText ScrollingShadow;
        public void Init(UIElement parent)
        {
            switch (TextType)
            {
                case TextBaseType.TextBlock:
                    Block = parent.FindVisualChildOfType<TextBlock>(Ref);
                    if (UseShadow)
                    {
                        BlockShadow = UICloner.Clone(Block) as TextBlock;
#if false
                        BlockShadow2 = UICloner.Clone(Block) as TextBlock;
                        BlockShadow3 = UICloner.Clone(Block) as TextBlock;
                        BlockShadow4 = UICloner.Clone(Block) as TextBlock;
#endif
                        if (Block.Parent is Panel p)
                        {
#if false
                            p.Children.Add(BlockShadow3);
                            p.Children.Add(BlockShadow4);
                            p.Children.Add(BlockShadow2);
#endif
                            p.Children.Add(BlockShadow);
                        }
                        BlockShadow.Margin = Block.Margin;
#if false
                        BlockShadow4.Margin = new Thickness(
                            Block.Margin.Left - ShadowDistance,
                            Block.Margin.Top + ShadowDistance,
                            Block.Margin.Right + ShadowDistance,
                            Block.Margin.Bottom - ShadowDistance);
                        BlockShadow3.Margin = new Thickness(
                            Block.Margin.Left + ShadowDistance,
                            Block.Margin.Top - ShadowDistance,
                            Block.Margin.Right - ShadowDistance,
                            Block.Margin.Bottom + ShadowDistance);
                        BlockShadow2.Margin = new Thickness(
                            Block.Margin.Left - ShadowDistance,
                            Block.Margin.Top - ShadowDistance,
                            Block.Margin.Right + ShadowDistance,
                            Block.Margin.Bottom + ShadowDistance);
                        BlockShadow3.TextColor = ShadowColor;
                        BlockShadow4.TextColor = ShadowColor;
                        BlockShadow2.TextColor = ShadowColor;
#endif
                        Block.Margin = new Thickness(
                            Block.Margin.Left + ShadowDistance,
                            Block.Margin.Top + ShadowDistance,
                            Block.Margin.Right - ShadowDistance,
                            Block.Margin.Bottom - ShadowDistance);
                        BlockShadow.TextColor = Block.TextColor;
                        Block.TextColor = ShadowColor;
                    }
                    break;
                case TextBaseType.ScrollingText:
                    Scrolling = parent.FindVisualChildOfType<ScrollingText>(Ref);
                    break;
            }

            if (UseConsole)
            {
                ConsoleTweener.Bind(this);
            }

            if (!string.IsNullOrWhiteSpace(Locstring))
            {
                text = Loc.Get(Locstring);
            }
        }

        public void Skip()
        {
            if (UseConsole)
            {
                ConsoleTweener.Skip();
            }
        }

        public void Clear()
        {
            if (UseConsole)
            {
                ConsoleTweener.Clear();
            }
            else
            {
                text = "";
            }
        }

        public void Update(float dt)
        {
            if (UseConsole)
            {
                ConsoleTweener.OnUpdate(dt);
            }
        }

        void SyncToInternal()
        {
            var value = !string.IsNullOrWhiteSpace(textInternal) || PrefixSuffixOnEmpty ? $"{Prefix}{textInternal}{Suffix}" : "";
            if (Block != null)
                Block.Text = value;
            else if (Scrolling != null)
                Scrolling.Text = value;

            if (UseShadow)
            {
#if false
                BlockShadow3.Text = value;
                BlockShadow4.Text = value;
                BlockShadow2.Text = value;
#endif
                BlockShadow.Text = value;
            }
        }

        public void SetTextFromAnimator(string text)
        {
            textInternal = text;
            SyncToInternal();
        }

        string textInternal;
        [DataMemberIgnore]
        public string text
        {
            get
            {
                return textInternal;
            }
            set
            {
                if (UseConsole)
                {
                    ConsoleTweener.To(value);
                }
                else
                {
                    textInternal = value;
                    SyncToInternal();
                }
            }
        }
        [DataMemberIgnore]
        public string Text { get => text; set => text = value; }

        [DataMemberIgnore]
        public Color color
        {
            get { return BlockShadow?.TextColor ?? Block?.TextColor ?? Scrolling?.TextColor ?? Color.White; }
            set
            {
                if (BlockShadow != null)
                    BlockShadow.TextColor = value;
                else if (Block != null)
                    Block.TextColor = value;
                else if (Scrolling != null)
                    Scrolling.TextColor = value;
            }
        }

        [DataMemberIgnore]
        public Color Color { get => color ; set => color = value; }
    }
}
