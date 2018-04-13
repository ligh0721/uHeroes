using UnityEngine;

namespace cca
{
	/** @class ScaleBy
	 * @brief Scales a Node object a zoom factor by modifying it's scale attribute.
	 @warning The physics body contained in Node doesn't support this action.
	*/
	public class ScaleBy : ScaleTo
	{
		/** 
	     * Creates the action with the same scale factor for X and Y.
	     * @param duration duration time, in seconds.
	     * @param s scale factor of x and y.
	     * @return An autoreleased ScaleBy object.
	     */
		public ScaleBy (float duration, float s)
			: this (duration, s, s, s)
		{
		}

		/** 
	     * Creates the action with and X factor and a Y factor.
	     * @param duration duration time, in seconds.
	     * @param sx scale factor of x.
	     * @param sy scale factor of y.
	     * @return An autoreleased ScaleBy object.
	     */
		public ScaleBy (float duration, float sx, float sy)
			: this (duration, sx, sy, 1.0f)
		{
		}

		/** 
	     * Creates the action with X Y Z factor.
	     * @param duration duration time, in seconds.
	     * @param sx scale factor of x.
	     * @param sy scale factor of y.
	     * @param sz scale factor of z.
	     * @return An autoreleased ScaleBy object.
	     */
		public ScaleBy (float duration, float sx, float sy, float sz)
			: base (duration, sx, sy, sz)
		{
		}
		
		//
		// Overrides
		//
		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_delta.x = _startScale.x * _endScale.x - _startScale.x;
			_delta.y = _startScale.y * _endScale.y - _startScale.y;
			_delta.z = _startScale.z * _endScale.z - _startScale.z;
		}

		public override Action clone ()
		{
			return new ScaleBy (_duration, _endScale.x, _endScale.y, _endScale.z);
		}

		public override Action reverse ()
		{
			return new ScaleBy (_duration, 1 / _endScale.x, 1 / _endScale.y, 1 / _endScale.z);
		}
	}
}
