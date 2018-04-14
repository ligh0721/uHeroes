using UnityEngine;
using System.Collections.Generic;

namespace cca {
    /** @class ActionManager
	 @brief ActionManager is a singleton that manages all the actions.
	 Normally you won't need to use this singleton directly. 99% of the cases you will use the Node interface,
	 which uses this singleton.
	 But there are some cases where you might need to use this singleton.
	 Examples:
	    - When you want to run an action where the target is different from a Node. 
	    - When you want to pause / resume the actions.
	 
	 @since v0.8
	 */
    public class ActionManager : MonoBehaviour {
        void Awake() {
            if (_main == null) {
                _main = this;
            }
        }

        void OnDestroy() {
            if (_main == this) {
                _main = null;
            }
        }

        void Update() {
            update(Time.deltaTime);
        }

        public class TargetActionData {
            protected class ActionData {
                public ActionData(Action action = null) {
                    this.action = action;
                }

                public Action action;
                public bool salvaged = false;
            }

            public TargetActionData(bool paused) {
                this.paused = paused;
            }

            public void addAction(Action action) {
                ActionData data = new ActionData(action);
                if (_delayLock) {
                    _delayToAdd.Add(data);
                } else {
                    _actions.Add(data);
                }
            }

            public bool removeAction(Action action) {
                int index = _actions.FindIndex(data => data.action == action);
                if (index < 0) {
                    return false;
                }
                return removeActionAtIndex(index);
            }

            public bool removeActionAtIndex(int index) {
                ActionData action = _actions[index];
                if (action.salvaged) {
                    return false;
                }
                if (_delayLock) {
                    action.salvaged = true;
                    _delayToRemove.Add(action);
                } else {
                    _actions.RemoveAt(index);
                }
                return _actions.Count == 0;
            }

            public bool removeActionByTag(int tag) {
                if (tag == Action.INVALID_TAG) {
                    return false;
                }
                int index = _actions.FindIndex(data => data.action.tag == tag);
                if (index < 0) {
                    return false;
                }
                return removeActionAtIndex(index);
            }

            public Action getActionByTag(int tag) {
                if (tag == Action.INVALID_TAG) {
                    return null;
                }

                foreach (ActionData data in _actions) {
                    if (data.action.tag == tag) {
                        return data.action;
                    }
                }
                return null;
            }

            public delegate void Function(int index,Action action);

            public bool everyValidAction(Function func) {
                _delayLock = true;
                for (int i = 0; i < _actions.Count; ++i) {
                    ActionData data = _actions[i];
                    if (!data.salvaged) {
                        func(i, data.action);
                    }
                }

                // delay to remove
                for (int i = 0; _delayToRemove.Count > 0 && i < _actions.Count;) {
                    ActionData data = _actions[i];
                    if (_delayToRemove.Contains(data)) {
                        _actions.RemoveAt(i);
                        _delayToRemove.Remove(data);
                    } else {
                        ++i;
                    }
                }
                _delayToRemove.Clear();

                // delay to add
                foreach (ActionData toAdd in _delayToAdd) {
                    _actions.Add(toAdd);
                }
                _delayToAdd.Clear();

                _delayLock = false;

                return _actions.Count == 0;
            }

            protected List<ActionData> _actions = new List<ActionData>();
            public bool salvaged = false;
            public bool paused;

            protected bool _delayLock = false;
            protected HashSet<ActionData> _delayToRemove = new HashSet<ActionData>();
            protected List<ActionData> _delayToAdd = new List<ActionData>();
        }

        public void addAction(Node target, Action action, bool pause = false) {
            action.startWithTarget(target);
            TargetActionData data;
            if (!_targets.TryGetValue(target, out data) || data.salvaged) {
                // not found target or target salvaged. invalid
                if (_delayLock) {
                    if (!_delayToAdd.TryGetValue(target, out data)) {
                        data = new TargetActionData(pause);
                        _delayToAdd.Add(target, data);
                    }
                } else {
                    // not exists and not locked
                    Debug.Assert(data == null || !data.salvaged, "target exists and salvaged, must be locked");
                    data = new TargetActionData(pause);
                    _targets.Add(target, data);
                }
            } else {
                /*
				// exists and not salvaged. valid
				if (_delayLock) {
					if (!_delayToAdd.TryGetValue (target, out data)) {
						data = new TargetActionData (pause);
						_delayToAdd.Add (target, data);
					}
				} else {
					// valid and not locked
				}
                */
            }
            data.addAction(action);
        }

        public void removeAction(Node target, Action action) {
            TargetActionData data;
            if (!_targets.TryGetValue(target, out data) || data.salvaged) {
                // invalid
                if (!_delayToAdd.TryGetValue(target, out data)) {
                    //return;
                } else {
                    if (data.removeAction(action)) {
                        _delayToAdd.Remove(target);
                    } else {
                        //return;
                    }
                }
            } else {
                // valid
                if (data.removeAction(action)) {
                    if (_delayLock) {
                        data.salvaged = true;
                        _delayToRemove.Add(target);
                    } else {
                        _targets.Remove(target);
                    }
                } else {
                    //return;
                }
            }
        }

