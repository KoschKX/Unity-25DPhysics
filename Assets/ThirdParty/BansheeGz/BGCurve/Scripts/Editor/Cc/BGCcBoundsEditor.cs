using UnityEngine;
using System.Collections;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using UnityEditor;

namespace BansheeGz.BGSpline.Editor
{
    [CustomEditor(typeof(BGCcBounds))]
    public class BGCcBoundsEditor : BGCcSplitterPolylineEditor
    {
        private BGCcBounds Bounds
        {
            get { return (BGCcBounds) cc; }
        }

        protected override void AdditionalParams()
        {
            BGEditorUtility.VerticalBox(() =>
            {
                BGEditorUtility.Horizontal(() =>
                {
                    //EditorGUILayout.PropertyField(serializedObject.FindProperty("profileMode"));
                    //if (!GUILayout.Button("Rebuild")) return;

                    Bounds.UpdateUI();
                });

                if (Bounds.ProfileMode == BGCcBounds.ProfileModeEnum.Line)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pathWidth"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pathHeight"));
                }
                else
                {
                    //EditorGUILayout.PropertyField(serializedObject.FindProperty("profileSpline"));
                    if (Bounds.ProfileSpline != null)
                    {
//                        BGEditorUtility.CustomField(new GUIContent("U Coord Field"), Bounds.ProfileSpline.Curve, Bounds.UCoordinateField, BGCurvePointField.TypeEnum.Float, field => Bounds.UCoordinateField = field);
                    }
                }

            });

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("uCoordinateStart"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("uCoordinateEnd"));

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("swapUV"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("swapNormals"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("vCoordinateScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recalculateNormals"));
        }

        protected override void InternalOnInspectorGUIPost()
        {
            if (paramsChanged) Bounds.UpdateUI();
        }

        protected override void InternalOnUndoRedo()
        {
            Bounds.UpdateUI();
        }
    }
}