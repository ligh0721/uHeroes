using System;


public class BuffSkill : Skill {
    new protected class SkillData : Skill.SkillData {
    }

    public BuffSkill(string pName, float fDuration, bool bStackable)
        : base(pName, 0) {
        m_stackable = bStackable;
    }

    public override Skill Clone() {
        throw new NotImplementedException();
    }

    new protected void CopyDataFrom(Skill from) {
        base.CopyDataFrom(from);
        BuffSkill a = from as BuffSkill;
        m_sourceUnit = a.m_sourceUnit;
        m_appendBuff = a.m_appendBuff;
    }

    public override Skill Clone(string data) {
        throw new NotImplementedException();
    }

    protected void CopyDataFrom(SkillData data) {
        base.CopyDataFrom(data);
    }

    public virtual void OnUnitDisplaceSkill(BuffSkill newBuff) {
    }

    public float Duration {
        get { return m_duration; }

        set { m_duration = value; }
    }

    public float Elapsed {
        get { return m_elapsed; }

        set { m_elapsed = value; }
    }

    public virtual bool Done {
        get { return m_elapsed >= m_duration; }
    }

    public bool Stackable {
        get { return m_stackable; }

        set { m_stackable = value; }
    }

    public Unit SourceUnit {
        get { return m_sourceUnit; }

        set { m_sourceUnit = value; }
    }

    public BuffSkill AppendBuff {
        get { return m_appendBuff; }

        set { m_appendBuff = value; }
    }

    protected float m_duration;
    protected float m_elapsed;
    protected bool m_stackable;
    protected Unit m_sourceUnit;
    protected BuffSkill m_appendBuff;
}
