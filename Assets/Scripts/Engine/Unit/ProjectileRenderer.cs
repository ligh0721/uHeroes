using UnityEngine;
using cca;

public class ProjectileRenderer : ObjectRenderer
{
    public ProjectileRenderer()
    {
    }

    public ProjectileRenderer(GameObject prefab, GameObject gameObject)
        : base(prefab, gameObject)
    {
    }

	public Projectile Projectile {
		get {
			return m_projectile;
		}
	}

    public virtual void DoLinkUnitToUnit(UnitRenderer from, UnitRenderer to, int id, Function onSpecial, int loop, Function onFinished)
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
            m_node.visible = false;
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
		m_node.runAction(act);
    }

    public virtual void DoMoveToUnit(UnitRenderer pToUnit, bool bFixRotation, float fMaxHeightDelta, float duration, Function onFinished)
    {
        ActionInterval act = new MoveToNode(duration, pToUnit, true, fMaxHeightDelta);
        act = new Sequence(act, new CallFunc(onFinished));
        act.tag = kActionMoveTo;
        m_node.runAction(act);
    }

    public override void DoMoveTo(Vector2 pos, float duration, Function onFinished, float speed = 1.0f)
    {
        base.DoMoveTo(pos, duration, onFinished, speed);
        m_node.rotation = Mathf.Atan2(pos.y - m_node.position.y, pos.x - m_node.position.x);
    }

	protected internal Projectile m_projectile;
}
