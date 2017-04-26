#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class EditorWindowBase : EditorWindow
{
	#region Types
	#endregion // Types

	#region Fields
	Vector2 gui_scrollPos;
	#endregion // Fields

	#region Properties
	protected virtual string infoString
	{
		get { return null; }
	}
	#endregion // Properties

	#region Mono
	protected void OnGUI()
	{
		gui_scrollPos = EditorGUILayout.BeginScrollView(gui_scrollPos);

		if(infoString != null)
		{
			EditorGUILayout.HelpBox(infoString, MessageType.Info);
		}

		Draw();

		EditorGUILayout.EndScrollView();
	}
	#endregion // Mono

	#region Methods
	protected abstract void Draw();
	#endregion // Methods
}
#endif // UNITY_EDITOR