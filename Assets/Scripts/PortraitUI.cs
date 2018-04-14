using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PortraitUI : MonoBehaviour, IPointerClickHandler {
    public Text m_level;
    public Image m_portrait;
    public Image m_selected;
    public Slider m_hpSlider;
    public Slider m_expSlider;

    int m_levelValue;
    float m_hp;
    float m_maxHp = 1;
    int m_exp;
    int m_maxExp = 1;
    bool m_selectedValue = true;
    UnitSafe m_unit;
    internal PortraitGroupUI m_parent;

    // Use this for initialization
    void Start() {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Selected = false;
    }

    public Unit Unit {
        get { return m_unit; }
        set { m_unit.Set(value); }
    }
	
	// Update is called once per frame
	void Update() {
        Unit u = m_unit;
        if (u == null) {
            return;
        }

        if (MaxHp != u.MaxHp) {
            MaxHp = u.MaxHp;
        }
        if (Hp != u.Hp) {
            Hp = u.Hp;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        Unit u = m_unit;
        if (u == null) {
            return;
        }

        Selected = true;
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

    public float Hp {
        get {
            return m_hp;
        }

        set {
            m_hp = value;
            float per = m_hp / m_maxHp;
            m_hpSlider.value = per;
        }
    }

    public float MaxHp {
        get {
            return m_maxHp;
        }

        set {
            m_hp = (m_hp / m_maxHp * value);
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
            return m_selectedValue;
        }

        set {
            if (m_selectedValue == value) {
                return;
            }

            var img = m_selected.GetComponent<Image>();
            var color = img.color;
            color.a = value ? 1.0f : 0.0f;
            img.color = color;
            m_selectedValue = value;
            if (value == true) {
                if (m_parent != null) {
                    // 只允许选中一个
                    foreach (PortraitUI portrait in m_parent.m_portraits) {
                        if (portrait != this) {
                            portrait.Selected = false;
                        }
                    }
                }
                OnSelected();
            }
        }
    }

    void OnSelected() {
        Unit u = m_unit;
        if (u != null) {
            if (PlayerUnitController.Current.Controlling != u) {
                PlayerUnitController.Current.Controlling = u;
            }
        }
    }
}
