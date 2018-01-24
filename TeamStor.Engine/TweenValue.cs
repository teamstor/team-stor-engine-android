using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStor.Engine
{
    public enum TweenEaseType
    {
        Linear,

        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,

        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,

        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,

        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,

        EaseInSine,
        EaseOutSine,
        EaseInOutSine,

        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,

        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,

        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,

        EaseInBack,
        EaseOutBack,
        EaseInOutBack,

        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce
    }

    /// <summary>
    /// Value that can be tweened to other values.
    /// Based on https://github.com/danro/jquery-easing/blob/master/jquery.easing.js
    /// </summary>
    public class TweenValue
    {
        private Game _game;
        private double _sourceValue;
        private double _duration;

        /// <summary>
        /// Current value.
        /// </summary>
        public double Value
        {
            get
            {
                return EaseWithType(EaseType, MathHelper.Clamp((float)(_game.Time / CompletionTime), 0f, 1f), _sourceValue, TargetValue - _sourceValue);
            }
        }

        /// <summary>
        /// Target value.
        /// </summary>
        public double TargetValue
        {
            get; private set;
        }

        /// <summary>
        /// Time when the tween will complete.
        /// </summary>
        public double CompletionTime
        {
            get; private set;
        }

        /// <summary>
        /// Current ease type.
        /// </summary>
        public TweenEaseType EaseType
        {
            get; private set;
        }

        /// <summary>
        /// If the current tween is completed.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _game.Time >= CompletionTime;
            }
        }
        
        public TweenValue(Game game, double initialValue)
        {
            _game = game;
            CompletionTime = game.Time;
            _duration = 1.0f;

            _sourceValue = initialValue;
            TargetValue = initialValue;
        }

        /// <summary>
        /// Tweens the value to the new value.
        /// </summary>
        public void TweenTo(double newValue, TweenEaseType easeType, double duration = 1.0f)
        {
            _sourceValue = Value;
            TargetValue = newValue;

            CompletionTime = _game.Time + duration;

            _duration = duration;
            EaseType = easeType;
        }

        /// <summary>
        /// Eases with the specified ease type.
        /// </summary>
        public static double EaseWithType(TweenEaseType easeType, double amount, double startValue, double change)
        {
            switch(easeType)
            {
                case TweenEaseType.Linear:
                    return MathHelper.Lerp((float)startValue, (float)startValue + (float)change, (float)amount);

                case TweenEaseType.EaseInQuad:
                    return change * amount * amount + startValue;
                case TweenEaseType.EaseOutQuad:
                    return -change * amount * (amount - 2) + startValue;
                // TODO: lägg till resten
            }
            return startValue + change;
        }
    }
}
