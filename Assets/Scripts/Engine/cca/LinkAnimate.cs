using UnityEngine;
using System.Collections;
using cca;


class LinkAnimate : Animate {
    public LinkAnimate(cca.Animation animation, Animate.Function onSpecial, UnitNode from, UnitNode to)
        : base(animation, onSpecial) {
        m_fromToType = Projectile.FromToType.kUnitToUnit;
        m_from.Set(from.GetComponent<Unit>());
        m_to.Set(to.GetComponent<Unit>());
    }

    public override Action clone() {
        return new LinkAnimate(_animation.clone(), _onSpecial, m_from.Node, m_to.Node);
    }

    public override Action reverse() {
        Sprite[] frames = _animation.Frames.Clone() as Sprite[];
        System.Array.Reverse(frames);

        cca.Animation newAnim = new cca.Animation(frames, _animation.DelayPerUnit);
        newAnim.RestoreOriginalFrame = _animation.RestoreOriginalFrame;
        return new LinkAnimate(newAnim, _onSpecial, m_from.Node, m_to.Node);
    }

    public override void startWithTarget(Node target) {
        base.startWithTarget(target);

        ProjectileNode r = target as ProjectileNode;
        Projectile p = r.Projectile;
        m_fireFrom = m_fromToType == Projectile.FromToType.kUnitToUnit && p.SourceUnit != null && m_from.Node == p.SourceUnit.Node;
        //target.visible = false;
    }

    public override void update(float t) {
        if (_target) {
            if (_target.visible == false) {
                _target.visible = true;
            }
            fixTargetPosition();
        }

        base.update(t);
    }

    protected void fixTargetPosition() {
        UnitNode from = m_from.Node;
        UnitNode to = m_to.Node;
        if (from == null || to == null) {
            return;
        }

        m_fromPos = from.position;

        float fFromOffsetX = 0.0f;
        float fFromHeight = 0.0f;
        if (m_fireFrom) {
            bool useFireOffset = (_target as ProjectileNode).Projectile.UseFireOffset;
            float offsetX = useFireOffset ? from.FireOffset.x : from.HalfOfWidth;
            float offsetY = useFireOffset ? from.FireOffset.y : from.HalfOfHeight;
            bool bFlipX = m_fromPos.x > m_toPos.x;
            fFromOffsetX = (bFlipX ? -offsetX : offsetX);
            fFromHeight = from.height + offsetY;
        } else {
            fFromHeight = from.height + from.HalfOfHeight;
        }

        m_fromPos.x += fFromOffsetX;
        m_fromPos.y += fFromHeight;

        m_toPos = to.position;
        float fToHeight = to.height + to.HalfOfHeight;
        m_toPos.y += fToHeight;

        //RendererNode rendererTarget = _target as RendererNode;
        //rendererTarget.height = (fFromHeight + fToHeight) / 2;

        Vector2 delta = m_toPos - m_fromPos;
        float fR = Utils.GetAngle(delta);

        float fScale = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y) / _target.size.x;
        _target.position = new Vector2((m_fromPos.x + m_toPos.x) / 2, (m_fromPos.y + m_toPos.y) / 2);
        _target.rotation = fR;
        Vector2 scale = _target.scale;
        scale.x = fScale;
        _target.scale = scale;
    }

    protected Projectile.FromToType m_fromToType;
    protected UnitSafe m_from;
    protected Vector2 m_fromPos;
    protected UnitSafe m_to;
    protected Vector2 m_toPos;
    protected bool m_fireFrom;
    // 用来区分这段link是刚发射的，还是传递中的，他们的出发点位置不同
}
