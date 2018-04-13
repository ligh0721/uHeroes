using UnityEngine;

namespace cca
{
	/** @class RemoveSelf
	* @brief Remove the node.
	*/
	public class RemoveSelf : ActionInstant
	{
		/** Create the action.
	     *
	     * @param isNeedCleanUp Is need to clean up, the default value is true.
	     * @return An autoreleased RemoveSelf object.
	     */
		public RemoveSelf (bool isNeedCleanUp = true)
		{
			_isNeedCleanUp = isNeedCleanUp;
		}

		//
		// Override
		//
		/**
	     * @param time In seconds.
	     */
		public override void update (float time)
		{
			if (_target) {
				_target.removeFromParentAndCleanup();
			}
		}

		public override Action clone ()
		{
			return new RemoveSelf (_isNeedCleanUp);
		}

		public override Action reverse ()
		{
			return new RemoveSelf (_isNeedCleanUp);
		}

		protected bool _isNeedCleanUp = true;
	}
}
