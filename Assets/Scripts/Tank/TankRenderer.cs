using UnityEngine;
using System.Collections.Generic;
using cca;


public class TankGunRenderer : UnitRenderer {
    public TankGunRenderer() {
    }

    public TankGunRenderer(GameObject prefab, GameObject gameObject)
        : base(prefab, gameObject) {
    }

    public override void SetFrame(int id) {
        if (m_frames.Count == 0) {
            Texture2D texture = Resources.Load<Texture2D>(string.Format("{0}/{1}", "Tanks/gun", "gun14"));
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            m_frames.Add(kFrameDefault, sprite);
        }

        Sprite frame;
        if (!m_frames.TryGetValue(id, out frame)) {
            return;
        }
        m_node.frame = frame;
    }
}


public class TankRenderer : UnitRenderer {
    public TankRenderer() {
    }

    public TankRenderer(GameObject prefab, GameObject gameObject)
        : base(prefab, gameObject) {
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
        GamePlayerController.localClient.ServerAddSyncAction(new SyncDoMoveTo(m_unit.Id, pos, duration, speed));

        float rotation;
        if (Node.rotation > 180.0f) {
            rotation = Node.rotation - 360.0f;
        } else {
            rotation = Node.rotation;
        }
        rotation = Node.rotation;
        float angle = Utils.GetAngle(pos - Node.position);
        if (angle < 0) {
            angle += 360;
        }
        Debug.LogFormat("{0}, {1}", rotation, angle);
        float delta = angle - rotation;
        if (delta > 180) {
            delta -= 360;
        } else if (delta < -180) {
            delta += 360;
        }
        var action = new Speed(new Sequence(new RotateBy(Mathf.Abs(delta / 500.0f), delta), new MoveTo(duration, pos), new CallFunc(onFinished)), speed);
        action.tag = kActionMoveTo;
        m_node.runAction(action);
    }
}
