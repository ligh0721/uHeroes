using UnityEngine;

namespace cca
{
	// Extra action for making a Sequence or Spawn when only adding one action to it.
	public class ExtraAction : FiniteTimeAction
	{
		public override Action clone ()
		{
			return new ExtraAction ();
		}

		public override Action reverse ()
		{
			return new ExtraAction ();
		}

		public override void update (float time)
		{
		}

		public override void step (float dt)
		{
		}
	}
}
