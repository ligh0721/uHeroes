using UnityEngine;

namespace cca
{
	/** 
	 @class Ease
	 @brief class for Easing actions.
	 @details Ease actions are created from other interval actions.
	         The ease action will change the timeline of the inner action.
	 @ingroup Actions
	 */
	public class Ease : ActionEase
	{
		public delegate float EaseFunction (float time);

		public Ease (ActionInterval action, EaseFunction func)
			: base (action)
		{
			_function = func;
		}

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new Ease (_inner.clone () as ActionInterval, _function);
		}

		public override Action reverse ()
		{
			return new Ease (_inner.reverse () as ActionInterval, _function);
		}

		public override void update (float time)
		{
			_inner.update (_function != null ? _function (time) : time);
		}

		protected EaseFunction _function;

		protected const float _PI2 = Mathf.PI / 2;

		public static float funcSineIn (float time)
		{
			return -Mathf.Cos (time * _PI2) + 1;
		}

		public static float funcSineOut (float time)
		{
			return Mathf.Sin (time * _PI2);
		}

		public static float funcSineInOut (float time)
		{
			return -0.5f * (Mathf.Cos (_PI2 * time) - 1);
		}

		public static float funcQuadIn (float time)
		{
			return time * time;
		}

		public static float funcQuadOut (float time)
		{
			return -time * (time - 2);
		}

		public static float funcQuadInOut (float time)
		{
			time *= 2;
			if (time < 1) {
				return 0.5f * time * time;
			}
			--time;
			return -0.5f * (time * (time - 2) - 1);
		}

		public static float funcCubicIn (float time)
		{
			return time * time * time;
		}

		public static float funcCubicOut (float time)
		{
			--time;
			return time * time * time + 1;
		}

		public static float funcCubicInOut (float time)
		{
			time *= 2;
			if (time < 1) {
				return 0.5f * time * time * time;
			}
			time -= 2;
			return 0.5f * (time * time * time + 2);
		}

		public static float funcQuartIn (float time)
		{
			return time * time * time * time;
		}

		public static float funcQuartOut (float time)
		{
			--time;
			return -time * time * time * time + 1;
		}

		public static float funcQuartInOut (float time)
		{
			time *= 2;
			if (time < 1) {
				return 0.5f * time * time * time * time;
			}
			time -= 2;
			return -0.5f * (time * time * time * time - 2);
		}

		public static float funcQuintIn (float time)
		{
			return time * time * time * time * time;
		}

		public static float funcQuintOut (float time)
		{
			--time;
			return time * time * time * time * time + 1;
		}

		public static float funcQuintInOut (float time)
		{
			time *= 2;
			if (time < 1) {
				return 0.5f * time * time * time * time * time;
			}
			time -= 2;
			return 0.5f * (time * time * time * time * time + 2);
		}

		public static float funcExpoIn (float time)
		{
			return time == 0 ? 0 : Mathf.Pow (2, 10 * (time / 1 - 1)) - 1 * 0.001f;
		}

		public static float funcExpoOut (float time)
		{
			return time == 1 ? 1 : (-Mathf.Pow (2, -10 * time / 1) + 1);
		}

		public static float funcExpoInOut (float time)
		{
			time *= 2;
			if (time < 1) {
				time = 0.5f * Mathf.Pow (2, 10 * (time - 1));
			} else {
				time = 0.5f * (-Mathf.Pow (2, -10 * (time - 1)) + 2);
			}

			return time;
		}

		public static float funcCircIn (float time)
		{
			return 1 - Mathf.Sqrt (1 - time * time);
		}

		public static float funcCircOut (float time)
		{
			--time;
			return Mathf.Sqrt (1 - time * time);
		}

		public static float funcCircInOut (float time)
		{
			time *= 2;
			if (time < 1) {
				return -0.5f * (Mathf.Sqrt (1 - time * time) - 1);
			}
			time -= 2;
			return 0.5f * (Mathf.Sqrt (1 - time * time) + 1);
		}

		public static float funcBackIn (float time)
		{
			const float overshoot = 1.70158f;
			return time * time * ((overshoot + 1) * time - overshoot);
		}

		public static float funcBackOut (float time)
		{
			const float overshoot = 1.70158f;
			--time;
			return time * time * ((overshoot + 1) * time + overshoot) + 1;
		}

		public static float funcBackInOut (float time)
		{
			const float overshoot = 1.70158f * 1.525f;
			time *= 2;
			if (time < 1) {
				return (time * time * ((overshoot + 1) * time - overshoot)) / 2;
			} else {
				time -= 2;
				return (time * time * ((overshoot + 1) * time + overshoot)) / 2 + 1;
			}
		}

		protected static float _bounceTime (float time)
		{
			if (time < 1 / 2.75) {
				return 7.5625f * time * time;
			} else if (time < 2 / 2.75) {
				time -= 1.5f / 2.75f;
				return 7.5625f * time * time + 0.75f;
			} else if (time < 2.5 / 2.75) {
				time -= 2.25f / 2.75f;
				return 7.5625f * time * time + 0.9375f;
			}

			time -= 2.625f / 2.75f;
			return 7.5625f * time * time + 0.984375f;
		}

		public static float funcBounceIn (float time)
		{
			return 1 - _bounceTime (1 - time);
		}

		public static float funcBounceOut (float time)
		{
			return _bounceTime (time);
		}

		public static float funcBounceInOut (float time)
		{
			float newT = 0;
			if (time < 0.5f) {
				time = time * 2;
				newT = (1 - _bounceTime (1 - time)) * 0.5f;
			} else {
				newT = _bounceTime (time * 2 - 1) * 0.5f + 0.5f;
			}

			return newT;
		}
	}
}
