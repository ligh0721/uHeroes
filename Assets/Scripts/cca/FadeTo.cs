using UnityEngine;

namespace cca
{
	/** @class FadeTo
	 * @brief Fades an object that implements the RGBAProtocol protocol. It modifies the opacity from the current value to a custom one.
	 @warning This action doesn't support "reverse"
	 */
	public class FadeTo : ActionInterval
	{
		/** 
	     * Creates an action with duration and opacity.
	     * @param duration duration time, in seconds.
	     * @param opacity A certain opacity, the range is from 0.0f to 1.0f.
	     * @return An autoreleased FadeTo object.
	     */
		public FadeTo (float duration, float opacity)
			: base (duration)
		{
			_toOpacity = opacity;
		}

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new FadeTo (_duration, _toOpacity);
		}

		public override Action reverse ()
		{
			Debug.Assert (false, "reverse() not supported in FadeTo");
			return null;
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);

			if (target) {
				_fromOpacity = target.opacity;
			}
		}

		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			if (_target) {
				_target.opacity = _fromOpacity + (_toOpacity - _fromOpacity) * time;
			}
		}

		protected float _toOpacity;
		protected internal float _fromOpacity;
	}
}
