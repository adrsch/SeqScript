using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Core;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    [DataContract]
    [ContentSerializer(typeof(ConsoleTweener))]
    public class ConsoleTweener 
    {
        public bool Scramble;
        public bool UseFlash = true;

        public float Speed = 15f;

        public bool FlashOnComplete;
        public float FlashInterval = 0.5f;

        [DataMemberIgnore]
        public string TargetString;
        float Position;
        [DataMemberIgnore]
        int Index => MathUtil.RoundToInt(Position);

        public string placeChar = "█";

        string LastTarget;

        public TextDisplay Bound;
        public void Bind(TextDisplay display)
        {
            Bound = display;
            TargetString = "";
        }



        public void To(string to)
        {
            IsDoingTweenOnClear = false;
            if (TargetString != to)
            {
                LastTarget = TargetString;
                if (string.IsNullOrEmpty(to))
                {
                    if (TweenOnClear)
                    {
                        IsDoingTweenOnClear = true;
                        Position = MathF.Min(Position, TargetString.Length - 1);
                    }
                    else
                    {
                        DoSetText("");
                        TargetString = "";
                        Position = 0;
                    }
                }
                else
                {
                    TargetString = to;
                    Position = 0;
                }
            }
        }

        public void Clear()
        {
            AtEnd = false;
            IsDoingTweenOnClear = false;
            LastTarget = TargetString;
            DoSetText("");
            TargetString = "";
            Position = 0;
        }
        public bool TweenOnClear;
        public float BackspaceSpeedMultiplier = 1f;
        bool IsDoingTweenOnClear;

        public void Skip()
        {
            Position = TargetString.Length;
        }

        float LastToggleTime;
        bool usingEnd = true;
        [DataMemberIgnore]
        public bool IsComplete => Position > TargetString.Length;
        public UISoundClip BeepSFX;
        public void OnUpdate(float dt)
        {
            if (IsDoingTweenOnClear)
            {
                Position -= Speed * BackspaceSpeedMultiplier * dt;
                if (Position < 0) Position = 0;
                if (Position == 0)
                    DoSetText("");
                else if (Position <= TargetString.Length)
                    DoSetText(UseFlash ? TargetString.Substring(0, Index) + placeChar : TargetString.Substring(0, Index));
            }
            else
            {
                var lastIndex = Index;
                Position += Speed * dt;
                if (Position <= TargetString.Length)
                {
                    AtEnd = false;
                    if (Index > lastIndex && BeepSFX != UISoundClip.None)
                        UIAudio.Play(BeepSFX);
                    if (Scramble)
                        DoScramble();
                    else
                        DoSetText(UseFlash ? TargetString.Substring(0, Index) + placeChar : TargetString.Substring(0, Index));
                    usingEnd = true;
                }
                else if (UseFlash && FlashOnComplete)
                {
                    AtEnd = true;
                    if (Time.unscaledTime > LastToggleTime + FlashInterval)
                    {
                        LastToggleTime = Time.unscaledTime;
                        DoSetText(usingEnd ? TargetString : TargetString + placeChar);
                        usingEnd = !usingEnd;
                    }
                }
                else
                {
                    AtEnd = true;
                    DoSetText(TargetString);
                }
            }
        }

        public bool AtEnd;

        void DoSetText(string txt)
        {
            Bound.SetTextFromAnimator(txt);
        }

        StringBuilder Sb = new StringBuilder();

        public bool ScrambleFromTarget = false;

        void DoScramble()
        {
            Sb.Clear();
            Sb.Append(TargetString.Substring(0, Index));
            if (ScrambleFromTarget)
            {
                var min = TargetString.Length;
                for (var i = Index; i < min; i++)
                {
                    if (LastTarget.Length > i && TargetString.Length > i && TargetString[i] == LastTarget[i])
                        Sb.Append(LastTarget[i]);
                    else
                        Sb.Append(RandomUppercase());
                }
            }
            else
            {
                var min = Math.Min(TargetString.Length, LastTarget.Length);
                for (var i = Index; i < min; i++)
                {
                    if (LastTarget.Length > i && TargetString.Length > i && TargetString[i] == LastTarget[i])
                        Sb.Append(LastTarget[i]);
                    else
                        Sb.Append(RandomUppercase());
                }
            }
            DoSetText(Sb.ToString());
        }


        char RandomUppercase()
        {
            return (char)Random.Shared.Next(65, 91);
        }

    }
}