        protected void removeActionAtIndex(Node target, TargetActionData data, int index) {
            if (data == null || data.salvaged) {
                // invalid
                //return;
#if false  // 这里不能从待添加队里搜寻，index指向的为原data位置
                if (!_delayToAdd.TryGetValue (target, out data)) {
					//return;
				} else {
					if (data.removeActionAtIndex (index)) {
						_delayToAdd.Remove (target);
					} else {
						//return;
					}
				}
#endif
            } else {
                // valid
                if (data.removeActionAtIndex(index)) {
                    if (_delayLock) {
                        data.salvaged = true;
                        _delayToRemove.Add(target);
                    } else {
                        _targets.Remove(target);
                    }
                } else {
                    //return;
                }
            }
        }

        public void removeAllActions(Node target) {
            TargetActionData data;
            if (!_targets.TryGetValue(target, out data) || data.salvaged) {
                // invalid
                if (!_delayToAdd.TryGetValue(target, out data)) {
                    //return;
                } else {
                    _delayToAdd.Remove(target);
                }
            } else {
                // valid
                if (_delayLock) {
                    data.salvaged = true;
                    _delayToRemove.Add(target);
                } else {
                    _targets.Remove(target);
                }
            }
        }

        public Action getActionByTag(Node target, int tag) {
            TargetActionData data;
            if (!_targets.TryGetValue(target, out data) || data.salvaged) {
                // invalid
                if (!_delayToAdd.TryGetValue(target, out data)) {
                    return null;
                } else {
                    return data.getActionByTag(tag);
                }
            } else {
                return data.getActionByTag(tag);
            }
        }

        public void removeActionByTag(Node target, int tag) {
            TargetActionData data;
            if (!_targets.TryGetValue(target, out data) || data.salvaged) {
                // invalid
                if (!_delayToAdd.TryGetValue(target, out data)) {
                    //return;
                } else {
                    if (data.removeActionByTag(tag)) {
                        _delayToAdd.Remove(target);
                    } else {
                        //return;
                    }
                }
            } else {
                // valid
                if (data.removeActionByTag(tag)) {
                    if (_delayLock) {
                        data.salvaged = true;
                        _delayToRemove.Add(target);
                    } else {
                        _targets.Remove(target);
                    }
                } else {
                    //return;
                }
            }
        }

        public delegate void Function(Node target,TargetActionData data,int index,Action action);

        public void everyValidAction(Function func) {
            _delayLock = true;
            //Debug.Log("Lock");
            foreach (KeyValuePair<Node, TargetActionData> kv in _targets) {
                if (!kv.Value.salvaged) {
                    if (kv.Value.everyValidAction(delegate (int index, Action action) {
                            func(kv.Key, kv.Value, index, action);
                        })) {
                        kv.Value.salvaged = true;
                        _delayToRemove.Add(kv.Key);
                    }
                }
            }

            // delay to remove
            foreach (Node toRemove in _delayToRemove) {
                _targets.Remove(toRemove);
            }
            _delayToRemove.Clear();

            _delayLock = false;
            //Debug.LogFormat("Unlock, {0}", _targets.Count);

            // delay to add
            foreach (KeyValuePair<Node, TargetActionData> toAdd in _delayToAdd) {
#if false
                //Debug.Assert(!_targets.ContainsKey(toAdd.Key));
				if (!toAdd.Value.everyValidAction (delegate(int index, Action action) {
					func (toAdd.Key, toAdd.Value, index, action);
				})) {
					_targets.Add (toAdd.Key, toAdd.Value);
				}
#else
                TargetActionData data;
                if (_targets.TryGetValue(toAdd.Key, out data)) {
                    toAdd.Value.everyValidAction(delegate (int index, Action action) {
                            data.addAction(action);
                        });
                } else {
                    _targets.Add(toAdd.Key, toAdd.Value);
                }
#endif
            }
            _delayToAdd.Clear();
        }

        public void update(float dt) {
            everyValidAction(delegate (Node target, TargetActionData data, int index, Action action) {
                    if (!data.paused) {
                        action.step(dt);
                        if (action.isDone()) {
                            action.stop();
                            removeActionAtIndex(target, data, index);
                        }
                    }
                });
            //System.GC.Collect ();
            //Debug.LogFormat("targets: {0}.", _targets.Count);
        }

        protected static ActionManager _main = null;

        public static ActionManager Main {
            get { return _main; }
        }

        protected Dictionary<Node, TargetActionData> _targets = new Dictionary<Node, TargetActionData>();

        protected bool _delayLock = false;
        protected List<Node> _delayToRemove = new List<Node>();
        protected Dictionary<Node, TargetActionData> _delayToAdd = new Dictionary<Node, TargetActionData>();
    }
}
