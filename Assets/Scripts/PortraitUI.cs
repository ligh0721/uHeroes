using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitUI : MonoBehaviour {
    public Text m_level;
    public Image m_portrait;
    public Image m_selected;
    public Slider m_hpSlider;
    public Slider m_expSlider;

    int m_levelValue;
    int m_hp;
    int m_maxHp = 1;
    int m_exp;
    int m_maxExp = 1;
    Unit m_unit;
    // Use this for initialization
    void Start () {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Selected = false;
    }

    public void SetUnit(Unit unit) {
        m_unit = unit;
    }
	
	// Update is called once per frame
	void Update () {
        MaxHp = (int)m_unit.MaxHp;
        Hp = (int)m_unit.Hp;
    }
    
    public int Level {
        get {
            return m_levelValue;
        }

        set {
            m_levelValue = value;
            m_level.text = m_levelValue.ToString();
        }
    }

    public Sprite Portrait {
        get {
            return m_portrait.sprite;
        }

        set {
            m_portrait.sprite = value;
        }
    }

    public int Hp {
        get {
            return m_hp;
        }

        set {
            m_hp = value;
            float per = (float)m_hp / m_maxHp;
            m_hpSlider.value = per;
        }
    }

    public int MaxHp {
        get {
            return m_maxHp;
        }

        set {
            m_hp = (int)(m_hp * 1.0f / m_maxHp * value);
            m_maxHp = value;
        }
    }

    public int Exp {
        get {
            return m_exp ;
        }

        set {
            m_exp = value;
            float per = (float)m_exp / m_maxExp;
            m_expSlider.value = per;
        }
    }

    public int MaxExp {
        get {
            return m_maxExp;
        }

        set {
            m_exp = (int)((float)m_exp / m_maxExp * value);
            m_maxExp = value;
        }
    }

    public bool Selected {
        get {
            return m_selected.GetComponent<Renderer>().material.color.a != 0.0f;
        }

        set {
            var img = m_selected.GetComponent<Image>();
            var color = img.color;
            color.a = value ? 1.0f : 0.0f;
            img.color = color;
        }
    }
}
