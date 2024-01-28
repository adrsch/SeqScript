// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Newtonsoft.Json;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public static class Constants
    {
        public const int Build = 1;
        public const string FolderName = "DEV";
        public const string SaveFolder = "UserSaves";
        public const string AutosaveFile = "Autosave.json";
        public const string OldAutosaveFile = "OldAutosave.json";
        public const string OlderAutosaveFile = "OlderAutosave.json";

        public const float Deg2Rad = 0.0174533f;
        public const float Rad2Deg = 57.2958f;
    }

    public static class Utils
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float VFov(float hfov)
        {
            var asp = ((float)G.S.Window.ClientBounds.Height) / ((float)G.S.Window.ClientBounds.Width);
            var vfov =  2 * MathF.Atan(MathF.Tan(hfov * Constants.Deg2Rad / 2f) * asp) * Constants.Rad2Deg;
            return vfov;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float HFov(float vfov)
        {
            var asp = G.S.Window.ClientBounds.Height / G.S.Window.ClientBounds.Width;
           // var asp = Screen.height / Screen.width;
            return 2 * MathF.Atan(MathF.Tan(vfov * Constants.Deg2Rad / 2f) / asp) * Constants.Rad2Deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LogicDirectionToWorldDirection(Vector2 logicDirection, CameraComponent camera, Vector3 upVector)
        {
            camera.Update();
            var inverseView = Matrix.Invert(camera.ViewMatrix);

            var forward = Vector3.Cross(upVector, inverseView.Right);
            forward.Normalize();

            var right = Vector3.Cross(forward, upVector);
            var worldDirection = forward * logicDirection.Y + right * logicDirection.X;
            worldDirection.Normalize();
            return worldDirection;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LogicDirectionToWorldDirection(Vector3 logicDirection, CameraComponent camera, Vector3 upVector)
        {
            camera.Update();
            var inverseView = Matrix.Invert(camera.ViewMatrix);

            var forward = Vector3.Cross(upVector, inverseView.Right);
            forward.Normalize();

            var right = Vector3.Cross(forward, upVector);
            var worldDirection = forward * logicDirection.Y + right * logicDirection.X;
            worldDirection.Normalize();
            return worldDirection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b, float c)
        {
            return MathF.Max(a, MathF.Max(b, c));
        }
            public static System.Random Random = new System.Random();

            public static bool ParsesAsNil(string s) =>
                string.IsNullOrWhiteSpace(s) || s == "nil" || s == "null" || s == "none";

            public static float GetRandomNormal(float mean, float stdev)
            {
                return (float)GetRandomNormal((double)mean, (double)stdev);
            }
            //https://stackoverflow.com/questions/218060/random-gaussian-variables
            public static double GetRandomNormal(double mean, double stdev)
            {
                double u1 = 1.0 - Random.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - Random.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                return mean + stdev * randStdNormal; //random normal(mean,stdDev^2)
            }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStep(float t) => t * t * (3f - 2f * t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmootherStep(float t) => t * t * t * (t * (6f * t - 15f) + 10f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOut(float t) => MathF.Sin(t * MathF.PI * 0.5f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseIn(float t) => 1f - MathF.Cos(t * MathF.PI * 0.5f);
        /*
            public static Color LerpColor(Color a, Color b, float t) => new Color(
                Single.Lerp(a.R, b.R, t),
                 MathF.Lerp(a.G, b.G, t),
                  MathF.Lerp(a.B, b.B, t),
                   MathF.Lerp(a.A, b.A, t)
                );
        */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Accelerate(Vector3 targetDir, float targetSpeed, float accel, ref Vector3 currentVelocity, float dt)
            {
                float currentspeed = Vector3.Dot(currentVelocity, targetDir);
                float addspeed = targetSpeed - currentspeed;
                if (addspeed <= 0)
                {
                    return;
                }

                float accelspeed = accel * dt * targetSpeed;
                if (accelspeed > addspeed)
                {
                    accelspeed = addspeed;
                }

                currentVelocity.X += accelspeed * targetDir.X;
                currentVelocity.Z += accelspeed * targetDir.Z;
            }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Accelerate3D(Vector3 targetDir, float targetSpeed, float accel, ref Vector3 currentVelocity, float dt)
            {
                float currentspeed = Vector3.Dot(currentVelocity, targetDir);
                float addspeed = targetSpeed - currentspeed;
                // if (addspeed <= 0)
                //{
                //   return;
                // }

                float accelspeed = accel * dt * targetSpeed;
                if (accelspeed > addspeed)
                {
                    accelspeed = addspeed;
                }

                currentVelocity.X += accelspeed * targetDir.X;
                currentVelocity.Y += accelspeed * targetDir.Y;
                currentVelocity.Z += accelspeed * targetDir.Z;
            }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CollisionVelocity(float ma, float mb, Vector3 va, Vector3 vb)
            {
                return (ma * va + mb * vb) / (ma + mb);
            }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpIndependent(float pos, float dt) => 1 - MathF.Pow(pos, dt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
                Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
            {

                Vector3 lineVec3 = linePoint2 - linePoint1;
                Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
                Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

                float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

                //is coplanar, and not parallel
                if (MathF.Abs(planarFactor) < 0.0001f
                        && crossVec1and2.LengthSquared() > 0.0001f)
                {
                    float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.LengthSquared();
                    intersection = linePoint1 + (lineVec1 * s);
                    return true;
                }
                else
                {
                    intersection = Vector3.Zero;
                    return false;
                }
            }


            private static System.Random rng = new System.Random();

            public static void Shuffle<T>(this IList<T> list)
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }

            public static string FormatTime(int hours, int minutes)
            {
                hours = hours % 24;
                minutes = minutes % 60;
                return
                    $"{(hours > 12 ? hours - 12 : hours < 1 ? 12 : hours):D2}:{minutes:D2} {(hours >= 12 ? "PM" : "AM")}";
            }

            public static Vector3 FirstOrderIntercept
            (
            Vector3 shooterPosition,
            Vector3 shooterVelocity,
            float shotSpeed,
            Vector3 targetPosition,
            Vector3 targetVelocity
            )

            {
                Vector3 targetRelativePosition = targetPosition - shooterPosition;
                Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
                float t = FirstOrderInterceptTime
                (
                shotSpeed,
                targetRelativePosition,
                targetRelativeVelocity
                );
                return targetPosition + t * (targetRelativeVelocity);
            }



            public static float FirstOrderInterceptTime
            (
            float shotSpeed,
            Vector3 targetRelativePosition,
            Vector3 targetRelativeVelocity
            )
            {
                float velocitySquared = targetRelativeVelocity.LengthSquared();
                if (velocitySquared < 0.001f)
                    return 0f;

                float a = velocitySquared - shotSpeed * shotSpeed;

                //handle similar velocities
                if (MathF.Abs(a) < 0.001f)
                {
                    float t = -targetRelativePosition.LengthSquared() /
                    (
                    2f * Vector3.Dot
                    (
                    targetRelativeVelocity,
                    targetRelativePosition
                    )
                    );
                    return MathF.Max(t, 0f); //don't shoot back in time
                }

                float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
                float c = targetRelativePosition.LengthSquared();
                float determinant = b * b - 4f * a * c;

                if (determinant > 0f)
                { //determinant > 0; two intercept paths (most common)
                    float t1 = (-b + MathF.Sqrt(determinant)) / (2f * a),
                    t2 = (-b - MathF.Sqrt(determinant)) / (2f * a);
                    if (t1 > 0f)
                    {
                        if (t2 > 0f)
                            return MathF.Min(t1, t2); //both are positive
                        else
                            return t1; //only t1 is positive
                    }
                    else
                        return MathF.Max(t2, 0f); //don't shoot back in time
                }
                else if (determinant < 0f) //determinant < 0; no intercept path
                    return 0f;
                else //determinant = 0; one intercept path, pretty much never happens
                    return MathF.Max(-b / (2f * a), 0f); //don't shoot back in time
            }

        /*

            //https://gist.github.com/maxattack/4c7b4de00f5c1b95a33b
            public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
            {
                if (Time.deltaTime < float.Epsilon) return rot;
                // account for double-cover
                var Dot = Quaternion.Dot(rot, target);
                var Multi = Dot > 0f ? 1f : -1f;
                target.X *= Multi;
                target.Y *= Multi;
                target.Z *= Multi;
                target.w *= Multi;

                // smooth damp (nlerp approx)
                var Result = new Vector4(
                    Utils.SmoothCD(rot.X, target.X, ref deriv.X, time),
                    Utils.SmoothCD(rot.Y, target.Y, ref deriv.Y, time),
                    Utils.SmoothCD(rot.Z, target.Z, ref deriv.Z, time),
                    Utils.SmoothCD(rot.w, target.w, ref deriv.w, time)
                ).normalized;

                // ensure deriv is tangent
                var derivError = Vector4.Project(new Vector4(deriv.X, deriv.Y, deriv.Z, deriv.w), Result);
                deriv.X -= derivError.X;
                deriv.Y -= derivError.Y;
                deriv.Z -= derivError.Z;
                deriv.w -= derivError.w;

                return new Quaternion(Result.X, Result.Y, Result.Z, Result.w);
            }
        */
        /*
            public static Rect BoundsToScreenRect(Bounds bounds, Camera cam)
            {
                // Get mesh origin and farthest extent (this works best with simple convex meshes)
                Vector3 origin = cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, 0f));
                Vector3 extent = cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, 0f));

                // Create rect in screen space and return - does not account for camera perspective
                return new Rect(origin.X, cam.pixelHeight - origin.Y, extent.X - origin.X, origin.Y - extent.Y);
            }
            public static Vector4 GetScreenBounds(Bounds bounds, Camera cam)
            {
                var minx = MathF.Min(
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.min.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.min.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.min.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.max.Y, bounds.max.Z)).X);
                var maxX = MathF.Max(
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.min.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.min.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.min.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).X,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.max.Y, bounds.max.Z)).X);
                var minY = MathF.Min(
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.min.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.min.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.min.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.max.Y, bounds.max.Z)).Y);
                var maxY = MathF.Max(
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.min.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.min.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.max.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.min.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.min.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.min.X, bounds.min.Y, bounds.max.Z)).Y,
                    cam.WorldToScreenPoint(new Vector3(bounds.max.X, bounds.max.Y, bounds.max.Z)).Y);
                if (!Pointer.Inst.UseRenderTexture)
                    return new Vector4(
                        minx,
                        minY,
                        maxX,
                        maxY);
                else
                    return Pointer.Inst.GetBoundsForScreen(new Vector4(
                        minx,
                        minY,
                        maxX,
                        maxY));
            }
        */
            public static DateTime GetWeekStart(this DateTime today)
            {
                int daysToSubtract = (int)today.DayOfWeek;
                var timeSpan = new TimeSpan(days: daysToSubtract, 0, 0, 0);
                return today - timeSpan;
            }
        }
    }