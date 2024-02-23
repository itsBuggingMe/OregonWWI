using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI
{
    /// <summary>
    /// Simple Tweening Class, interpolates between two floating point values. For more information, visit https://easings.net/
    /// <br>Contains an optional onEnd Action</br>
    /// </summary>
    internal class Animation
    {
        public static List<Animation> Animations = new();

        public bool Delete { get; private set; } = false;

        public List<KeyFrame<float>> KeyFrames { get; private set; } = new List<KeyFrame<float>>();
        protected KeyFrame<float> PrevKeyFrame;

        protected float t;

        protected readonly Action<float> value;
        protected Action? OnEnd;

        /// <summary>
        /// Parameter ticks is the length of time in 1/60 second increments. 1 second animation = 60 ticks
        /// </summary>
        public Animation(Action<float> modifier, float startingValue, Action? OnEnd = null, params KeyFrame<float>[] keyFrames)
        {
            if (keyFrames.Length == 0)
                throw new ArgumentException("Animation must have at least one keyframe", nameof(keyFrames));

            value = modifier;
            KeyFrames = keyFrames.ToList();
            this.OnEnd = OnEnd;
            PrevKeyFrame = new KeyFrame<float>(startingValue, 0, AnimationType.Linear);
        }

        public Animation(Action<float> Modifer, int ticks, Action? OnEnd = null, AnimationType animationType = AnimationType.Linear) : this(Modifer, 0, OnEnd, new KeyFrame<float>(1, ticks, animationType))
        {

        }

        public void AnimationUpdate(GameTime gameTime)
        {
            if (Delete)
                return;

            const float m = 0.06f;
            t += (float)(KeyFrames[0].TicksRec * gameTime.ElapsedGameTime.TotalMilliseconds * m);

            if (t < 1.0f)
            {
                if (KeyFrames[0].AnimationType == AnimationType.None)
                    return;
                value(ComputeAnimatedValue());
            }
            else
            {
                PrevKeyFrame = KeyFrames[0];

                KeyFrames.RemoveAt(0);

                if (KeyFrames.Count == 0)
                {
                    Delete = true;
                    OnEnd?.Invoke();
                    Animations.Remove(this);
                    return;
                }

                t = 0;

                if (KeyFrames[0].AnimationType == AnimationType.PrevAnimationType)
                {
                    var k = KeyFrames[0];
                    k.AnimationType = PrevKeyFrame.AnimationType;
                    KeyFrames[0] = k;
                }
            }
        }

        public void SetKeyFrame(int index, KeyFrame<float> keyFrame) => KeyFrames[index] = keyFrame;
        public void AddKeyFrame(KeyFrame<float> keyFrame) => KeyFrames.Add(keyFrame);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ComputeAnimatedValue()
        {
            return KeyFrames[0].AnimationType switch
            {
                AnimationType.Linear => t,
                AnimationType.Parabolic => ParabolicInterpolation(t),
                AnimationType.Cubic => CubicInterpolation(t),
                AnimationType.InverseCubic => InverseCubicInterpolation(t),
                AnimationType.Sigmoid => SigmoidInterpolation(t),
                AnimationType.InverseParabolic => InverseParabolicInterpolation(t),
                AnimationType.EaseInOutQuad => EaseInOutQuadInterpolation(t),
                AnimationType.EaseInOutExpo => EaseInOutExpoInterpolation(t),
                AnimationType.EaseInOutQuart => EaseInOutQuartInterpolation(t),
                AnimationType.EaseOutBounce => EaseOutBounceInterpolation(t),
                AnimationType.EaseInOutBounce => EaseInOutBounceInterpolation(t),
                AnimationType.EaseInBounce => EaseInBounceInterpolation(t),
                AnimationType.PrevAnimationType => throw new ArgumentException("No previous animation keyframe"),
                _ => throw new ArgumentException("No Animation Type implementaion"),
            } * (KeyFrames[0].EndValue - PrevKeyFrame.EndValue) + PrevKeyFrame.EndValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ParabolicInterpolation(float t) => t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InverseParabolicInterpolation(float t) => 1 - (1 - t) * (1 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CubicInterpolation(float t) => t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InverseCubicInterpolation(float t) => 1 - (1 - t) * (1 - t) * (1 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SigmoidInterpolation(float t) => 1 / (1 + MathF.Exp(10 * (-t + 0.5f)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutQuadInterpolation(float x) => x < 0.5 ? 2 * x * x : 1 - MathF.Pow(-2 * x + 2, 2) / 2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutQuartInterpolation(float x) => x < 0.5f ? 8 * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 4) / 2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutExpoInterpolation(float x) => x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? MathF.Pow(2, 20 * x - 10) / 2 : (2 - MathF.Pow(2, -20 * x + 10)) / 2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutBounceInterpolation(float x) => x switch { < 1 / 2.75f => 7.5625f * x * x, < 2 / 2.75f => 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f, < 2.5f / 2.75f => 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f, _ => 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutBounceInterpolation(float x) => x < 0.5f ? (1 - EaseOutBounceInterpolation(1 - 2 * x)) / 2 : (1 + EaseOutBounceInterpolation(2 * x - 1)) / 2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInBounceInterpolation(float x) => 1 - EaseOutBounceInterpolation(1 - x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddAnimation(Action<float> modifier, float startingValue, Action OnEnd, params KeyFrame<float>[] keyFrames) => _ = new Animation(modifier, startingValue, OnEnd, keyFrames);
    }

    /// <summary>
    /// Used to detemines the animation style
    /// </summary>
    internal enum AnimationType
    {
        Linear,
        Parabolic,
        Cubic,
        InverseCubic,

        /// <summary>
        /// Prefer Quadradic
        /// </summary>
        Sigmoid,

        InverseParabolic,
        EaseInOutQuad,
        EaseInOutQuart,
        EaseInOutExpo,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInBounce,

        PrevAnimationType,
        None,
    }

    /// <summary>
    /// Represents a single key frame for the animation class
    /// </summary>
    internal struct KeyFrame<T>
    {
        public AnimationType AnimationType;
        public T EndValue;
        public float TicksRec;

        public readonly int Ticks => (int)Math.Round(1 / TicksRec, 0);

        public KeyFrame(T EndValue, int ticks, AnimationType animationType = AnimationType.PrevAnimationType)
        {
            this.EndValue = EndValue;
            AnimationType = animationType;
            TicksRec = 1f / ticks;
        }
    }
}
