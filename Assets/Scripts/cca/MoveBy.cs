using UnityEngine;

namespace cca
{
	/** @class MoveBy
	 * @brief Moves a Node object x,y pixels by modifying it's position attribute.
	 x and y are relative to the position of the object.
	 Several MoveBy actions can be concurrently called, and the resulting
	 movement will be the sum of individual movements.
	 @since v2.1beta2-custom
	 */
	public class MoveBy : ActionInterval
	{
		/** 
	     * Creates the action.
	     *
	     * @param duration duration time, in seconds.
	     * @param deltaPosition The delta distance in 2d, it's a Vec2 type.
	     * @return An autoreleased MoveBy object.
	     */
		public MoveBy (float duration, Vector2 deltaPosition)
			: base (duration)
		{
            _positionDelta = deltaPosition;
        }

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new MoveBy (_duration, _positionDelta);
		}

		public override Action reverse ()
		{
			return new MoveBy (_duration, -_positionDelta);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_previousPosition = _startPosition = target.position;
		}

		/**
	     * @param time in seconds
	     */
		public override void update (float time)
		{
			if (_target) {
#if CC_ENABLE_STACKABLE_ACTIONS
				Vector3 currentPos = _target.transform.localPosition;
				Vector3 diff = currentPos - _previousPosition;
				_startPosition = _startPosition + diff;
				Vector3 newPos = _startPosition + (_positionDelta * time);
				_target.transform.postion = newPos;
				_previousPosition = newPos;
#else
				_target.position = _startPosition + _positionDelta * time;

#endif // CC_ENABLE_STACKABLE_ACTIONS
			}
		}

		protected Vector2 _positionDelta;
		protected Vector2 _startPosition;
		protected Vector2 _previousPosition;
	}
}
