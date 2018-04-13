using UnityEngine;


namespace cca
{
	/** @class ActionInterval
	@brief An interval action is an action that takes place within a certain period of time.
	It has an start time, and a finish time. The finish time is the parameter
	duration plus the start time.

	These ActionInterval actions have some interesting properties, like:
	- They can run normally (default)
	- They can run reversed with the reverse method
	- They can run with the time altered with the Accelerate, AccelDeccel and Speed actions.

	For example, you can simulate a Ping Pong effect running the action normally and
	then running it again in Reverse mode.

	Example:

	Action *pingPongAction = Sequence::actions(action, action.reverse(), null);
	*/
	public class ActionInterval : FiniteTimeAction
	{
		public ActionInterval (float duration)
		{
			if (duration == 0) {
				_duration = float.Epsilon;
			} else {
				_duration = duration;
			}
		
			_elapsed = 0;
			_firstTick = true;
		}

		public float elapsed {
			/** How many seconds had elapsed since the actions started to run.
		     *
		     * @return The seconds had elapsed since the actions started to run.
		     */
			get {
				return _elapsed;
			}
		}

		public float amplitudeRate {
			/** Sets the amplitude rate, extension in GridAction
		     *
		     * @param amp   The amplitude rate.
		     */
			set {
				Debug.Assert (false, "Subclass should implement this method!");
			}
		
			/** Gets the amplitude rate, extension in GridAction
		     *
		     * @return  The amplitude rate.
		     */
			get {
				Debug.Assert (false, "Subclass should implement this method!");
				return 0;
			}
		}
		//
		// Overrides
		//
		public override bool isDone ()
		{
			return _elapsed >= _duration;
		}

		/**
	     * @param dt in seconds
	     */
		public override void step (float dt)
		{
			if (_firstTick) {
				_firstTick = false;
				//_elapsed = 0;
				_elapsed = dt;
			} else {
				_elapsed += dt;
			}

			float updateDt = Mathf.Max (0, Mathf.Min (1, _elapsed / Mathf.Max (_duration, float.Epsilon)));
            update (updateDt);
        }

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_elapsed = 0;
			_firstTick = true;
		}

		public override Action reverse ()
		{
			Debug.Assert (false);
			return null;
		}

		public override Action clone ()
		{
			Debug.Assert (false);
			return null;
		}

		protected float _elapsed = 0;
		protected bool _firstTick = true;
	}
}
