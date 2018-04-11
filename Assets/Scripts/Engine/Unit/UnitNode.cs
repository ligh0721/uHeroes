using UnityEngine;
using System.Collections.Generic;
using cca;


[RequireComponent(typeof(Unit))]
public class UnitNode : ModelNode, INetworkable<GamePlayerController> {
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

    protected internal int m_id;

    public int Id {
        get { return m_id; }
    }

    public override void StopAction(int tag) {

        GamePlayerController.localClient.ServerAddSyncAction(new SyncStopAction(this, tag));
        base.StopAction(tag);
    }

    public override void StopAllActions() {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncStopAllActions(this));
        base.StopAllActions();
    }

    public override void SetActionSpeed(int tag, float speed) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncSetActionSpeed(this, tag, speed));
        base.SetActionSpeed(tag, speed);
    }

    public override void SetFrame(int id) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncSetFrame(this, id));
        base.SetFrame(id);
    }

    public override void SetFlippedX(bool flippedX) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncSetFlippedX(this, flippedX));
        base.SetFlippedX(flippedX);
    }

    public override void DoMoveTo(Vector2 pos, float duration, Function onFinished, float speed = 1.0f) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncDoMoveTo(this, pos, duration, speed));
        base.DoMoveTo(pos, duration, onFinished, speed);
    }

    public override void DoAnimate(int id, Function onSpecial, int loop, Function onFinished, float speed = 1.0f) {
        GamePlayerController.localClient.ServerAddSyncAction(new SyncDoAnimate(this, id, loop, speed));
        base.DoAnimate(id, onSpecial, loop, onFinished, speed);
    }

    public void AddBattleTip(string tip, string font, float fontSize, Color color) {
        //Debug.LogFormat (tip);
    }

    public void SetGeometry(float halfOfWidth, float halfOfHeight, Vector2 fireOffset) {
        Sprite frame;
        if (!m_frames.TryGetValue(kFrameDefault, out frame)) {
            return;
        }

        float width = frame.rect.size.x / frame.pixelsPerUnit;
        float height = frame.rect.size.y / frame.pixelsPerUnit;
        m_halfOfWidth = halfOfWidth * width;
        m_halfOfHeight = halfOfHeight * height;
        m_fireOffset.x = fireOffset.x * width;
        m_fireOffset.y = fireOffset.y * height;
    }

    public float HalfOfWidth {
        get { return m_halfOfWidth; }

        //set
        //{
        //m_halfOfWidth = value * m_node.size.x;
        //}
    }

    public float HalfOfHeight {
        get { return m_halfOfHeight; }

        //set
        //{
        //m_halfOfHeight = value * m_node.size.y;
        //}
    }

    public Vector2 FireOffset {
        get { return m_fireOffset; }

        //set
        //{
        //m_fireOffset = new Vector2(value.x * m_node.size.x, value.y * m_node.size.y);
        //}
    }

    public GamePlayerController localClient {
        get { return GamePlayerController.localClient; }
    }

    public bool isServer {
        get { return localClient.isServer; }
    }

    protected float m_halfOfWidth;
    protected float m_halfOfHeight;
    protected Vector2 m_fireOffset = new Vector2();
}
