using UnityEngine;


namespace cca
{
	/** @class animation
	 * A animation object is used to perform animations on the Sprite objects.
	 * The animation object contains AnimationFrame objects, and a possible delay between the frames.
	 * You can animate a animation object by using the Animate action. Example:
	 * @code
	 * sprite.runAction(new Animate(animation));
	 * @endcode
	*/
	public class Animation
	{
		/* Creates an animation with an array of SpriteFrame and a delay between frames in seconds.
		* The frames will be added with one "delay unit".
		* @since v0.99.5
		* @param arrayOfSpriteFrameNames An array of SpriteFrame.
		* @param delay A delay between frames in seconds.
		* @param loops The times the animation is going to loop.
		*/
		public Animation (Sprite[] frames, float delayPerUnit, uint loops = 1)
		{
			_frames = frames.Clone () as Sprite[];
			_delayPerUnit = delayPerUnit;
			_loops = loops;
			_restoreOriginalFrame = false;
			_framesData = new object[_frames.Length];
		}

		public virtual Animation clone ()
		{
			Animation a = new Animation (_frames, _delayPerUnit, _loops);
			a.RestoreOriginalFrame = _restoreOriginalFrame;
			a._framesData = _framesData.Clone () as object[];
			return a;
		}

		public void setFrameData (int index, object data)
		{
			Debug.Assert (index >= 0 && index < _framesData.Length);
			_framesData [index] = data;
		}

		public object getFrameData (int index)
		{
			Debug.Assert (index >= 0 && index < _framesData.Length);
			return _framesData [index];
		}

		public Sprite[] Frames {
			/** Sets the array of Sprites. 
		     *
		     * @param frames The array of Sprites.
		     */
			set {
				_frames = value.Clone () as Sprite[];
			}

			/** Gets the array of Sprites.
		     * 
		     * @return The array of Sprites.
		     */
			get {
				return _frames;
			}
		}

		public float DelayPerUnit {
			/** Sets the delay in seconds of the "delay unit".
			*
			* @param delayPerUnit The delay in seconds of the "delay unit".
			*/
			set {
				_delayPerUnit = value;
			}

			/** Gets the delay in seconds of the "delay unit".
		     * 
		     * @return The delay in seconds of the "delay unit".
		     */
			get {
				return _delayPerUnit;
			}
		}

		public float duration {
			/** Gets the duration in seconds of the whole animation. It is the result of totalDelayUnits * delayPerUnit.
		     *
		     * @return Result of totalDelayUnits * delayPerUnit.
		     */
			get {
				return _frames.Length * _delayPerUnit;
			}
		}

		public bool RestoreOriginalFrame {
			/** Sets whether to restore the original frame when animation finishes. 
		     *
		     * @param restoreOriginalFrame Whether to restore the original frame when animation finishes.
		     */
			set {
				_restoreOriginalFrame = value;
			}

			/** Checks whether to restore the original frame when animation finishes. 
		     *
		     * @return Restore the original frame when animation finishes.
		     */
			get {
				return _restoreOriginalFrame;
			}
		}

		public uint Loops {
			/** Gets the times the animation is going to loop. 0 means animation is not animated. 1, animation is executed one time, ... 
		     *
		     * @return The times the animation is going to loop.
		     */
			get {
				return _loops;
			}

			/** Sets the times the animation is going to loop. 0 means animation is not animated. 1, animation is executed one time, ... 
		     *
		     * @param loops The times the animation is going to loop.
		     */
			set {
				_loops = value;
			}
		}

		protected Sprite[] _frames;
		protected float _delayPerUnit;
		protected uint _loops;
		protected bool _restoreOriginalFrame;
		protected internal object[] _framesData;
	}

	/** @class Animate
	 * @brief Animates a sprite given the name of an animation.
	 */
	public class Animate : ActionInterval
	{
		public delegate void Function (int index,ref object data);

