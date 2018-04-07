using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UnitHUD : MonoBehaviour {
    [HideInInspector]
    public UnitController m_unitCtrl;
    public Slider m_hpSlider;
    public Image m_hpFill;

    RectTransform m_rt;

    public static UnitHUD Create(UnitController ctrl) {
        GameObject gameObject = Instantiate(ctrl.m_uiPrefab);
        gameObject.transform.SetParent(GameObject.Find("HUDCanvas").transform);
        UnitHUD unitui = gameObject.GetComponent<UnitHUD>();
        unitui.m_unitCtrl = ctrl;

        unitui.UpdateRectTransform();
        return unitui;
    }

    // Use this for initialization
    void Start () {
        if (m_rt == null) {
            m_rt = GetComponent<RectTransform>();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (m_unitCtrl != null) {
            var pos = m_unitCtrl.transform.position;
            pos.z -= 0.001f;
            m_rt.position = pos;

            if (m_unitCtrl.unit != null) {
                float hpPer = m_unitCtrl.unit.Hp / m_unitCtrl.unit.MaxHp;
                if (hpPer != m_hpSlider.value) {
                    m_hpSlider.value = hpPer;
                }
            }
        }

        m_hpFill.color = new Color(Mathf.Min(1.0f, (1.00f - m_hpSlider.value) * 2.0f), Mathf.Min(1.0f, 2.0f * m_hpSlider.value), 0);
    }

    public void UpdateRectTransform() {
        // set ui size and pivot
        Sprite sprite = m_unitCtrl.GetComponent<SpriteRenderer>().sprite;
        Vector2 pivot = sprite.pivot;
        Vector2 size = sprite.rect.size;
        pivot = new Vector2(pivot.x / size.x, pivot.y / size.y);
        size = size / sprite.pixelsPerUnit;
        if (m_rt == null) {
            m_rt = GetComponent<RectTransform>();
        }
        m_rt.pivot = pivot;
        m_rt.sizeDelta = size;

        // set hp bar width
        var rt = m_hpSlider.GetComponent<RectTransform>();
        var hpsize = rt.sizeDelta;
        hpsize.x = m_unitCtrl.unit.Renderer.HalfOfWidth * 2 + 0.1f;
        rt.sizeDelta = hpsize;

        // set hp bar hight
        var pos = rt.anchoredPosition;
        pos.y = size.y * (pivot.y - 0.5f) + m_unitCtrl.unit.Renderer.HalfOfHeight * 2.0f + 0.2f;
        rt.anchoredPosition = pos;
    }
}
