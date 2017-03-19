using System.Collections.Generic;


// v = a * x + b
public struct Coeff
{
    public Coeff(float a, float b)
    {
        this.a = a;
        this.b = b;
    }

    public float GetValue(float x)
    {
        return a * x + b;
    }

    public float a;
    public float b;
}

public struct Value
{
    public Value(float value)
    {
        x = value;
        coeff = new Coeff(1, 0);
    }

    public float a
    {
        get
        {
            return coeff.a;
        }

        set
        {
            coeff.a = value;
        }
    }

    public float b
    {
        get
        {
            return coeff.b;
        }

        set
        {
            coeff.b = value;
        }
    }

    public float v
    {
        get
        {
            return coeff.GetValue(x);
        }
    }

    public float x;
    public Coeff coeff;
}

public struct AttackValue
{
    public const int CONST_MAX_ATTACK_TYPE = 4;

    public enum Type
    {
        kPhysical,
        kMagical,
        kSiege,
        kHoly
    }

    public static Type NameToType(string name)
    {
        switch (name)
        {
            default:
                return Type.kPhysical;
            case "Magical":
                return Type.kMagical;
            case "Siege":
                return Type.kSiege;
            case "Holy":
                return Type.kHoly;
        }
    }

    public static string TypeToName(Type type)
    {
        switch (type)
        {
            default:
                return "Physical";
            case Type.kMagical:
                return "Magical";
            case Type.kSiege:
                return "Siege";
            case Type.kHoly:
                return "Holy";
        }
    }

    public AttackValue(Type type, float value)
    {
        this.type = type;
        this.value = new Value(value);
    }

    public void SetBase(Type type, float value)
    {
        this.type = type;
        this.value.x = value;
    }

    public void SetCoef(float a, float b)
    {
        value.a = a;
        value.b = b;
    }

    public float x
    {
        get
        {
            return value.x;
        }

        set
        {
            this.value.x = value;
        }
    }

    public float v
    {
        get
        {
            return value.v;
        }
    }

    public Coeff coeff
    {
        get
        {
            return value.coeff;
        }

        set
        {
            this.value.coeff = value;
        }
    }

    public float a
    {
        get
        {
            return value.a;
        }

        set
        {
            this.value.a = value;
        }
    }

    public float b
    {
        get
        {
            return value.b;
        }

        set
        {
            this.value.b = value;
        }
    }

    public Type type;
    public Value value;
}

public struct ArmorValue
{
    public const int CONST_MAX_ARMOR_TYPE = 5;

    public enum Type
    {
        kHeavy,
        kCrystal,
        kWall,
        kHero,
        kHoly
    }

    public static Type NameToType(string name)
    {
        switch (name)
        {
            default:
                return Type.kHeavy;
            case "Heavy":
                return Type.kCrystal;
            case "Crystal":
                return Type.kWall;
            case "Wall":
                return Type.kHero;
            case "Holy":
                return Type.kHoly;
        }
    }

    public static string TypeToName(Type type)
    {
        switch (type)
        {
            default:
                return "Heavy";
            case Type.kCrystal:
                return "Heavy";
            case Type.kWall:
                return "Crystal";
            case Type.kHero:
                return "Wall";
            case Type.kHoly:
                return "Holy";
        }
    }

    public ArmorValue(Type type, float value)
    {
        this.type = type;
        this.value = new Value(value);
    }

    public void SetBase(Type type, float value)
    {
        this.type = type;
        this.value.x = value;
    }

    public void SetCoef(float a, float b)
    {
        value.a = a;
        value.b = b;
    }

    public float x
    {
        get
        {
            return value.x;
        }

        set
        {
            this.value.x = value;
        }
    }

    public float v
    {
        get
        {
            return value.v;
        }
    }

    public Coeff coeff
    {
        get
        {
            return value.coeff;
        }

        set
        {
            this.value.coeff = value;
        }
    }

    public float a
    {
        get
        {
            return value.a;
        }

        set
        {
            this.value.a = value;
        }
    }

    public float b
    {
        get
        {
            return value.b;
        }

        set
        {
            this.value.b = value;
        }
    }

    public Type type;
    public Value value;
}

public class ArmorAttackTable
{
    public static float[,] Table
    {
        get
        {
            return s_table;
        }
    }

    /*
     0.00    无
     0.25    微弱
     0.50    弱
     0.75    较弱
     1.00    正常
     1.25    较强
     1.50    强
     1.75    超强
     2.00    瓦解
     */
    // 防护效果表
    static float[,] s_table = {
    //           物理攻击 魔法攻击 攻城攻击 神圣攻击
    /*重装护甲*/ { 1.50f,   0.25f,   1.00f,   0.00f },
    /*水晶护甲*/ { 0.25f,   1.50f,   0.75f,   0.00f },
    /*城墙护甲*/ { 2.00f,   2.00f,   0.25f,   0.00f },
    /*英雄护甲*/ { 1.00f,   1.00f,   1.00f,   0.00f },
    /*神圣护甲*/ { 5.00f,   5.00f,   5.00f,   1.00f }
    };
}

public struct AttackBuff
{
    public AttackBuff(BuffSkill buffTemplate)
    {
        this.buffTemplate = buffTemplate;
    }

    public BuffSkill buffTemplate;
}

public class AttackData
{
    public AttackData()
    {
		m_attackValue = new AttackValue (AttackValue.Type.kPhysical, 0);
        m_attackBuffs = new List<AttackBuff>();
    }

    public AttackData(AttackValue value, List<AttackBuff> buff = null)
    {
        m_attackValue = value;
        if (buff != null)
        {
            m_attackBuffs = new List<AttackBuff>(buff);
        }
        else
        {
            m_attackBuffs = new List<AttackBuff>();
        }
    }

    public AttackData(AttackData ad)
    {
        m_attackValue = ad.m_attackValue;
        m_attackBuffs = new List<AttackBuff>(ad.m_attackBuffs);
    }

    public AttackData Clone()
    {
        return new AttackData(m_attackValue, m_attackBuffs);
    }

    public void setAttackValueBase(AttackValue.Type type, float value)
    {
        m_attackValue.SetBase(type, value);
    }

    public void addAttackBuff(AttackBuff buff)
    {
        m_attackBuffs.Add(buff);
    }

    public AttackValue attackValue
    {
        get
        {
            return m_attackValue;
        }

        set
        {
            m_attackValue = value;
        }
    }

    public List<AttackBuff> attackBuffs
    {
        get
        {
            return m_attackBuffs;
        }
    }

    public void AddBuff(params AttackBuff[] buffs)
    {
        m_attackBuffs.AddRange(buffs);
    }

    protected AttackValue m_attackValue;
    protected List<AttackBuff> m_attackBuffs;
}
