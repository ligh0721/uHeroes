using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomStatusBarUI : MonoBehaviour {
    public Sprite m_SpriteAttackPhysical;
    public Sprite m_SpriteAttackMagical;
    public Sprite m_SpriteArmorHeavy;
    public Sprite m_SpriteArmorCrystal;

    public Image m_portrait;
    public Text m_name;
    public Text m_level;
    public Text m_hp;
    public Image m_attackType;
    public Text m_attackValue;
    public Image m_armorType;
    public Text m_armorValue;

    UnitSafe m_unit;
    int m_hpValue;
    int m_maxHpValue;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Unit u = m_unit;
        if (u == null) {
            Hide();
            return;
        }

        int hpValue = (int)u.Hp;
        int maxHpValue = (int)u.MaxHp;
        if (hpValue != m_hpValue || maxHpValue != m_maxHpValue) {
            m_hpValue = hpValue;
            m_maxHpValue = maxHpValue;
            m_hp.text = string.Format("{0}/{1}", m_hpValue, m_maxHpValue);
        }
    }

    public void SetUnit(Unit unit) {
        if (m_unit.Unit == unit) {
            return;
        }
        m_unit.Set(unit);
        Unit u = m_unit;
        if (u == null) {
            Hide();
            return;
        }

        string path = string.Format("{0}/portrait_sel", u.Model);
        m_portrait.sprite = Resources.Load<Sprite>(path);
        m_name.text = u.Name;
        m_level.text = string.Format("Lv {0}", 1);

        // hp
        m_hpValue = (int)u.Hp;
        m_maxHpValue = (int)u.MaxHp;
        m_hp.text = string.Format("{0}/{1}", m_hpValue, m_maxHpValue);

        // attack
        switch (u.AttackSkill.AttackType) {
        default:
            m_attackType.sprite = m_SpriteAttackPhysical;
            break;
        case AttackValue.Type.kMagical:
            m_attackType.sprite = m_SpriteAttackMagical;
            break;
        }
        float baseValue = u.AttackSkill.AttackValueBase;
        float halfRange = u.AttackSkill.AttackValueRandomRange * 0.5f;
        int from = (int)(baseValue * (1.0f - halfRange));
        int to = (int)(baseValue * (1.0f + halfRange));
        Coeff coeff = u.AttackSkill.AttackValueCoeff;
        int appendValue = (int)(u.AttackSkill.AttackValue - baseValue);
        if (appendValue > 0) {
            m_attackValue.text = string.Format("{0} - {1} <color=lime>+{2}</color>", from, to, appendValue);
        } else if (appendValue < 0) {
            m_attackValue.text = string.Format("{0} - {1} <color=red>{2}</color>", from, to, appendValue);
        } else {
            m_attackValue.text = string.Format("{0} - {1}", from, to);
        }

        // armor
        switch (u.ArmorType) {
        default:
            m_armorType.sprite = m_SpriteArmorHeavy;
            break;
        case ArmorValue.Type.kCrystal:
            m_armorType.sprite = m_SpriteArmorCrystal;
            break;
        }
        baseValue = u.ArmorValueBase;
        appendValue = (int)(u.ArmorValue - baseValue);
        if (appendValue > 0) {
            m_armorValue.text = string.Format("{0} <color=lime>+{1}</color>", (int)baseValue, appendValue);
        } else if (appendValue < 0) {
            m_armorValue.text = string.Format("{0} <color=red>{1}</color>", (int)baseValue, appendValue);
        } else {
            m_armorValue.text = string.Format("{0}", (int)baseValue);
        }
    }

    public void Show() {
        gameObject.SetActive(true);
        enabled = true;
    }

    public void Hide() {
        enabled = false;
        gameObject.SetActive(false);
    }


}
