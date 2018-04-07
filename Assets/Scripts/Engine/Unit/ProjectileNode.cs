using UnityEngine;
using cca;


[RequireComponent(typeof(Projectile))]
public class ProjectileNode : ModelNode
{
    void Start() {
        // TODO: delete test 删掉是否会被调用
        init();
    }

    void OnDestroy() {
        cleanup();
    }

    Projectile m_projectile;
	public Projectile Projectile {
		get {
			return m_projectile;
		}
	}

    public virtual void DoLinkUnitToUnit(UnitNode from, UnitNode to, int id, Function onSpecial, int loop, Function onFinished)
    {
		ActionInterval act;
		cca.Animation ani;
		if (!m_animations.TryGetValue(id, out ani))
		{
			if (id == kActionDie)
			{
				act = new FadeOut(0.1f);
			}
			else
			{
				act = new DelayTime(0.2f);
			}
			if (onSpecial != null)
			{
				act = new Sequence(act, new CallFunc(onSpecial));
			}
		}
		else
		{
            visible = false;
			act = new LinkAnimate(ani, delegate (int index, ref object data) {
				if (onSpecial != null)
				{
					onSpecial();
				}
			}, from, to);
		}

		if (loop == CONST_LOOP_FOREVER)
		{
			act = new RepeatForever(act);
		}
		else
		{
			act = new Sequence(new Repeat(act, (uint)loop), new CallFunc(onFinished));
		}

		//m_node.stopActionByTag(id);
		act.tag = id;
		runAction(act);
    }

    public virtual void DoMoveToUnit(UnitNode pToUnit, bool bFixRotation, float fMaxHeightDelta, float duration, Function onFinished)
    {
        ActionInterval act = new MoveToNode(duration, pToUnit, true, fMaxHeightDelta);
        act = new Sequence(act, new CallFunc(onFinished));
        act.tag = kActionMoveTo;
        runAction(act);
    }

    public override void DoMoveTo(Vector2 pos, float duration, Function onFinished, float speed = 1.0f)
    {
        base.DoMoveTo(pos, duration, onFinished, speed);
        rotation = Mathf.Atan2(pos.y - position.y, pos.x - position.x);
    }
}
