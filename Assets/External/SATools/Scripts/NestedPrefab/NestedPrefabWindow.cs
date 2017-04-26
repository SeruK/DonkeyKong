#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class NestedPrefabWindow : EditorWindowBase
{
	#region Types
	#endregion // Types

	#region Fields
	#endregion // Fields

	#region Properties
	protected override string infoString
	{
		get
		{
			return "Contains NestedPrefab-related settings.";
        }
	}
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	[MenuItem("SA/Nested Prefabs")]
	static void MenuOpen()
	{
		Open();
	}

	public static NestedPrefabWindow Open()
	{
		return EditorWindow.GetWindow<NestedPrefabWindow>("Nested Prefabs");
	}

	protected override void Draw()
	{
		NestedPrefab.editorSelectionAllowed = EditorGUILayout.Toggle("Selection Allowed", NestedPrefab.editorSelectionAllowed);
	}
	#endregion // Methods
}
#endif // UNITY_EDITOR