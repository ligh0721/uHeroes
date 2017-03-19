using UnityEngine;

namespace cca
{
	/** @class RotateBy
	 * @brief Rotates a Node object clockwise a number of degrees by modifying it's rotation attribute.
	*/
	public class RotateBy : ActionInterval
	{
		/** 
	     * Creates the action.
	     *
	     * @param duration duration time, in seconds.
	     * @param deltaAngle In degreesCW.
	     * @return An autoreleased RotateBy object.
	     */
		public RotateBy (float duration, float deltaAngle)
			: base (duration)
		{
            _deltaAngle = deltaAngle;
		}

		//
		// Override
		//
		public override Action clone ()
		{
			return new RotateBy (_duration, _deltaAngle);
		}

		public override Action reverse ()
		{
			return new RotateBy (_duration, -_deltaAngle);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_startAngle = target.rotation;
		}

		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			if (_target) {
				_target.rotation = _startAngle + _deltaAngle * time;
			}
		}

		protected float _deltaAngle;
		protected float _startAngle;
	}
}
