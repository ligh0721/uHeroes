using UnityEngine;

namespace cca
{
	/** @class Repeat
	 * @brief Repeats an action a number of times.
	 * To repeat an action forever use the RepeatForever action.
	 */
	public class Repeat : ActionInterval
	{
		/** Creates a Repeat action. Times is an unsigned integer between 1 and pow(2,30).
	     *
	     * @param action The action needs to repeat.
	     * @param times The repeat times.
	     * @return An autoreleased Repeat object.
	     */
		public Repeat (FiniteTimeAction action, uint times)
			: base (action.duration * times)
		{
			_innerAction = action;
			_times = times;
			_actionInstant = _innerAction as ActionInstant != null ? true : false;
			//an instant action needs to be executed one time less in the update method since it uses startWithTarget to execute the action
			if (_actionInstant) {
				_times -= 1;
			}
			_total = 0;
		}

		public FiniteTimeAction innerAction {
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
			return new Repeat (_innerAction.clone () as FiniteTimeAction, _innerAction as ActionInstant != null ? _times + 1 : _times);
		}

		public override Action reverse ()
		{
			return new Repeat (_innerAction.reverse () as FiniteTimeAction, _innerAction as ActionInstant != null ? _times + 1 : _times);
		}

		public override void startWithTarget (Node target)
		{
			_total = 0;
			_nextDt = _innerAction.duration / _duration;
			base.startWithTarget (target);
			_innerAction.startWithTarget (target);
		}

		public override void stop ()
		{
			_innerAction.stop ();
			base.stop ();
		}

		/**
	     * @param dt In seconds.
	     */
		public override void update (float time)
		{
			if (time >= _nextDt) {
				while (time > _nextDt && _total < _times) {
					_innerAction.update (1.0f);
					++_total;

					_innerAction.stop ();
					_innerAction.startWithTarget (_target);
					_nextDt = _innerAction.duration / _duration * (_total + 1);
				}

				// fix for issue #1288, incorrect end value of repeat
				if (time >= 1.0f && _total < _times) {
					++_total;
				}

				// don't set an instant action back or update it, it has no use because it has no duration
				if (!_actionInstant) {
					if (_total == _times) {
						_innerAction.update (1);
						_innerAction.stop ();
					} else {
						// issue #390 prevent jerk, use right update
						_innerAction.update (time - (_nextDt - _innerAction.duration / _duration));
					}
				}
			} else {
				_innerAction.update ((time * _times) % 1.0f);
			}
		}

		public override bool isDone ()
		{
			return _total == _times;
		}

		protected uint _times;
		protected uint _total;
		protected float _nextDt;
		protected bool _actionInstant;
		/** Inner action */
		protected FiniteTimeAction _innerAction;
	}
}
