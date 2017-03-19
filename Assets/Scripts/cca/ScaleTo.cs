using UnityEngine;

namespace cca
{
	/** @class ScaleTo
	 @brief Scales a Node object to a zoom factor by modifying it's scale attribute.
	 @warning This action doesn't support "reverse".
	 @warning The physics body contained in Node doesn't support this action.
	 */
	public class ScaleTo : ActionInterval
	{
		/** 
	     * Creates the action with the same scale factor for X and Y.
	     * @param duration duration time, in seconds.
	     * @param s scale factor of x and y.
	     * @return An autoreleased ScaleTo object.
	     */
		public ScaleTo (float duration, float s)
			: this (duration, s, s, s)
		{
		}

		/** 
	     * Creates the action with and X factor and a Y factor.
	     * @param duration duration time, in seconds.
	     * @param sx scale factor of x.
	     * @param sy scale factor of y.
	     * @return An autoreleased ScaleTo object.
	     */
		public ScaleTo (float duration, float sx, float sy)
			: this (duration, sx, sy, 1.0f)
		{
		}

		/** 
	     * Creates the action with X Y Z factor.
	     * @param duration duration time, in seconds.
	     * @param sx scale factor of x.
	     * @param sy scale factor of y.
	     * @param sz scale factor of z.
	     * @return An autoreleased ScaleTo object.
	     */
		public ScaleTo (float duration, float sx, float sy, float sz)
			: base (duration)
		{
			_endScale.x = sx;
			_endScale.y = sy;
			_endScale.z = sz;
		}
		
		//
		// Overrides
		//
		public override Action clone ()
		{
			return new ScaleTo (_duration, _endScale.x, _endScale.y, _endScale.z);
		}

		public override Action reverse ()
		{
			Debug.Assert (false, "reverse() not supported in ScaleTo");
			return null;
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_startScale = target.scale;
			_delta = _endScale - _startScale;
		}

		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			if (_target) {
				_target.scale = _startScale + _delta * time;
			}
		}

		protected Vector3 _scale;
		protected Vector3 _startScale;
		protected Vector3 _endScale;
		protected Vector3 _delta;
	}
}
