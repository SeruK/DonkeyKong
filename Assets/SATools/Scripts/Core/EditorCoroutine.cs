#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class EditorCoroutine : CoroutineBase<EditorCoroutine>
{
	#region Methods
	public static EditorCoroutine Start(IEnumerator routine)
	{
		return StartInactive(routine);
	}

	static EditorCoroutine()
	{
		EditorApplication.update += UpdateAll;
	}
	#endregion // Methods
}
#endif // UNITY_EDITOR
