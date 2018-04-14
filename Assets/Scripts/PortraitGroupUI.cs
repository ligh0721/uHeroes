using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitGroupUI : MonoBehaviour {
    public GameObject m_portraitPrefab;

    internal List<PortraitUI> m_portraits = new List<PortraitUI>();

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	}

    public List<PortraitUI> Portraits {
        get { return m_portraits; }
    }

    public void AddPortrait(Unit unit) {
        GameObject obj = Instantiate(m_portraitPrefab);
        obj.transform.SetParent(gameObject.transform);
        PortraitUI portraitUI = obj.GetComponent<PortraitUI>();
        portraitUI.m_parent = this;

        portraitUI.Unit = unit;
        portraitUI.Portrait = Resources.Load<Sprite>(string.Format("{0}/portrait_hero", unit.Model));
        portraitUI.MaxHp = (int)unit.MaxHp;
        portraitUI.Hp = (int)unit.Hp;
        portraitUI.Level = 1;
        m_portraits.Add(portraitUI);
    }

    public void Select(int index) {
        Debug.Assert(index >= 0 && index < m_portraits.Count);
        m_portraits[index].Selected = true;
    }
}
