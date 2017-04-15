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
        base.SetFrame(id);
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
}
