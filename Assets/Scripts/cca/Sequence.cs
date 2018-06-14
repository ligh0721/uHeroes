using UnityEngine;

namespace cca
{
	/** @class Sequence
	 * @brief Runs actions sequentially, one after another.
	 */
	public class Sequence : ActionInterval
	{
		/** Helper constructor to create an array of sequenceable actions.
	     *
	     * @return An autoreleased Sequence object.
	     */
		public Sequence (params FiniteTimeAction[] actions)
			: base (0)
		{
			int count = actions.Length;
			if (count == 0) {
				Debug.Assert (false, "actions can't be empty");
				_actions [0] = new ExtraAction ();
				_actions [1] = new ExtraAction ();
			} else {
				_actions [0] = actions [0];
				if (count == 1) {
					_actions [1] = new ExtraAction ();
				} else {
					// else size > 1
					for (int i = 1; i < count - 1; ++i) {
						_actions [0] = new Sequence (_actions [0], actions [i]);
					}
					_actions [1] = actions [count - 1];
				}
			}
			_duration = _actions [0].duration + _actions [1].duration;
		}

		public FiniteTimeAction ActionOne {
			get {
				return _actions [0];
			}
		}

		public FiniteTimeAction ActionTwo {
			get {
				return _actions [1];
			}
		}

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new Sequence (_actions [0].clone () as FiniteTimeAction, _actions [1].clone () as FiniteTimeAction);
		}

		public override Action reverse ()
		{
			return new Sequence (_actions [1].reverse () as FiniteTimeAction, _actions [0].reverse () as FiniteTimeAction);
		}

		public override void startWithTarget (Node target)
		{
            if (_duration > float.Epsilon) {
                _split = _actions[0].duration > float.Epsilon ? (_actions[0].duration / _duration) : 0;
            }
            base.startWithTarget (target);
			_last = -1;
		}

		public override void stop ()
		{
			if (_last != -1 && _actions[_last] != null) {
				_actions [_last].stop ();
			}
			base.stop ();
		}

		/**
	     * @param t In seconds.
	     */
		public override void update (float time)
		{
			int found = 0;
			float new_t = 0;

			if (time < _split) {
				// action[0]
				found = 0;
				if (_split != 0) {
					new_t = time / _split;
				} else {
					new_t = 1;
				}
			} else {
				// action[1]
				found = 1;
				if (_split == 1) {
					new_t = 1;
				} else {
					new_t = (time - _split) / (1 - _split);
				}
			}

			if (found == 1) {

				if (_last == -1) {
					// action[0] was skipped, execute it.
					_actions [0].startWithTarget (_target);
					_actions [0].update (1.0f);
					_actions [0].stop ();
				} else if (_last == 0) {
					// switching to action 1. stop action 0.
					_actions [0].update (1.0f);
					_actions [0].stop ();
				}
			} else if (found == 0 && _last == 1) {
				// Reverse mode ?
				// FIXME: Bug. this case doesn't contemplate when _last==-1, found=0 and in "reverse mode"
				// since it will require a hack to know if an action is on reverse mode or not.
				// "step" should be overridden, and the "reverseMode" value propagated to inner Sequences.
				_actions [1].update (0);
				_actions [1].stop ();
			}
			// Last action found and it is done.
			if (found == _last && _actions [found].isDone ()) {
				return;
			}

			// Last action found and it is done
			if (found != _last) {
				_actions [found].startWithTarget (_target);
			}

			_actions [found].update (new_t);
			_last = found;
		}

		protected FiniteTimeAction[] _actions = new FiniteTimeAction[2];
		protected float _split;
		protected int _last;
	}
}
