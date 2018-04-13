using UnityEngine;

namespace cca
{
	/** @class TargetedAction
	 * @brief Overrides the target of an action so that it always runs on the target
	 * specified at action creation rather than the one specified by runAction.
	 */
	public class TargetedAction : ActionInterval
	{
		/** Create an action with the specified action and forced target.
	     * 
	     * @param target The target needs to override.
	     * @param action The action needs to override.
	     * @return An autoreleased TargetedAction object.
	     */
		public TargetedAction (GameObject target, FiniteTimeAction action)
			: base (action.duration)
		{
			_forcedTarget = target;
			_action = action;
		}

		public GameObject ForcedTarget {
			/** Sets the target that the action will be forced to run with.
		     *
		     * @param forcedTarget The target that the action will be forced to run with.
		     */
			set {
				_forcedTarget = value;
			}
			/** returns the target that the action is forced to run with. 
		     *
		     * @return The target that the action is forced to run with.
		     */
			get {
				return _forcedTarget;
			}
		}

		//
		// Overrides
		//
		public override Action clone ()
		{
			return new TargetedAction (_forcedTarget, _action.clone () as FiniteTimeAction);
		}

		public override Action reverse ()
		{
			return new TargetedAction (_forcedTarget, _action.reverse () as FiniteTimeAction);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_action.startWithTarget (target);
		}

		public override void stop ()
		{
			_action.stop ();
		}

		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			_action.update (time);
		}
		//
		// Overrides
		//
		public override bool isDone ()
		{
			return _action.isDone ();
		}

		protected FiniteTimeAction _action;
		protected GameObject _forcedTarget;
	}
}
