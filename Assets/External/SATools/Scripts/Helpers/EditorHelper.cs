#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class EditorHelper
{
	public static string GetTooltipFromAttribute(FieldInfo fieldInfo, string defaultValue)
	{
		object[] attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), inherit: true);
		return attributes.Length == 0 ? defaultValue : ((TooltipAttribute)attributes[0]).tooltip;
	}

	#region Serialized Property
	// Kudoes to/Stolen from Rotorz Reorderable List
	// https://bitbucket.org/rotorz/reorderable-list-editor-field-for-unity
	public static void ResetPropertyValue(SerializedProperty property)
	{
		if(property == null)
			throw new ArgumentNullException("property");

		switch(property.propertyType)
		{
			case SerializedPropertyType.Integer:
				property.intValue = 0;
				break;
			case SerializedPropertyType.Boolean:
				property.boolValue = false;
				break;
			case SerializedPropertyType.Float:
				property.floatValue = 0f;
				break;
			case SerializedPropertyType.String:
				property.stringValue = "";
				break;
			case SerializedPropertyType.Color:
				property.colorValue = Color.black;
				break;
			case SerializedPropertyType.ObjectReference:
				property.objectReferenceValue = null;
				break;
			case SerializedPropertyType.LayerMask:
				property.intValue = 0;
				break;
			case SerializedPropertyType.Enum:
				property.enumValueIndex = 0;
				break;
			case SerializedPropertyType.Vector2:
				property.vector2Value = default(Vector2);
				break;
			case SerializedPropertyType.Vector3:
				property.vector3Value = default(Vector3);
				break;
			case SerializedPropertyType.Vector4:
				property.vector4Value = default(Vector4);
				break;
			case SerializedPropertyType.Rect:
				property.rectValue = default(Rect);
				break;
			case SerializedPropertyType.ArraySize:
				property.intValue = 0;
				break;
			case SerializedPropertyType.Character:
				property.intValue = 0;
				break;
			case SerializedPropertyType.AnimationCurve:
				property.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
				break;
			case SerializedPropertyType.Bounds:
				property.boundsValue = default(Bounds);
				break;
			case SerializedPropertyType.Gradient:
				//!TODO: Amend when Unity add a public API for setting the gradient.
				break;
		}

		if(property.isArray)
		{
			property.arraySize = 0;
		}

		ResetChildPropertyValues(property);
	}

	static void ResetChildPropertyValues(SerializedProperty element)
	{
		if(!element.hasChildren)
			return;

		var childProperty = element.Copy();
		int elementPropertyDepth = element.depth;
		bool enterChildren = true;

		while(childProperty.Next(enterChildren) && childProperty.depth > elementPropertyDepth)
		{
			enterChildren = false;
			ResetPropertyValue(childProperty);
		}
	}

	public static void CopyPropertyValue(SerializedProperty destProperty, SerializedProperty sourceProperty)
	{
		if(destProperty == null)
			throw new ArgumentNullException("destProperty");
		if(sourceProperty == null)
			throw new ArgumentNullException("sourceProperty");

		sourceProperty = sourceProperty.Copy();
		destProperty = destProperty.Copy();

		CopyPropertyValueSingular(destProperty, sourceProperty);

		if(sourceProperty.hasChildren)
		{
			int elementPropertyDepth = sourceProperty.depth;
			while(sourceProperty.Next(true) && destProperty.Next(true) && sourceProperty.depth > elementPropertyDepth)
				CopyPropertyValueSingular(destProperty, sourceProperty);
		}
	}

	private static void CopyPropertyValueSingular(SerializedProperty destProperty, SerializedProperty sourceProperty)
	{
		switch(destProperty.propertyType)
		{
			case SerializedPropertyType.Integer:
				destProperty.intValue = sourceProperty.intValue;
				break;
			case SerializedPropertyType.Boolean:
				destProperty.boolValue = sourceProperty.boolValue;
				break;
			case SerializedPropertyType.Float:
				destProperty.floatValue = sourceProperty.floatValue;
				break;
			case SerializedPropertyType.String:
				destProperty.stringValue = sourceProperty.stringValue;
				break;
			case SerializedPropertyType.Color:
				destProperty.colorValue = sourceProperty.colorValue;
				break;
			case SerializedPropertyType.ObjectReference:
				destProperty.objectReferenceValue = sourceProperty.objectReferenceValue;
				break;
			case SerializedPropertyType.LayerMask:
				destProperty.intValue = sourceProperty.intValue;
				break;
			case SerializedPropertyType.Enum:
				destProperty.enumValueIndex = sourceProperty.enumValueIndex;
				break;
			case SerializedPropertyType.Vector2:
				destProperty.vector2Value = sourceProperty.vector2Value;
				break;
			case SerializedPropertyType.Vector3:
				destProperty.vector3Value = sourceProperty.vector3Value;
				break;
			case SerializedPropertyType.Vector4:
				destProperty.vector4Value = sourceProperty.vector4Value;
				break;
			case SerializedPropertyType.Rect:
				destProperty.rectValue = sourceProperty.rectValue;
				break;
			case SerializedPropertyType.ArraySize:
				destProperty.intValue = sourceProperty.intValue;
				break;
			case SerializedPropertyType.Character:
				destProperty.intValue = sourceProperty.intValue;
				break;
			case SerializedPropertyType.AnimationCurve:
				destProperty.animationCurveValue = sourceProperty.animationCurveValue;
				break;
			case SerializedPropertyType.Bounds:
				destProperty.boundsValue = sourceProperty.boundsValue;
				break;
			case SerializedPropertyType.Gradient:
				//!TODO: Amend when Unity add a public API for setting the gradient.
				break;
		}
	}
	#endregion // Serialized property
}
#endif // UNITY_EDITOR