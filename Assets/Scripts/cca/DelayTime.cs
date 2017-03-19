using UnityEngine;

namespace cca
{
	/** @class DelayTime
	 * @brief Delays the action a certain amount of seconds.
	*/
	public class DelayTime : ActionInterval
	{
		/** 
	     * Creates the action.
	     * @param d duration time, in seconds.
	     * @return An autoreleased DelayTime object.
	     */
		public DelayTime (float duration)
			: base (duration)
		{
		}

		//
		// Overrides
		//
		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
		}

		public override Action reverse ()
		{
			return new DelayTime (_duration);
		}

		public override Action clone ()
		{
			return new DelayTime (_duration);
		}
	}
}
