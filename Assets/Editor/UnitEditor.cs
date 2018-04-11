using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Unit))]
public class UnitControllerEditor : Editor {
    SerializedProperty uiPrefab;

    TextAsset m_modelTextAsset;

    void OnEnable() {
        uiPrefab = serializedObject.FindProperty("uiPrefab");
    }

    public override void OnInspectorGUI() {
        Unit t = target as Unit;
        serializedObject.Update();
        EditorGUILayout.PropertyField(uiPrefab);

        TextAsset textAsset = EditorGUILayout.ObjectField("Unit", m_modelTextAsset, typeof(TextAsset), true) as TextAsset;
        if (textAsset != m_modelTextAsset) {
            UnitInfo baseInfo = ResourceManager.instance.LoadUnit(null, textAsset.text);
            t.Node.cleanup();
            ResourceManager.instance.AssignModelToUnitNode(baseInfo.model, t.Node);
            t.Node.SetFrame(ModelNode.kFrameDefault);

            //            if (t.m_ui == null) {
            //                t.m_ui = UnitHUD.Create(t);
            //            } else {
            //                t.m_ui.UpdateRectTransform();
            //            }
            m_modelTextAsset = textAsset;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
