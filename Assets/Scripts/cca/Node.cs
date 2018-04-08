using UnityEngine;
using System.Collections.Generic;


namespace cca
{
    public class Node : MonoBehaviour
    {
        public GameObject m_prefab;  // use to alloc object to pool
        public ActionManager m_actionManager;

        SpriteRenderer _spriteRenderer;
        void Start() {
            init();
        }

        // detach and destroy gameObject
        void OnDestroy()
        {
            cleanup();
        }

        public virtual void init() {
            if (m_actionManager == null) {
                Debug.LogWarning("Action Manager is not set");
                m_actionManager = ActionManager.Main;
            }
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public virtual void cleanup() {
            m_actionManager.removeAllActions(this);
        }

        public virtual float positionZ
        {
            get
            {
                return transform.localPosition.z;
            }

            set
            {
                Vector3 pos = transform.localPosition;
                pos.z = value;
                transform.localPosition = pos;
            }
        }

        public virtual float worldPositionZ
        {
            get
            {
                return transform.position.z;
            }

            set
            {
                Vector3 pos = transform.position;
                pos.z = value;
                transform.position = pos;
            }
        }

        public virtual Vector2 position
        {
            get
            {
                return transform.localPosition;
            }

            set
            {
                Vector3 pos = transform.localPosition;
                pos.x = value.x;
                pos.y = value.y;
                transform.localPosition = pos;
            }
        }

        public virtual Vector2 worldPosition
        {
            get
            {
                return transform.position;
            }

            set
            {
                Vector3 pos = transform.position;
                pos.x = value.x;
                pos.y = value.y;
                transform.position = pos;
            }
        }

        public virtual float rotation
        {
            get
            {
                return transform.localRotation.eulerAngles.z;
            }

            set
            {
                Vector3 rotation = transform.localRotation.eulerAngles;
                rotation.z = value;
                transform.localRotation = Quaternion.Euler(rotation);
            }
        }

        public virtual Vector2 boundsSize
        {
            get
            {
                return _spriteRenderer.sprite.bounds.size;
            }
        }

        public virtual Vector2 size
        {
            get
            {
                Sprite sprite = _spriteRenderer.sprite;
                return sprite.rect.size / sprite.pixelsPerUnit;
            }
        }

        public virtual Vector2 sizePixel
        {
            get
            {
                return _spriteRenderer.sprite.rect.size;
            }
        }

        public virtual Vector2 scale
        {
            get
            {
                return transform.localScale;
            }

            set
            {
                Vector3 scale = transform.localScale;
                scale.x = value.x;
                scale.y = value.y;
                transform.localScale = scale;
            }
        }

        public bool flippedX
        {
            get
            {
                return _spriteRenderer.flipX;
            }

            set
            {
                _spriteRenderer.flipX = value;
            }
        }

        public Sprite frame
        {
            get
            {
                return _spriteRenderer.sprite;
            }

            set
            {
                _spriteRenderer.sprite = value;
            }
        }

		public bool visible
		{
			get {
				return _spriteRenderer.enabled;
			}

			set {
                _spriteRenderer.enabled = value;
			}
		}

        public float opacity
        {
            get
            {
                return _spriteRenderer.color.a;
            }

            set
            {
                Color color = _spriteRenderer.color;
                color.a = value;
                _spriteRenderer.color = color;
            }
        }

        public Node parent
        {
            get
            {
                if (transform.parent == null)
                {
                    return null;
                }

                Node parentNode = transform.parent.GetComponent<Node>();
                return parentNode;
            }

            set
            {
                if (value != null) {
                    transform.SetParent(value.transform);
                } else {
                    transform.SetParent(null);
                }
            }
        }

        public void removeFromParentAndCleanup()
        {
            //_gameObject.transform.SetParent(null);
            //this.destroy();
            parent = null;
            GameObjectPool.instance.Destroy(m_prefab, gameObject);
        }

        public void runAction(Action action)
        {
            m_actionManager.addAction(this, action);
        }

        public cca.Action getActionByTag(int tag)
        {
            return m_actionManager.getActionByTag(this, tag);
        }

        public void stopActionByTag(int tag)
        {
            m_actionManager.removeActionByTag(this, tag);
        }

        public void stopAllActions()
        {
            m_actionManager.removeAllActions(this);
        }
    }
}
