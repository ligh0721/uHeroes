using UnityEngine;

namespace cca
{
	/** @class FiniteTimeAction
	 * @brief
	 * Base class actions that do have a finite time duration.
	 * Possible actions:
	 * - An action with a duration of 0 seconds.
	 * - An action with a duration of 35.5 seconds.
	 * Infinite time actions are valid.
	 */
	public class FiniteTimeAction : Action
	{
		public float duration {
			/** Get duration in seconds of the action. 
		     *
		     * @return The duration in seconds of the action.
		     */
			get {
				return _duration;
			}

			/** Set duration in seconds of the action. 
		     *
		     * @param duration In seconds of the action.
		     */
			set {
				_duration = value;
			}
		}

		//
		// Overrides
		//
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

		//! duration in seconds.
		protected float _duration = 0;
	}
}