		/** Creates the action with an animation and will restore the original frame when the animation is over.
	     *
	     * @param animation A certain animation.
	     * @return An autoreleased Animate object.
	     */
		public Animate (Animation animation, Function onSpecial = null)
			: base (animation.duration * animation.Loops)
		{
			Debug.Assert (animation != null, "Animate: argument animation must be non-null");
			_onSpecial = onSpecial;
			_nextFrame = 0;
			_animation = animation;
			_origFrame = null;
			_executedLoops = 0;

			_splitTimes = new float[animation.Frames.Length];

			float accumUnitsOfTime = 0;
			float newUnitOfTimeValue = animation.duration / animation.Frames.Length;

			for (int i = 0; i < animation.Frames.Length; ++i) {
				float value = (accumUnitsOfTime * newUnitOfTimeValue) / animation.duration;
				accumUnitsOfTime += 1.0f;
				_splitTimes [i] = value;
			}
		}

		public Animation animation {
			/** Sets the animation object to be animated 
		     * 
		     * @param animation certain animation.
		     */
			set {
				_animation = value;
			}
			/** returns the animation object that is being animated 
		     *
		     * @return Gets the animation object that is being animated.
		     */
			get {
				return _animation;
			}
		}

		public Function frameFunction {
			get {
				return _onSpecial;
			}

			set {
				_onSpecial = value;
			}
		}

		public int currentFrameIndex {
			/**
		     * Gets the index of sprite frame currently displayed.
		     * @return int  the index of sprite frame currently displayed.
		     */
			get {
				return _currFrameIndex;
			}
		}
		//
		// Overrides
		//
		public override Action clone ()
		{
			return new Animate (_animation.clone (), _onSpecial);
		}

		public override Action reverse ()
		{
			Sprite[] frames = _animation.Frames.Clone() as Sprite[];
			System.Array.Reverse (frames);

			Animation newAnim = new Animation (frames, _animation.DelayPerUnit);
			newAnim.RestoreOriginalFrame = _animation.RestoreOriginalFrame;
			return new Animate (newAnim, _onSpecial);
		}

		public override void startWithTarget (Node target)
		{
			base.startWithTarget (target);

			if (_animation.RestoreOriginalFrame) {
				_origFrame = target.frame;
			}

			_nextFrame = 0;
			_executedLoops = 0;
		}

		public override void stop ()
		{
			if (_animation.RestoreOriginalFrame && _target) {
				_target.frame = _origFrame;
			}

			base.stop ();
		}

		/**
	     * @param t In seconds.
	     */
		public override void update (float time)
		{
			// if t==1, ignore. animation should finish with t==1
			if (time < 1.0f) {
				time *= _animation.Loops;

				// new loop?  If so, reset frame counter
				uint loopNumber = (uint)time;
				if (loopNumber > _executedLoops) {
					_nextFrame = 0;
					++_executedLoops;
				}

				// new t for animations
				time = time % 1.0f;
			}

			Sprite[] frames = _animation.Frames;
			int numberOfFrames = frames.Length;
			Sprite frameToDisplay = null;

			for (int i = _nextFrame; i < numberOfFrames; ++i) {
				float splitTime = _splitTimes [i];
				if (splitTime <= time) {
					_currFrameIndex = i;
					frameToDisplay = frames [_currFrameIndex];
					_target.frame = frameToDisplay;

					if (_onSpecial != null) {
						object data = _animation.getFrameData(_currFrameIndex);
						if (data != null) {
							_onSpecial (_currFrameIndex, ref _animation._framesData[_currFrameIndex]);
						}
					}
					_nextFrame = i + 1;
				} else {
					// Issue 1438. Could be more than one frame per tick, due to low frame rate or frame delta < 1/FPS
					break;
				}
			}
		}

		protected float[] _splitTimes;
		protected int _nextFrame;
		protected Sprite _origFrame;
		protected int _currFrameIndex;
		protected uint _executedLoops;
		protected Animation _animation;
		protected Function _onSpecial;

		//EventCustom* _frameDisplayedEvent;
		//AnimationFrame::DisplayedEventInfo _frameDisplayedEventInfo;
	}
}
