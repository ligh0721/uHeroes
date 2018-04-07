using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitGroupUI : MonoBehaviour {
    public GameObject m_portraitPrefab;

    List<PortraitUI> m_portraits = new List<PortraitUI>();
    int m_selected;
	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void AddPortrait(Unit unit) {
        GameObject portrait = Instantiate(m_portraitPrefab);
        portrait.transform.SetParent(gameObject.transform);
        PortraitUI portraitUI = portrait.GetComponent<PortraitUI>();
        portraitUI.SetUnit(unit);
        portraitUI.Portrait = Resources.Load<Sprite>(string.Format("{0}/portrait_hero", unit.Model));
        portraitUI.MaxHp = (int)unit.MaxHp;
        portraitUI.Hp = (int)unit.Hp;
        portraitUI.Level = 1;
        m_portraits.Add(portraitUI);
    }
}
