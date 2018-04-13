using UnityEngine;
using System.Collections;
using cca;


public class NodeWithHeight : Node {
    protected float _height;

#if UNITY_EDITOR
    void Reset() {
        Awake();
    }
#endif

    void Awake() {
        // TODO: delete test 删掉是否会被调用
        init();
    }

    void OnDestroy() {
        cleanup();
    }

    public override void init() {
        base.init();
        height = 0;
    }

    public const float CONST_BASE_Z = -100.0f;

    public float height {
        get { return _height; }

        set {
            Vector3 pos = transform.localPosition;
            pos.y -= height;
            pos.z = CONST_BASE_Z + pos.y;
            pos.y += value;
            _height = value;

            transform.localPosition = pos;
        }
    }

    public override Vector2 position {
        get {
            Vector2 pos = transform.localPosition;
            pos.y -= _height;
            return pos;
        }

        set { transform.localPosition = new Vector3(value.x, value.y + _height, CONST_BASE_Z + value.y); }
    }
}
