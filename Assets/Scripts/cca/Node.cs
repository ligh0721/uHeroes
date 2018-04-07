using UnityEngine;
using System.Collections.Generic;


namespace cca
{
    public class Node
    {
        public Node()
        {
        }

        public Node(GameObject prefab, GameObject gameObject)
        {
            init(prefab, gameObject);
        }

        public virtual void init(GameObject prefab, GameObject gameObject)
        {
            Debug.Assert(!_nodeMapper.ContainsKey(gameObject));
            _nodeMapper.Add(gameObject, this);
            _prefab = prefab;
            _gameObject = gameObject;
        }

        public GameObject gameObject
        {
            get
            {
                return _gameObject;
            }
        }

        public bool valid
        {
            get
            {
                return _gameObject != null;
            }
        }

        public static Node mapped(GameObject prefab, GameObject gameObject)
        {
            Node node;
            if (!_nodeMapper.TryGetValue(gameObject, out node))
            {
                node = new Node(prefab, gameObject);
                //node = ObjectPool<Node>.instance.Instantiate(); node.init(prefab, gameObject);
            }
            return node;
        }

        // detach and destroy gameObject
        public virtual void destroy()
        {
            cleanup();
            _nodeMapper.Remove(_gameObject);
            GameObjectPool.instance.Destroy(_prefab, _gameObject);
            _gameObject = null;
        }

        public static void destroyAll()
        {
            var copy = new Dictionary<GameObject, Node>(_nodeMapper);
            foreach (Node node in copy.Values)
            {
                node.destroy();
            }
        }

        public static implicit operator bool(Node node)
        {
            return node != null && node.valid;
        }

        public virtual float positionZ
        {
            get
            {
                return _gameObject.transform.localPosition.z;
            }

            set
            {
                Vector3 pos = _gameObject.transform.localPosition;
                pos.z = value;
                _gameObject.transform.localPosition = pos;
            }
        }

        public virtual float worldPositionZ
        {
            get
            {
                return _gameObject.transform.position.z;
            }

            set
            {
                Vector3 pos = _gameObject.transform.position;
                pos.z = value;
                _gameObject.transform.position = pos;
            }
        }

        public virtual Vector2 position
        {
            get
            {
                return _gameObject.transform.localPosition;
            }

            set
            {
                Vector3 pos = _gameObject.transform.localPosition;
                pos.x = value.x;
                pos.y = value.y;
                _gameObject.transform.localPosition = pos;
            }
        }

        public virtual Vector2 worldPosition
        {
            get
            {
                return _gameObject.transform.position;
            }

            set
            {
                Vector3 pos = _gameObject.transform.position;
                pos.x = value.x;
                pos.y = value.y;
                _gameObject.transform.position = pos;
            }
        }

        public virtual float rotation
        {
            get
            {
                return _gameObject.transform.localRotation.eulerAngles.z;
            }

            set
            {
                Vector3 rotation = _gameObject.transform.localRotation.eulerAngles;
                rotation.z = value;
                _gameObject.transform.localRotation = Quaternion.Euler(rotation);
            }
        }

        public virtual Vector2 boundsSize
        {
            get
            {
                return _gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size;
            }
        }

        public virtual Vector2 size
        {
            get
            {
                Sprite sprite = _gameObject.GetComponent<SpriteRenderer>().sprite;
                return sprite.rect.size / sprite.pixelsPerUnit;
            }
        }

        public virtual Vector2 sizePixel
        {
            get
            {
                return _gameObject.GetComponent<SpriteRenderer>().sprite.rect.size;
            }
        }

        public virtual Vector2 scale
        {
            get
            {
                return _gameObject.transform.localScale;
            }

            set
            {
                Vector3 scale = _gameObject.transform.localScale;
                scale.x = value.x;
                scale.y = value.y;
                _gameObject.transform.localScale = scale;
            }
        }

        public bool flippedX
        {
            get
            {
                return _gameObject.GetComponent<SpriteRenderer>().flipX;
            }

            set
            {
                _gameObject.GetComponent<SpriteRenderer>().flipX = value;
            }
        }

        public Sprite frame
        {
            get
            {
                return _gameObject.GetComponent<SpriteRenderer>().sprite;
            }

            set
            {
                _gameObject.GetComponent<SpriteRenderer>().sprite = value;
            }
        }

		public bool visible
		{
			get {
				return _gameObject.GetComponent<SpriteRenderer> ().enabled;
			}

			set {
				_gameObject.GetComponent<SpriteRenderer> ().enabled = value;
			}
		}

        public float opacity
        {
            get
            {
                return _gameObject.GetComponent<SpriteRenderer>().color.a;
            }

            set
            {
                Color color = _gameObject.GetComponent<SpriteRenderer>().color;
                color.a = value;
                _gameObject.GetComponent<SpriteRenderer>().color = color;
            }
        }

        public Node parent
        {
            get
            {
                if (_gameObject.transform.parent == null)
                {
                    return null;
                }

                Node node;
                if (_nodeMapper.TryGetValue(_gameObject.transform.parent.gameObject, out node))
                {
                    return node;
                }
                else
                {
                    Debug.LogWarning("parent is not assigned to a node");
                    //node = ObjectPool<Node>.instance.Instantiate(); node.init(_prefab, _gameObject.transform.parent.gameObject);
                    //return node;
                    return new Node(_prefab, _gameObject.transform.parent.gameObject);
                }
            }

            set
            {
                _gameObject.transform.SetParent(value.gameObject.transform);
            }
        }

        public void removeFromParentAndCleanup()
        {
            Debug.Assert(_gameObject);
            //_gameObject.transform.SetParent(null);
            this.destroy();
        }

        public void cleanup()
        {
            Debug.Assert(_gameObject);
            _actionManager.removeAllActions(this);
        }

        public void runAction(Action action)
        {
            Debug.Assert(_gameObject);
            _actionManager.addAction(this, action);
        }

        public cca.Action getActionByTag(int tag)
        {
            Debug.Assert(_gameObject);
            return _actionManager.getActionByTag(this, tag);
        }

        public void stopActionByTag(int tag)
        {
            Debug.Assert(_gameObject);
            _actionManager.removeActionByTag(this, tag);
        }

        public void stopAllActions()
        {
            Debug.Assert(_gameObject);
            _actionManager.removeAllActions(this);
        }

        protected GameObject _gameObject;
        protected GameObject _prefab;
        protected static Dictionary<GameObject, Node> _nodeMapper = new Dictionary<GameObject, Node>();
        protected ActionManager _actionManager = ActionManager.instance;
    }
}
