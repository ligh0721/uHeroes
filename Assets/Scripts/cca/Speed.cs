using UnityEngine;

namespace cca
{
	/** @class Speed
	 * @brief Changes the speed of an action, making it take longer (speed>1)
	 * or less (speed<1) time.
	 * Useful to simulate 'slow motion' or 'fast forward' effect.
	 * @warning This action can't be Sequenceable because it is not an IntervalAction.
	 */
	public class Speed : Action
	{
		/** Create the action and set the speed.
	     *
	     * @param action An action.
	     * @param speed The action speed.
	     */
		public Speed (ActionInterval action, float speed)
		{
			Debug.Assert (action != null, "action must not be null");
			_innerAction = action;
			_speed = speed;
		}

		public float ActionSpeed {
			/** Return the speed.
		     *
		     * @return The action speed.
		     */
			get {
				return _speed;
			}
			/** Alter the speed of the inner function in runtime. 
		     *
		     * @param speed Alter the speed of the inner function in runtime.
		     */
			set {
				_speed = value;
			}
		}

		public ActionInterval innerAction {
			/** Replace the interior action.
		     *
		     * @param action The new action, it will replace the running action.
		     */
			set {
				_innerAction = value;
			}
			/** Return the interior action.
		     *
		     * @return The interior action.
		     */
			get {
				return _innerAction;
			}
		}

		//
		// Override
		//
		public override Action clone ()
		{
			return new Speed (_innerAction.clone () as ActionInterval, _speed);
		}

		public override Action reverse ()
		{
			return new Speed (_innerAction.reverse () as ActionInterval, _speed);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_innerAction.startWithTarget (target);
		}

		public override void stop ()
		{
			_innerAction.stop ();
			base.stop ();
		}

		/**
	     * @param dt in seconds.
	     */
		public override void step (float dt)
		{
			_innerAction.step (dt * _speed);
		}

		/** Return true if the action has onFinished.
	     *
	     * @return Is true if the action has onFinished.
	     */
		public override bool isDone ()
		{
			return _innerAction.isDone ();
		}

		protected float _speed = 0;
		ActionInterval _innerAction;
	}
}
