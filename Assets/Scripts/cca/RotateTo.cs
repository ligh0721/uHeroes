using UnityEngine;

namespace cca
{
	/** @class RotateTo
	 * @brief Rotates a Node object to a certain angle by modifying it's rotation attribute.
	 The direction will be decided by the shortest angle.
	*/ 
	public class RotateTo : RotateBy
	{
		/** 
	     * Creates the action.
	     *
	     * @param duration duration time, in seconds.
	     * @param dstAngle In degreesCW.
	     * @return An autoreleased RotateTo object.
	     */
		public RotateTo (float duration, float dstAngle)
			: base(duration, 0)
		{
            _dstAngle = dstAngle;
        }

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new RotateTo (_duration, _dstAngle);
		}

		public override Action reverse ()
		{
			Debug.Assert (false, "RotateTo doesn't support the 'reverse' method");
			return null;
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_deltaAngle = _dstAngle - target.rotation;
		}

		protected float _dstAngle;
	}
}
