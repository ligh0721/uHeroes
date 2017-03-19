using UnityEngine;

namespace cca
{
	/** @class MoveTo
	 * @brief Moves a Node object to the position x,y. x and y are absolute coordinates by modifying it's position attribute.
	 Several MoveTo actions can be concurrently called, and the resulting
	 movement will be the sum of individual movements.
	 @since v2.1beta2-custom
	 */
	public class MoveTo : MoveBy
	{
		/** 
	     * Creates the action.
	     * @param duration duration time, in seconds.
	     * @param position The destination position in 2d.
	     * @return An autoreleased MoveTo object.
	     */
		public MoveTo (float duration, Vector2 position)
			: base (duration, new Vector2 ())
		{
            _endPosition = position;
            //_needFixZ = true;
        }

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new MoveTo (_duration, _endPosition);
		}

		public override Action reverse ()
		{
			Debug.Assert (false, "reverse() not supported in MoveTo");
			return null;
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			//if (_needFixZ) {
				//_endPosition.z = target.position.z;
			//}
			_positionDelta = _endPosition - target.position;
		}

		protected Vector2 _endPosition;
		//protected bool _needFixZ = false;
	}
}
