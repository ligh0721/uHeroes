using UnityEngine;

namespace cca
{
	/** @class Spawn
	 * @brief Spawn a new action immediately
	 */
	public class Spawn : ActionInterval
	{
		
		/** Helper constructor to create an array of spawned actions given an array.
	     *
	     * @param arrayOfActions    An array of spawned actions.
	     * @return  An autoreleased Spawn object.
	     */
		public Spawn (params FiniteTimeAction[] actions)
			: base (0)
		{
			int count = actions.Length;
			if (count == 0) {
				Debug.Assert (false, "actions can't be empty");
				_one = new ExtraAction ();
				_two = new ExtraAction ();
			} else {
				_one = actions [0];
				if (count == 1) {
					_two = new DelayTime (_one.duration);
				} else {
					// else size > 1
					for (int i = 1; i < count - 1; ++i) {
						_one = new Spawn (_one, actions [i]);
					}
					_two = actions [count - 1];

					float delta = _one.duration - _two.duration;
					if (delta > 0) {
						_two = new Sequence (_two, new DelayTime (delta));
					} else if (delta < 0) {
						_one = new Sequence (_one, new DelayTime (-delta));
					}
				}
			}
			_duration = Mathf.Max (_one.duration, _two.duration);
		}
		
		//
		// Overrides
		//
		public override Action clone ()
		{
			return new Sequence (_one.clone () as FiniteTimeAction, _two.clone () as FiniteTimeAction);
		}

		public override Action reverse ()
		{
			return new Spawn (_two.reverse () as FiniteTimeAction, _one.reverse () as FiniteTimeAction);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_one.startWithTarget (target);
			_two.startWithTarget (target);
		}

		public override void stop ()
		{
			_one.stop ();
			_two.stop ();
			base.stop ();
		}

		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			_one.update (time);
			_two.update (time);
		}

		protected FiniteTimeAction _one;
		protected FiniteTimeAction _two;
	}
}
