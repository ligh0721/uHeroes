using UnityEngine;


public class Levelable {
    int m_iLvl;
    int m_iMaxLvl;
    int m_iExp;
    int m_iBaseExp;
    int m_iMaxExp;
    ILevelableHandler m_pUpdate;

    public Levelable(ILevelableHandler handler) {
        m_iLvl = 0;
        m_iMaxLvl = 1;
        m_iExp = 0;
        m_iBaseExp = 0;
        m_iMaxLvl = 0;
        m_pUpdate = handler;
    }

    public int Level
    {
        get
        {
            return m_iLvl;
        }

        set
        {
            int iOldLvl = m_iLvl;

            if (value >= m_iMaxLvl) {
                value = m_iMaxLvl;
            } else if (value < 0) {
                value = 0;
            }

            m_iLvl = value;

            int iChanged = m_iLvl - iOldLvl;

            if (iChanged != 0) {
                if (m_iLvl == m_iMaxLvl) {
                    m_iExp = 0;
                }
                if (m_pUpdate != null) {
                    m_pUpdate.OnLevelChanged(this, iChanged);
                }
                UpdateExpRange();
            }
        }
    }

    public void AddLevel(int iLvl) {
        Level = m_iLvl + iLvl;
    }

    public int MaxLevel
    {
        get
        {
            return m_iMaxLvl;
        }

        set
        {
            m_iMaxLvl = value <= 0 ? 1 : value;
            Level = m_iLvl;
        }
    }

    public bool CanIncreaseExp()
    {
        return m_iLvl < m_iMaxLvl;
    }

    public int Exp
    {
        get { return m_iExp; }
        set { m_iExp = value; }
    }

    public int BaseExp
    {
        get { return m_iBaseExp; }
    }

    public int MaxExp
    {
        get { return m_iMaxExp; }
    }

    public void SetExpRange(int baseExp, int maxExp) {
        Debug.Assert(maxExp > baseExp && maxExp > m_iExp);
        m_iBaseExp = baseExp;
        if (m_iBaseExp > m_iExp) {
            m_iExp = m_iBaseExp;
        }
        m_iMaxExp = maxExp;
    }

    public void AddExp(int iExp) {
        if (m_iLvl == m_iMaxLvl) {
            return;
        }

        m_iExp += iExp;
        while (m_iExp >= m_iMaxExp && m_iLvl < m_iMaxLvl) {
            ++m_iLvl;
            UpdateExpRange();
            if (m_pUpdate != null) {
                m_pUpdate.OnLevelChanged(this, 1);
            }
        }

        if (m_iLvl == m_iMaxLvl) {
            m_iExp = m_iBaseExp;
        }
    }

    public void UpdateExpRange() {
        if (m_pUpdate != null) {
            m_pUpdate.OnUpdateExpRange(this);
        } else {
            // default level update
            m_iBaseExp = m_iExp;
            m_iMaxExp = (int)(m_iExp * 1.5);
        }
    }

}

public interface ILevelableHandler {
    void OnLevelChanged(Levelable level, int iChanged);
    void OnUpdateExpRange(Levelable level);
}
