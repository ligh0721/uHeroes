using UnityEngine;

namespace cca
{
	/** 
	 @class ActionEase
	 @brief Base class for Easing actions.
	 @details Ease actions are created from other interval actions.
	         The ease action will change the timeline of the inner action.
	 @ingroup Actions
	 */
	public class ActionEase : ActionInterval
	{
		public ActionEase (ActionInterval action)
			: base (action.duration)
		{
			_inner = action;
		}

		public ActionInterval innerAction {
			/**
		    @brief Get the pointer of the inner action.
		    @return The pointer of the inner action.
		    */
			get {
				return _inner;
			}
		}

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

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);
			_inner.startWithTarget (target);
		}

		public override void stop ()
		{
			_inner.stop ();
			base.stop ();
		}

		public override void update (float time)
		{
			_inner.update (time);
		}

		/** 
	     @brief Initializes the action.
	     @return Return true when the initialization success, otherwise return false.
	    */
		/** The inner action */
		protected ActionInterval _inner;
	}
}
