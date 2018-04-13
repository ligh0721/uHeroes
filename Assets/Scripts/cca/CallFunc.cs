using UnityEngine;

namespace cca
{
    /** @class CallFunc
	* @brief Calls a 'callback'.
	*/

    public delegate void Function();

    public class CallFunc : ActionInstant
	{
		/** Creates the action with the callback of type std::function<void()>.
	     This is the preferred way to create the callback.
	     * When this function bound in js or lua ,the input param will be changed.
	     * In js: var create(var func, var this, var [data]) or var create(var func).
	     * In lua:local create(local funcID).
	     *
	     * @param func  A callback function need to be executed.
	     * @return  An autoreleased CallFunc object.
	     */
		public CallFunc (Function func)
		{
			_function = func;
		}

		/** Executes the callback.
     	*/
		public virtual void execute ()
		{
			if (_function != null) {
				_function ();
			}
		}

		//
		// Overrides
		//
		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			execute ();
		}

		public override Action reverse ()
		{
			return clone ();
		}

		public override Action clone ()
		{
			return new CallFunc (_function);
		}

		/** function that will be called */
		protected Function _function;
	}
}
