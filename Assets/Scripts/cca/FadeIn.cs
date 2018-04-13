using UnityEngine;

namespace cca
{
	/** @class FadeIn
	 * @brief Fades In an object that implements the RGBAProtocol protocol. It modifies the opacity from 0 to 255.
	 The "reverse" of this action is FadeOut
	 */
	public class FadeIn : FadeTo
	{
		/** 
	     * Creates the action.
	     * @param d duration time, in seconds.
	     * @return An autoreleased FadeIn object.
	     */
		public FadeIn (float duration)
			: base (duration, 1.0f)
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
				_toOpacity = 1.0f;
			}

			if (target) {
				_fromOpacity = target.opacity;
			}
		}

		public override Action clone ()
		{
			return new FadeIn (_duration);
		}

		public override Action reverse ()
		{
			FadeOut action = new FadeOut (_duration);
			if (_target) {
				action._reverseAction = this;
			}
			return action;
		}

		protected internal FadeTo _reverseAction;
	}
}
