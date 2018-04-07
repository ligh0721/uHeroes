using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(UnitController))]
public class UnitControllerEditor : Editor {
    SerializedProperty m_uiPrefab;

    TextAsset m_modelTextAsset;

    void OnEnable() {
        m_uiPrefab = serializedObject.FindProperty("m_uiPrefab");
    }

    public override void OnInspectorGUI() {
        UnitController t = target as UnitController;
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_uiPrefab);

        TextAsset textAsset = EditorGUILayout.ObjectField("Unit", m_modelTextAsset, typeof(TextAsset), true) as TextAsset;
        if (textAsset != m_modelTextAsset) {
            UnitInfo baseInfo = ResourceManager.instance.LoadUnit(null, textAsset.text);
            UnitRenderer r = new UnitRenderer(PrefabUtility.GetPrefabParent(t.gameObject) as GameObject, t.gameObject);
            ResourceManager.instance.PrepareUnitResource(baseInfo.model, r);
            t.m_unit = new Unit(r);

            if (t.m_ui == null) {
                t.m_ui = UnitHUD.Create(t);
            } else {
                t.m_ui.UpdateRectTransform();
            }
            m_modelTextAsset = textAsset;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
