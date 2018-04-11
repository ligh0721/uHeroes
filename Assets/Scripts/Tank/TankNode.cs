using UnityEngine;
using System.Collections.Generic;
using cca;


public class TankGunNode : UnitNode {
    void Start() {
        // TODO: delete test 删掉是否会被调用
        init();
    }

    void OnDestroy() {
        cleanup();
    }

    public override void init() {
        base.init();
    }

    public override void SetFrame(int id) {
        if (m_frames.Count == 0) {
            Texture2D texture = Resources.Load<Texture2D>(string.Format("{0}/{1}", "Tanks/gun", "gun14"));
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            m_frames.Add(kFrameDefault, sprite);
        }

        Sprite frameSprite;
        if (!m_frames.TryGetValue(id, out frameSprite)) {
            return;
        }
        frame = frameSprite;
    }
}

[RequireComponent(typeof(Tank))]
public class TankNode : UnitNode {
    void Start() {
        // TODO: delete test 删掉是否会被调用
        init();
    }

    void OnDestroy() {
        cleanup();
    }

    public override void SetFrame(int id) {
        if (m_frames.Count == 0) {
            Texture2D texture = Resources.Load<Texture2D>(string.Format("{0}/{1}", "Tanks/body", "body16"));
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            m_frames.Add(kFrameDefault, sprite);
        }
        base.SetFrame(id);
    }

    public override void DoMoveTo(Vector2 pos, float duration, Function onFinished, float speed = 1.0f) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncDoMoveTo(this, pos, duration, speed));

        float rotationFix;
//        if (rotation > 180.0f) {
//            rotationFix = rotation - 360.0f;
//        } else {
//            rotationFix = rotation;
//        }
        rotationFix = rotation;
        float angle = Utils.GetAngle(pos - position);
        if (angle < 0) {
            angle += 360;
        }
        Debug.LogFormat("{0}, {1}", rotationFix, angle);
        float delta = angle - rotationFix;
        if (delta > 180) {
            delta -= 360;
        } else if (delta < -180) {
            delta += 360;
        }
        var action = new Speed(new Sequence(new RotateBy(Mathf.Abs(delta / 500.0f), delta), new MoveTo(duration, pos), new CallFunc(onFinished)), speed);
        action.tag = kActionMoveTo;
        runAction(action);
    }
}
