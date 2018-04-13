using UnityEngine;

namespace cca {
    /** 
	 * @brief Base class for Action objects.
	 */
    public class Action {
        /** Default tag used for all the actions. */
        public const int INVALID_TAG = -1;

        public Action() {
            ++_refcount;
        }

        ~Action () {
            --_refcount;
            //Debug.LogFormat ("deallocing Action: {0} - tag: {1}, refCount: {2}.", this.GetHashCode (), _tag, _refcount);
        }

        /**
	     * @js NA
	     * @lua NA
	     */
        public virtual string description {
            get { return string.Format("<Action | tag = {0}", _tag); }
        }

        /** Returns a clone of action.
	     *
	     * @return A clone action.
	     */
        public virtual Action clone() {
            Debug.Assert(false);
            return null;
        }

        /** Returns a new action that performs the exactly the reverse action. 
	     *
	     * @return A new action that performs the exactly the reverse action.
	     * @js NA
	     */
        public virtual Action reverse() {
            Debug.Assert(false);
            return null;
        }

        /** Return true if the action has onFinished. 
	     * 
	     * @return Is true if the action has onFinished.
	     */
        public virtual bool isDone() {
            return true;
        }

        /** Called before the action start. It will also set the target. 
	     *
	     * @param target A certain target.
	     */
        public virtual void startWithTarget(Node target) {
            _originalTarget = _target = target;
        }

        /** 
	     * Called after the action has onFinished. It will set the 'target' to nil.
	     * IMPORTANT: You should never call "Action::stop()" manually. Instead, use: "target.stopAction(action);".
	     */
        public virtual void stop() {
            _target = null;
        }

        /** Called every frame with it's delta time, dt in seconds. DON'T override unless you know what you are doing. 
	     *
	     * @param dt In seconds.
	     */
        public virtual void step(float dt) {
            Debug.LogFormat("[Action step]. override me.");
        }

        /** 
	     * Called once per frame. time a value between 0 and 1.

	     * For example:
	     * - 0 Means that the action just started.
	     * - 0.5 Means that the action is in the middle.
	     * - 1 Means that the action is over.
	     *
	     * @param time A value between 0 and 1.
	     */
        public virtual void update(float time) {
            Debug.LogFormat("[Action update]. override me.");
        }

        public Node Target {
            /** Return certain target.
			 *
			 * @return A certain target.
			 */
			get { return _target; }

            /** The action will modify the target properties. 
		     *
		     * @param target A certain target.
		     */
			set { _target = value; }
        }

        public Node originalTarget {
            /** Return a original Target. 
		     *
		     * @return A original Target.
		     */
			get { return _originalTarget; }

            /** 
		     * Set the original target, since target can be nil.
		     * Is the target that were used to run the action. Unless you are doing something complex, like ActionManager, you should NOT call this method.
		     * The target is 'assigned', it is not 'retained'.
		     * @since v0.8.2
		     *
		     * @param originalTarget Is 'assigned', it is not 'retained'.
		     */
			set { _originalTarget = value; }
        }

        public int tag {
            /** Returns a tag that is used to identify the action easily. 
		     *
		     * @return A tag.
		     */
			get { return _tag; }

            /** Changes the tag that is used to identify the action easily. 
		     *
		     * @param tag Used to identify the action easily.
		     */
			set { _tag = value; }
        }

        public uint flags {
            /** Returns a flag field that is used to group the actions easily.
		     *
		     * @return A tag.
		     */
			get { return _flags; }

            /** Changes the flag field that is used to group the actions easily.
		     *
		     * @param tag Used to identify the action easily.
		     */
			set { _flags = value; }
        }

        protected Node _originalTarget = null;
        /** 
	     * The "target".
	     * The target will be set with the 'startWithTarget' method.
	     * When the 'stop' method is called, target will be set to nil.
	     * The target is 'assigned', it is not 'retained'.
	     */
        protected Node _target = null;
        /** The action tag. An identifier of the action. */
        protected int _tag = Action.INVALID_TAG;
        /** The action flag field. To categorize action into certain groups.*/
        protected uint _flags = 0;

        public static int refCount {
            get { return _refcount; }
        }

        private static int _refcount = 0;
    }
}
