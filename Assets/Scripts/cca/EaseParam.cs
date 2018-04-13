using UnityEngine;

namespace cca
{
	/** 
	 @class EaseParam
	 @brief Base class for Easing actions with one parameter.
	 @details Ease the inner action with specified param.
	 @ingroup Actions
	 */
	public class EaseParam : ActionEase
	{
		public delegate float EaseParamFunction (float time,float param);

		/**
	     @brief Creates the action with the inner action and the param parameter.
	     @param action A given ActionInterval
	     @param param A given param
	     @return An autoreleased EaseParam object.
	    **/
		public EaseParam (ActionInterval action, EaseParamFunction func, float param)
			: base (action)
		{
			_function = func;
			_param = param;
		}

		public float param {
			/**
		     @brief Set the param value for the ease param action.
		     @param param The value will be set.
		     */
			set {
				_param = value;
			}
			/**
		     @brief Get the param value of the ease param action.
		     @return Return the param value of the ease param action.
		     */
			get {
				return _param;
			}
		}
		//
		// Overrides
		//
		public override Action clone ()
		{
			return new EaseParam (_inner.clone () as ActionInterval, _function, _param);
		}

		public override Action reverse ()
		{
			return new EaseParam (_inner.reverse () as ActionInterval, _function, _param);
		}

		public override void update (float time)
		{
			_inner.update (_function != null ? _function (time, _param) : time);
		}

		protected EaseParamFunction _function;
		protected float _param;

		public static float funcPowIn (float time, float param)
		{
			return Mathf.Pow (time, param);
		}

		public static float funcPowOut (float time, float param)
		{
			return Mathf.Pow (time, 1 / param);
		}

		public static float funcPowInOut (float time, float param)
		{
			time *= 2;
			if (time < 1) {
				return 0.5f * Mathf.Pow (time, param);
			} else {
				return (1 - 0.5f * Mathf.Pow (2 - time, param));
			}
		}

		protected const float _PIx2 = Mathf.PI * 2;

		public const float defElasticPeriod = 0.3f;

		public static float funcElasticIn (float time, float period)
		{
			float newT = 0;
			if (time == 0 || time == 1) {
				newT = time;
			} else {
				float s = period / 4;
				--time;
				newT = -Mathf.Pow (2, 10 * time) * Mathf.Sin ((time - s) * _PIx2 / period);
			}

			return newT;
		}

		public static float funcElasticOut (float time, float period)
		{
			float newT = 0;
			if (time == 0 || time == 1) {
				newT = time;
			} else {
				float s = period / 4;
				newT = Mathf.Pow (2, -10 * time) * Mathf.Sin ((time - s) * _PIx2 / period) + 1;
			}

			return newT;
		}

		public static float funcElasticInOut (float time, float period)
		{
			float newT = 0;
			if (time == 0 || time == 1) {
				newT = time;
			} else {
				time = time * 2;
				if (period == 0) {
					period = 0.3f * 1.5f;
				}

				float s = period / 4;

				--time;
				if (time < 0) {
					newT = -0.5f * Mathf.Pow (2, 10 * time) * Mathf.Sin ((time - s) * _PIx2 / period);
				} else {
					newT = Mathf.Pow (2, -10 * time) * Mathf.Sin ((time - s) * _PIx2 / period) * 0.5f + 1;
				}
			}

			return newT;
		}
	}
}
