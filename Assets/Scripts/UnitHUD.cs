using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UnitHUD : MonoBehaviour {
    [HideInInspector]
    public UnitSafe m_unit;
    public Slider m_hpSlider;
    public Image m_hpFill;

    RectTransform m_rt;

    // Use this for initialization
    void Awake() {
        if (m_rt == null) {
            m_rt = GetComponent<RectTransform>();
        }
    }
	
	// Update is called once per frame
	void Update() {
        Unit u = m_unit;
        if (u != null) {
            var pos = u.transform.position;
            pos.z -= 0.001f;
            m_rt.position = pos;

            float hpPer = u.Hp / u.MaxHp;
            if (hpPer != m_hpSlider.value) {
                m_hpSlider.value = hpPer;
            }
        }

        m_hpFill.color = new Color(Mathf.Min(1.0f, (1.00f - m_hpSlider.value) * 2.0f), Mathf.Min(1.0f, 2.0f * m_hpSlider.value), 0);
    }

    public void UpdateRectTransform() {
        // set ui size and pivot
        Sprite sprite = m_unit.Node.frame;
        Vector2 pivot = sprite.pivot;
        Vector2 size = sprite.rect.size;
        pivot = new Vector2(pivot.x / size.x, pivot.y / size.y);
        size = size / sprite.pixelsPerUnit;
        if (m_rt == null) {
            m_rt = GetComponent<RectTransform>();
        }
        m_rt.pivot = pivot;
        size.x = m_unit.Node.HalfOfWidth * 2;
        size.y = m_unit.Node.HalfOfHeight * 2);
        m_rt.sizeDelta = size;

        // set hp bar width
        var rt = m_hpSlider.GetComponent<RectTransform>();
        var hpsize = rt.sizeDelta;
        hpsize.x = m_unit.Node.HalfOfWidth * 2 + 0.1f;
        rt.sizeDelta = hpsize;

        // set hp bar hight
        var pos = rt.anchoredPosition;
        pos.y = size.y * (pivot.y - 0.5f) + m_unit.Node.HalfOfHeight * 2.0f + 0.2f;
        rt.anchoredPosition = pos;

        Update();
    }
}
