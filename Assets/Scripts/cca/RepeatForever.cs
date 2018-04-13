using UnityEngine;

namespace cca
{
	/** @class RepeatForever
	 * @brief Repeats an action for ever.
	 To repeat the an action for a limited number of times use the Repeat action.
	 * @warning This action can't be Sequenceable because it is not an IntervalAction.
	 */
	public class RepeatForever : ActionInterval
	{
		/** Creates the action.
	     *
	     * @param action The action need to repeat forever.
	     * @return An autoreleased RepeatForever object.
	     */
		public RepeatForever (ActionInterval action)
			: base (0)
		{
			Debug.Assert (action != null, "action can't be null!");
			_innerAction = action;
		}

		public ActionInterval innerAction {
			/** Sets the inner action.
		     *
		     * @param action The inner action.
		     */
			set {
				_innerAction = value;
			}

			/** Gets the inner action.
		     *
		     * @return The inner action.
		     */
			get {
				return _innerAction;
			}
		}

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new RepeatForever (_innerAction.clone () as ActionInterval);
		}

		public override Action reverse ()
		{
			return new RepeatForever (_innerAction.reverse () as ActionInterval);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_innerAction.startWithTarget (target);
		}

		/**
	     * @param dt In seconds.
	     */
		public override void step (float dt)
		{
			_innerAction.step (dt);
			if (_innerAction.isDone ()) {
				float diff = _innerAction.elapsed - _innerAction.duration;
				if (diff > _innerAction.duration)
					diff = diff % _innerAction.duration;
				_innerAction.startWithTarget (_target);
				// to prevent jerk. issue #390, 1247
				_innerAction.step (0.0f);
				_innerAction.step (diff);
			}
		}

		public override bool isDone ()
		{
			return false;
		}

		/** Inner action */
		protected ActionInterval _innerAction;
	}
}
