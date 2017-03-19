using UnityEngine;

namespace cca
{
	/** @class ActionInstant
	* @brief Instant actions are immediate actions. They don't have a duration like the IntervalAction actions.
	**/
	public class ActionInstant : FiniteTimeAction
	{
		//
		// Overrides
		//
		public override Action clone ()
		{
			Debug.Assert (false);
			return null;
		}

		public override Action reverse ()
		{
			Debug.Assert (false);
			return null;
		}

		public override bool isDone ()
		{
			return true;
		}

		/**
	     * @param dt In seconds.
	     */
		public override void step (float dt)
		{
			update (1.0f);
		}

		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
		}
	}
}
