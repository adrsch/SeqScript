using Stride.Animations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using Stride.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Stride.Core.Mathematics.Color;
using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public enum TextFormatting
    {
        None,
        Upper,
        Lower,
        Snake,
        Kebab,
        Sentence,
        Title,
    }
    public static class Extensions
    {
        public static void Hide(this UIPage page) => page.RootElement.Visibility = Visibility.Collapsed;
        public static void Show(this UIPage page) => page.RootElement.Visibility = Visibility.Visible;

        public static PlayingAnimation PlayIfExistsAndNotPlaying(this AnimationComponent animationComponent, string clip)
        {
            if (animationComponent.Animations.ContainsKey(clip) && !animationComponent.IsPlaying(clip))
                return animationComponent.Play(clip);
            return null;
        }

        public static PlayingAnimation PlaySilent(this AnimationComponent animationComponent, string name)
        {
            if (animationComponent.Animations.ContainsKey(name) && !animationComponent.IsPlaying(name))
            {
              //  animationComponent.PlayingAnimations.Clear();
                var playingAnimation = new PlayingAnimation(name, animationComponent.Animations[name]) { CurrentTime = TimeSpan.Zero, Weight = 0f };
                animationComponent.PlayingAnimations.Add(playingAnimation);
                return playingAnimation;
            }
            return null;
        }

        public static PlayingAnimation PlayIfExists(this AnimationComponent animationComponent, string clip)
        {
            if (animationComponent.Animations.ContainsKey(clip))
                return animationComponent.Play(clip);
            return null;
        }
        public static PlayingAnimation BlendIfExists(this AnimationComponent animationComponent, string clip, float target, TimeSpan ts)
        {
            if (animationComponent.Animations.ContainsKey(clip))
                return animationComponent.Blend(clip, target, ts);
            return null;
        }

        public static PlayingAnimation CrossfadeIfExists(this AnimationComponent animationComponent, string clip, float target, TimeSpan ts)
        {
            if (animationComponent.Animations.ContainsKey(clip))
                return animationComponent.Crossfade(clip, ts);
            return null;
        }

        public static float CriticalDamp(this float from, float to, ref float vel, float smooth, float dt)
        {
            return MathUtil.CriticalDamp(from, to, ref vel, smooth, dt);
        }


        public static bool IsSphere(this RigidbodyComponent rb)
        {
            var isSph = true;
            foreach (var c in rb.ColliderShapes)
            {
                if (!(c is SphereColliderShapeDesc))
                    isSph = false;
            }
            return isSph;
        }

        public static bool IsCylinder(this RigidbodyComponent rb, out Vector3 axis)
        {
            axis = default;
            foreach (var c in rb.ColliderShapes)
            {
                if (c is CylinderColliderShapeDesc cshape)
                {
                    switch (cshape.Orientation)
                    {
                        case ShapeOrientation.UpX:
                            axis = rb.Entity.Transform.Right;
                            return true;

                        case ShapeOrientation.UpY:
                            axis = rb.Entity.Transform.Up;
                            return true;

                        case ShapeOrientation.UpZ:
                            axis = rb.Entity.Transform.Forward;
                            return true;
                    }
                }
            }
            return false;
        }

        public static Color ToColor(this int HexVal)
        {
            byte R = (byte)((HexVal >> 16) & 0xFF);
            byte G = (byte)((HexVal >> 8) & 0xFF);
            byte B = (byte)((HexVal) & 0xFF);
            // return new Color(R, G, B, 255);
            return new Color(R, G, B);
        }

        /*
        public static List<T> GetInterfacesInChildren<T>(this Entity go)
        {
            var list = new List<T>();
            foreach (var comp in go.GetChildren(tr)
            {
                if (comp is T asT)
                    list.Add(asT);
            }
            return list;
        }
        public static T GetInterface<T>(this Entity go)
        {
            foreach (var comp in go.GetAll<ScriptComponent>())
            {
                if (comp is T asT)
                    return asT;
            }
            return default(T);
        }

        public static T GetInterfaceInParent<T>(this GameObject go)
        {
            foreach (var comp in go.GetComponentsInParent<SyncScript>())
            {
                if (comp is T asT)
                    return asT;
            }
            return default(T);
        }

        public static List<T> GetInterfacesInParent<T>(this GameObject go)
        {
            var list = new List<T>();
            foreach (var comp in go.GetComponentsInParent<SyncScript>())
            {
                if (comp is T asT)
                    list.Add(asT);
            }
            return list;
        }
        public static T GetInterfaceInChildren<T>(this GameObject go)
        {
            foreach (var comp in go.GetComponentsInChildren<SyncScript>())
            {
                if (comp is T asT)
                    return asT;
            }
            return default(T);
        }
        public static void SetRaycastTarget(this TransformComponent t, bool value)
        {
            foreach (var g in t.GetComponentsInChildren<Graphic>())
                g.raycastTarget = value;
            foreach (var text in t.GetComponentsInChildren<Text>())
                text.raycastTarget = value;
        }
        
        */

        public static string GetNameForMouseButtn(this MouseButton key)
        {
            switch (key)
            {
                case MouseButton.Left:
                    return "left_mouse";
                case MouseButton.Right:
                    return "right_mouse";
                case MouseButton.Middle:
                    return "middle_mouse";
                default:
                    return key.ToString().ToSnakeCase();
            }
        }
        public static string GetNameForKey(this Keys key)
        {
            switch (key)
            {
                case Keys.D0:
                    return "0";
                case Keys.D1:
                    return "1";
                case Keys.D2:
                    return "2";
                case Keys.D3:
                    return "3";
                case Keys.D4:
                    return "4";
                case Keys.D5:
                    return "5";
                case Keys.D6:
                    return "6";
                case Keys.D7:
                    return "7";
                case Keys.D8:
                    return "8";
                case Keys.D9:
                    return "9";
                default:
                    return key.ToString().ToSnakeCase();
            }
        }

        public static string ApplyFormatting(this string text, TextFormatting formatting) =>
            formatting == TextFormatting.Sentence ? text.ToSentenceCase()
            : formatting == TextFormatting.Title ? text.ToTileCase()
            : formatting == TextFormatting.Lower ? text.ToLowerInvariant()
            : formatting == TextFormatting.Upper ? text.ToUpperInvariant()
            : formatting == TextFormatting.Kebab ? text.ToKebabCase()
            : formatting == TextFormatting.Snake ? text.ToSnakeCase()
            : text;

        // https://stackoverflow.com/questions/63055621/how-to-convert-camel-case-to-snake-case-with-two-capitals-next-to-each-other
        public static string ToSnakeCase(this string str)
        {
            var pattern =
                new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");

            return str == null
                ? null
                : string
                    .Join("_", pattern.Matches(str).Cast<Match>().Select(m => m.Value))
                    .ToLower();
        }

        public static string ToKebabCase(this string str)
        {
            var pattern =
                new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");

            return str == null
                ? null
                : string
                    .Join("-", pattern.Matches(str).Cast<Match>().Select(m => m.Value))
                    .ToLower();
        }

        //https://stackoverflow.com/questions/3141426/net-method-to-convert-a-string-to-sentence-case
        public static string ToSentenceCase(this string str)
        {
            // start by converting entire string to lower case
            var lowerCase = str.ToLower();
            // matches the first sentence of a string, as well as subsequent sentences
            var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            // MatchEvaluator delegate defines replacement of setence starts to uppercase
            return r.Replace(lowerCase, s => s.Value.ToUpper());
        }

        public static SurfaceType SurfaceType(this HitResult res) => res.Collider?.SurfaceType ?? Stride.Engine.SurfaceType.Default;
        public static string ToTileCase(this string str)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(str);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ParsesAsNil(this string s) =>
            string.IsNullOrWhiteSpace(s) || s == "nil" || s == "null" || s == "none";

        public static string ReplaceNewlines(this string str, string replace)
        {
            return Regex.Replace(str, @"\r\n?|\n", replace);
        }
        /// <summary>
        /// Returns the value of the highest of the x, y, and z components.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float MaxComponent(this Vector3 x) => Utils.Max(x.X, x.Y, x.Z);
        /// <summary>
        /// Returns the value of the highest of the x, y, and z components with a vector of this component.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float MaxComponent(this Vector3 x, out Vector3 component)
        {
            if (x.X > x.Y && x.X > x.Z)
            {
                component = Vector3.UnitX;
                return x.X;
            }
            if (x.Z > x.X && x.Z > x.Y)
            {
                component = Vector3.UnitZ;
                return x.Z;
            }
            component = Vector3.UnitY;
            return x.Y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MultiplyComponents(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        // this sucks
        public static T El<T>(this object[] arr, int p)
        {
            if (arr.Length > p)
            {
                return (T)arr[p];
            }
            else
            {
                return default;
            }
        }

    }
}
