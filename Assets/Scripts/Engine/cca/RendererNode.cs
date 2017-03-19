using UnityEngine;
using System.Collections;
using cca;

public class RendererNode : Node {
    public RendererNode()
    {
    }

    public RendererNode(GameObject prefab, GameObject gameObject)
        : base(prefab, gameObject)
    {
        //height = 0;  // it is used to update z
    }

    public override void init(GameObject prefab, GameObject gameObject)
    {
        base.init(prefab, gameObject);
        height = 0;
    }

    public const float CONST_BASE_Z = -100.0f;

    public static new RendererNode mapped(GameObject prefab, GameObject gameObject)
    {
        Node node;
        if (!_nodeMapper.TryGetValue(gameObject, out node))
        {
            node = new RendererNode(prefab, gameObject);
            //node = ObjectPool<RendererNode>.instance.Instantiate(); node.init(prefab, gameObject);
        }
        RendererNode ret = node as RendererNode;
        Debug.Assert(ret != null);
        return ret;
    }

    public override void destroy()
    {
        base.destroy();
        //ObjectPool<RendererNode>.instance.Destroy(this);
    }

    public float height
    {
        get
        {
            return _height;
        }

        set
        {
            Vector3 pos = _gameObject.transform.localPosition;
            pos.y -= height;
            pos.z = CONST_BASE_Z + pos.y;
            pos.y += value;
            _height = value;

            _gameObject.transform.localPosition = pos;
        }
    }

    public override Vector2 position
    {
        get
        {
            Vector2 pos = _gameObject.transform.localPosition;
            pos.y -= _height;
            return pos;
        }

        set
        {
            _gameObject.transform.localPosition = new Vector3(value.x, value.y + _height, CONST_BASE_Z + value.y);
        }
    }

    public ObjectRenderer renderer
    {
        get
        {
            return _renderer;
        }

        set
        {
            _renderer = value;
        }
    }
    protected ObjectRenderer _renderer;
    protected float _height;
}
