using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct FloatRange
{
	public float min;
	public float max;

	public FloatRange(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public float GetRandom()
	{
		return UE.Random.Range(min, max);
	}

	public bool Contains(float value)
	{
		return min <= value && value <= max;
	}

	public float Lerp(float value)
	{
		return Mathf.Lerp(min, max, value);
	}

	// What normalized value at value
	public float InverseLerp(float value)
	{
		return Mathf.InverseLerp(min, max, value);
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FloatRange))]
public class FloatRangeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var fieldPositions = EditorGUI.PrefixLabel(position, label);

		const float spacing = 5f;
		const float labelWidth = 28f;
		float valueWidth = (fieldPositions.width - labelWidth * 2f - spacing * 3) / 2f;

		Rect labelMinPos = fieldPositions;
		labelMinPos.width = labelWidth;

		Rect field1Pos = fieldPositions;
		field1Pos.x = labelMinPos.xMax + spacing;
		field1Pos.width = valueWidth;


		Rect labelMaxPos = fieldPositions;
		labelMaxPos.x = field1Pos.xMax + spacing;
		labelMaxPos.width = labelWidth;

		Rect field2Pos = fieldPositions;
		field2Pos.x = labelMaxPos.xMax + spacing;
		field2Pos.width = valueWidth;

		var min = property.FindPropertyRelative("min");
		var max = property.FindPropertyRelative("max");

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		EditorGUI.LabelField(labelMinPos, new GUIContent("min"));
		EditorGUI.LabelField(labelMaxPos, new GUIContent("max"));

		min.floatValue = EditorGUI.FloatField(field1Pos, min.floatValue);
		max.floatValue = EditorGUI.FloatField(field2Pos, max.floatValue);

		EditorGUI.indentLevel = indent;
	}
}
#endif // UNITY_EDTOR
