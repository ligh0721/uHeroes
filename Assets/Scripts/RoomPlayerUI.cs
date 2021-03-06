﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomPlayerUI : MonoBehaviour {
    public Image m_portrait;
    public Text m_name;
    public Text m_progress;

    Slider m_slider;
    IEnumerator m_progAct;
	// Use this for initialization
	void Start () {
        m_slider = GetComponent<Slider>();
        m_progress.enabled = false;
        m_slider.value = 0.0f;
	}

    public string Name {
        get { return m_name.text; }
        set { m_name.text = value; }
    }

    public Sprite Portrait {
        get { return m_portrait.sprite; }
        set { m_portrait.sprite = value; }
    }

    public float Progress {
        get { return m_slider.value; }
        set {
            m_slider.value = value;
            m_progress.text = string.Format("{0:N0}%", m_slider.value * 100.0f);
        }
    }

    IEnumerator ProgressAction(float to) {
        float from = m_slider.value;
        while (from < to) {
            from += 0.01f;
            m_slider.value = from;
            m_progress.text = string.Format("{0:N0}%", m_slider.value * 100.0f);
            yield return null;
        }
        
        m_progAct = null;
    }

    public bool IsRunningProgressAction {
        get { return m_progAct != null; }
    }

    public void ShowProgressText() {
        m_progress.enabled = true;
        m_progress.text = "0%";
    }
}
