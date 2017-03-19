using UnityEngine;

namespace cca
{
	/** @class FadeOut
	 * @brief Fades Out an object that implements the RGBAProtocol protocol. It modifies the opacity from 255 to 0.
	 The "reverse" of this action is FadeIn
	*/
	public class FadeOut : FadeTo
	{
		/** 
	     * Creates the action.
	     * @param d duration time, in seconds.
	     */
		public FadeOut (float duration)
			: base (duration, 0)
		{
		}

		//
		// Overrides
		//
		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);

			if (_reverseAction != null) {
				_toOpacity = _reverseAction._fromOpacity;
			} else {
				_toOpacity = 0;
			}

			if (target) {
				_fromOpacity = target.opacity;
			}
		}

		public override Action clone ()
		{
			return new FadeOut (_duration);
		}

		public override Action reverse ()
		{
			FadeIn action = new FadeIn (_duration);
			if (_target) {
				action._reverseAction = this;
			}
			return action;
		}

		protected internal FadeTo _reverseAction;
	}
}
