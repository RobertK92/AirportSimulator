using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(AI.UtilityAgent))]
public class UtilityAgentEditor : Editor
{
    public AI.UtilityAgent Target { get; private set; }

    private void OnEnable()
    {
        Target = (AI.UtilityAgent)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(string.Format("Thirst: {0}", Target.Thirst));
        EditorGUILayout.LabelField(string.Format("Hunger: {0}", Target.Hunger));
        EditorGUILayout.LabelField(string.Format("Toilet: {0}", Target.Toilet));
        EditorGUILayout.LabelField(string.Format("Boarding: {0}", Target.BoardPlane));
        EditorGUILayout.HelpBox("The rates represent the amount of value gained in 1 minute for every stat (this does not reflect utility).", MessageType.Info); 
        base.OnInspectorGUI();
        GUIStyle style = GUIStyle.none;
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;
        
        if(Target.CurrentGoal != AI.LongTermGoal.None) 
            EditorGUILayout.LabelField(string.Format("I need to {0}!", Target.CurrentGoal.ToString()), style);
        else
            EditorGUILayout.LabelField(string.Format("I don't need to do anything", Target.CurrentGoal.ToString()), style);
    }
}
