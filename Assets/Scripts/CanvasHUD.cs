using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CanvasHUD : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var list = new List<Transform>();
        foreach (Transform t in transform) {
            list.Add(t);
        }

        list.Sort((a, b) => {
            return b.position.z.CompareTo(a.position.z);
        });

        for (int i = 0; i < list.Count; i++) {
            list[i].SetSiblingIndex(i);
        }
    }
}
