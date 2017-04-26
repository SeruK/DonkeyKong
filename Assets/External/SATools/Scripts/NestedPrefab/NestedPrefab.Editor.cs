#if UNITY_EDITOR
using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public partial class NestedPrefab : MonoBehaviour,
	ISerializationCallbackReceiver
{
	#region Fields
	public static bool editorSelectionAllowed
	{
		get { return EditorPrefs.GetBool("NestedPrefab.selectionAllowed", defaultValue: false); }
		set { EditorPrefs.SetBool("NestedPrefab.selectionAllowed", value); }
	}

	static bool editorSelectionChanging;
	#endregion // Fields

	#region Methods
	static NestedPrefab()
	{
		Selection.selectionChanged += OnSelectionChanged;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowGui;
    }

	static void OnSelectionChanged()
	{
		if(editorSelectionChanging) { return; }

		editorSelectionChanging = true;

		bool dirty = false;
		UE.Object[] results = null;

		if(!NestedPrefab.editorSelectionAllowed)
		{
			using(var selectedObjs = TempList<UE.Object>.Get())
			{
				selectedObjs.buffer.AddRange(Selection.objects);

				for(int i = selectedObjs.Count - 1; i >= 0; --i)
				{
					var go = selectedObjs[i] as GameObject;
					if(go == null) { continue; }

					var nested = go.GetComponentInParent<NestedPrefab>();
					if(nested == null) { continue; }

					dirty = true;

					nested = FindRootNested(nested);

					selectedObjs.RemoveAt(i);
					if(!selectedObjs.buffer.Contains(nested.gameObject))
					{
						selectedObjs.buffer.Add(nested.gameObject);
					}
				}

				results = selectedObjs.buffer.ToArray();
            }
        }

		if(dirty)
		{
			EditorCoroutine.Start(SelectionRoutine(results));
		}
		else
		{
			editorSelectionChanging = false;
		}
    }

	static IEnumerator SelectionRoutine(UE.Object[] results)
	{
		yield return null;

		Selection.objects = results;

		EditorApplication.RepaintHierarchyWindow();

		editorSelectionChanging = false;
	}

	static void OnHierarchyWindowGui(int instanceID, Rect selectionRect)
	{
		var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

		if(go == null) { return; }

		using(var nesteds = TempList<NestedPrefab>.Get())
		{
			go.GetComponentsInParent<NestedPrefab>(includeInactive: true, results: nesteds.buffer);

			bool foundAnyParent = false;

			for(int i = 0; i < nesteds.Count; ++i)
			{
				var nested = nesteds[i];
				if(nested.gameObject != go)
				{
					foundAnyParent = true;
					break;
				}
			}

			if(!foundAnyParent) { return; }
		}

		bool selected = -1 != Array.IndexOf(Selection.instanceIDs, instanceID);
		if(selected) { return; }

		var c = GUI.color;
		var newC = Color.green;
		newC.a = 0.3f;
		GUI.color = newC;
		GUI.DrawTexture(selectionRect, Texture2D.whiteTexture);
		GUI.color = c;
	}

	void OnValidate()
	{
		bool invalidPrefab =
			!Application.isPlaying &&
			prefab != null &&
			(!EditorUtility.IsPersistent(prefab) ||
			PrefabUtility.GetPrefabParent(PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject)) == prefab);

		if(invalidPrefab)
		{
			Dbg.LogErrorRelease(this, "{0} had an invalid prefab", this);
			prefab = null;
		}
	}

	#region ISerializationCallbackReceiver 
	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		EditorCoroutine.Start(
			EditorAwakeRoutine()
		);
	}
	#endregion // ISerializationCallbackReceiver

	IEnumerator EditorAwakeRoutine()
	{
		yield return null;

		if(this == null) { yield break; }

		if(EditorApplication.isPlayingOrWillChangePlaymode)
		{
			yield break;
		}

		if(EditorUtility.IsPersistent(gameObject))
		{
			yield break;
		}

		Awake();
	}

	void EditorRevertLocalChanges()
	{
		if(instantiated == null || prefab == null)
		{
			return;
		}

		PrefabUtility.RevertPrefabInstance(instantiated);

		//PrefabType pfType = PrefabUtility.GetPrefabType(instantiated);

		//if(pfType != PrefabType.PrefabInstance) { return; }

		//PrefabUtility.ReplacePrefab(instantiated, prefab, ReplacePrefabOptions.Default);

		// TODO
		//instantiated.transform.parent = null;

		//instantiated = PrefabUtility.ConnectGameObjectToPrefab(
		//	instantiated,
		//	prefab
		//);

		//bool reverted = PrefabUtility.RevertPrefabInstance(instantiated);

		//Dbg.Log("Reverted: {0}", reverted);

		//PrefabUtility.DisconnectPrefabInstance(instantiated);

		//PrefabUtility.RecordPrefabInstancePropertyModifications(instantiated);

		//instantiated.transform.parent = transform;

		//EditorUtility.SetDirty(instantiated);
		//EditorUtility.SetDirty(transform);
	}

	void EditorApplyLocalChanges()
	{
		if(instantiated == null || prefab == null)
		{
			return;
		}

		// TODO
		PrefabUtility.ReplacePrefab(instantiated, prefab, ReplacePrefabOptions.Default);
	}
	#endregion // Methods

	#region Editor
	[CustomEditor(typeof(NestedPrefab))]
	[CanEditMultipleObjects]
	class Editor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Open Settings", GUILayout.Width(100.0f)))
			{
				NestedPrefabWindow.Open();
			}

			if(!serializedObject.isEditingMultipleObjects)
			{
				var nested = (NestedPrefab)target;

				if(!EditorUtility.IsPersistent(nested) && nested.prefab != null && nested.instantiated != null)
				{
					if(GUILayout.Button("Revert", GUILayout.Width(100.0f)))
					{
						nested.EditorRevertLocalChanges();
					}

					if(GUILayout.Button("Apply", GUILayout.Width(100.0f)))
					{
						nested.EditorApplyLocalChanges();
					}
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10.0f);

			base.OnInspectorGUI();
		}
	}
	#endregion // Editor

	#region PostProcessor
	class PostProcessor : AssetPostprocessor
	{
		static bool isPostProcessing;

		static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths
		)
		{
			if(isPostProcessing) { return; }

			isPostProcessing = true;

			bool anyDirty = false;

			using(var dirtyPrefabs = TempList<GameObject>.Get())
			{
				PostProcessAssets(importedAssets, dirtyPrefabs.buffer);

				anyDirty = dirtyPrefabs.Count > 0;

				for(int i = 0; i < dirtyPrefabs.Count; ++i)
				{
					GameObject prefab = dirtyPrefabs[i];

					EditorUtility.SetDirty(prefab);
				}

				UpdateSceneObjects(dirtyPrefabs.buffer);
			}

			if(anyDirty)
			{
				EditorCoroutine.Start(SaveProjectAndWaitForFrame());
				
			}
			else
			{
				isPostProcessing = false;
			}
		}

		static IEnumerator SaveProjectAndWaitForFrame()
		{
			Dbg.LogRelease("Saving project next frame");
			AssetDatabase.SaveAssets();

			yield return null;

			Dbg.LogRelease("Saving project complete");
			isPostProcessing = false;
		}

		static void PostProcessAssets(string[] importedAssets, List<GameObject> dirtyPrefabs)
		{
			for(int i = 0; i < importedAssets.Length; ++i)
			{
				string assetPath = importedAssets[i];
				PostProcessAsset(assetPath, dirtyPrefabs);
			}
		}

		static void PostProcessAsset(string assetPath, List<GameObject> dirtyPrefabs)
		{
			if(!assetPath.EndsWith("prefab")) { return; }

			var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

			if(asset == null) { return; }

			PrefabType pfType = PrefabUtility.GetPrefabType(asset);
			if(pfType != PrefabType.Prefab) { return; }

			using(var nesteds = TempList<NestedPrefab>.Get())
			{
				asset.GetComponentsInChildren<NestedPrefab>(
					includeInactive: true,
					results: nesteds.buffer
				);

				if(nesteds.Count > 0)
				{
					Dbg.LogRelease(asset, "PostProcessing {0} with NestedPrefab(s)", asset);
					dirtyPrefabs.Add(asset);
				}

				for(int i = 0; i < nesteds.Count; ++i)
				{
					var nested = nesteds[i];

					// May have been destroyed by parent
					if(nested == null) { continue; }

					nested = FindRootNested(nested);

					nested.instantiated = null;

					for(int z = nested.transform.childCount - 1; z >= 0; --z)
					{
						Transform child = nested.transform.GetChild(z);
						DestroyImmediate(child.gameObject, allowDestroyingAssets: true);
						Dbg.LogRelease(asset, "Destroying {0} child[{1}] in asset", nested.gameObject.name, z);
					}
				}
			}
		}

		static void UpdateSceneObjects(List<GameObject> dirtyPrefabs)
		{
			if(dirtyPrefabs.Count == 0) { return; }

			using(var sceneNested = TempList<NestedPrefab>.Get())
			{
				NestedPrefab[] nestedArray = GameObject.FindObjectsOfType<NestedPrefab>();
				sceneNested.buffer.AddRange(nestedArray);

				for(int i = 0; i < dirtyPrefabs.Count; ++i)
				{
					GameObject prefab = dirtyPrefabs[i];
					UpdateSceneObjects(prefab, sceneNested.buffer);
				}
			}
		}

		static void UpdateSceneObjects(GameObject prefab, List<NestedPrefab> allSceneObjects)
		{
			for(int i = allSceneObjects.Count - 1; i >= 0; --i)
			{
				NestedPrefab sceneNested = allSceneObjects[i];

				// May be destroyed by a parent
				if(sceneNested == null) { continue; }

				var pfType = PrefabUtility.GetPrefabType(sceneNested);

				if(pfType != PrefabType.PrefabInstance)
				{
					continue;
				}

				var scenePrefabRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(sceneNested.gameObject);
				if(scenePrefabRoot == null) { continue; }

				var scenePrefab = PrefabUtility.GetPrefabParent(scenePrefabRoot) as GameObject;
				if(scenePrefab != prefab) { continue; }

				Transform transform = sceneNested.transform;

				if(transform.childCount == 0) { continue; }

				for(int childIndex = transform.childCount - 1; childIndex >= 0; --childIndex)
				{
					DestroyImmediate(transform.GetChild(childIndex).gameObject);
				}

				// TODO: Not sure if this is needed
				EditorUtility.SetDirty(sceneNested.gameObject);

				// Prefab connection is lost when destroying children
				// Prefab connection was not "knowingly" disconnected
				// since checking PrefabType before, so reconnecting
				// should be safe
				PrefabUtility.ConnectGameObjectToPrefab(scenePrefabRoot, scenePrefab);

				allSceneObjects.RemoveAt(i);
			}
		}
	}
	#endregion // PostProcessor
}
#endif // UNITY_EDITOR