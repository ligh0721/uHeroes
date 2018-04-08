using UnityEngine;
using System.Collections.Generic;
using cca;

public class ModelNode : NodeWithHeight {
    void Start() {
        // TODO: delete test 删掉是否会被调用
        init();
    }

    void OnDestroy() {
        cleanup();
    }

    public const int kActionMove = 0x1;
    public const int kActionDie = 0x2;
    public const int kActionAct1 = 0x11;
    public const int kActionAct2 = 0x12;
    public const int kActionAct3 = 0x13;
    public const int kActionAct4 = 0x14;
    public const int kActionAct5 = 0x15;
    public const int kActionAct6 = 0x16;
    public const int kActionAct7 = 0x17;
    public const int kActionAct8 = 0x18;
    public const int kActionAct9 = 0x19;

    public const int kActionMoveTo = 0x101;

    public const int kFrameDefault = 0x1001;

    public const int CONST_LOOP_FOREVER = -1;


    // like NameToId("move")
    public static int NameToId(string name) {
        switch (name) {
        case "move":
            return kActionMove;
        case "die":
            return kActionDie;
        case "act1":
            return kActionAct1;
        case "act2":
            return kActionAct2;
        case "act3":
            return kActionAct3;
        case "act4":
            return kActionAct4;
        case "act5":
            return kActionAct5;
        case "act6":
            return kActionAct6;
        case "act7":
            return kActionAct7;
        case "act8":
            return kActionAct8;
        case "act9":
            return kActionAct9;

        case "default":
            return kFrameDefault;

        default:
            return 0;
        }
    }

    public static string IdToName(int id) {
        switch (id) {
        case kActionMove:
            return "move";
        case kActionDie:
            return "die";
        case kActionAct1:
            return "act1";
        case kActionAct2:
            return "act2";
        case kActionAct3:
            return "act3";
        case kActionAct4:
            return "act4";
        case kActionAct5:
            return "act5";
        case kActionAct6:
            return "act6";
        case kActionAct7:
            return "act7";
        case kActionAct8:
            return "act8";
        case kActionAct9:
            return "act9";

        case kFrameDefault:
            return "default";

        default:
            return null;
        }
    }

    // like PrepareAnimation(kActionMove, "Malik/move")
    public void AssignAnimation(int id, cca.Animation animation) {
        m_animations.Add(id, animation);
    }

    public void AssignFrame(int id, Sprite frame) {
        m_frames.Add(id, frame);
    }

    public bool IsDoingAction(int tag) {
        return tag != 0 && getActionByTag(tag) != null;
    }

    public virtual void StopAction(int tag) {
        if (tag != 0) {
            stopActionByTag(tag);
        }
    }

    public virtual void StopAllActions() {
        stopAllActions();
    }

    public virtual void SetActionSpeed(int tag, float speed) {
        if (tag == 0) {
            return;
        }

        Speed spd = getActionByTag(tag) as Speed;
        if (spd == null) {
            return;
        }

        spd.ActionSpeed = speed;
    }

    public virtual void SetFlippedX(bool flippedX) {
        base.flippedX = flippedX;
    }

    public virtual void SetFrame(int id) {
        Sprite fr;
        if (!m_frames.TryGetValue(id, out fr)) {
            return;
        }
        frame = fr;
    }

    public virtual void DoMoveTo(Vector2 pos, float duration, Function onFinished, float speed = 1.0f) {
        var action = new Speed(new Sequence(new MoveTo(duration, pos), new CallFunc(onFinished)), speed);
        action.tag = kActionMoveTo;
        runAction(action);
    }

    public virtual void DoAnimate(int id, Function onSpecial, int loop, Function onFinished, float speed = 1.0f) {
        ActionInterval act;
        cca.Animation ani;
        if (!m_animations.TryGetValue(id, out ani)) {
            if (id == kActionDie) {
                act = new FadeOut(0.1f);
            } else {
                act = new DelayTime(0.2f);
            }
            if (onSpecial != null) {
                act = new Sequence(act, new CallFunc(onSpecial));
            }
        } else {
            act = new Animate(ani, delegate (int index, ref object data) {
                    if (onSpecial != null) {
                        onSpecial();
                    }
                });
        }

        if (loop == CONST_LOOP_FOREVER) {
            act = new RepeatForever(act);
        } else {
            act = new Sequence(new Repeat(act, (uint)loop), new CallFunc(onFinished));
        }

        Speed action = new Speed(act, speed);
        action.tag = id;
        runAction(action);
    }

    protected Dictionary<int, cca.Animation> m_animations = new Dictionary<int, cca.Animation>();
    protected Dictionary<int, Sprite> m_frames = new Dictionary<int, Sprite>();
}
