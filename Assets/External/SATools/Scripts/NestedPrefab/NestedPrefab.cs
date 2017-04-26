using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public partial class NestedPrefab : MonoBehaviour
{
	#region Types
	[Flags]
	enum InheritFlags
	{
		None = 0,
		StaticFlags = 1 << 0,
		Tag = 1 << 1,
		Layer = 1 << 2,
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	GameObject prefab;

	// What flags should all children inherit?
	[SerializeField]
	InheritFlags inheritFlags = InheritFlags.StaticFlags;

	[SerializeField]
	protected GameObject instantiated;
#pragma warning restore 0649
	#endregion // Serialized Fields

	bool isSpawning;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	#region Mono
	protected void Awake()
	{
		if(instantiated == null)
		{
			Spawn();
		}
	}
	#endregion // Mono

	// Spawn breakdown:
	// - Move to top of NestedPrefab hierarchy
	// - Enqueue top node
	// - - Instantiate node
	// - - Apply flags on each child
	// - - Enqueue each NestedPrefab child
	// - - Repeat
	[ContextMenu("Spawn")]
	public void Spawn()
	{
		if(instantiated != null || isSpawning) { return; }

		NestedPrefab rootParent = FindRootNested(this);

		if(rootParent != null && rootParent != this)
		{
			rootParent.Spawn();
			return;
		}

		isSpawning = true;

		//Dbg.LogRelease(this, "Spawning from {0}", this);

		using(var queue = TempList<NestedPrefab>.Get())
		using(var previouslyInstantiated = TempList<GameObject>.Get())
		{
			queue.Add(this);

			while(queue.Count > 0)
			{
				NestedPrefab prefab = queue[0];
				queue.RemoveAt(0);
				SpawnInternal(
					prefab,
					queue.buffer,
					previouslyInstantiated.buffer
				);
			}
		}

		isSpawning = false;
	}

	static NestedPrefab FindRootNested(NestedPrefab prefabInstance)
	{
		NestedPrefab rootParent = prefabInstance;

		// Find a parent, any will do (that is not us,
		// GetComponentsInParent includes the same GameObject)
		using(var parents = TempList<NestedPrefab>.Get())
		{
			while(true)
			{
				var thisParent = rootParent;

				rootParent.GetComponentsInParent<NestedPrefab>(
					includeInactive: true,
					results: parents.buffer
				);

				for(int i = parents.Count - 1; i >= 0; --i)
				{
					var parent = parents[i];
					if(parent != thisParent)
					{
						rootParent = parent;
						break;
					}
				}

				// No new parent found
				if(rootParent == thisParent)
				{
					break;
				}
			}
		}

		return rootParent;
	}

	static void SpawnInternal(
		NestedPrefab prefabInstance,
		List<NestedPrefab> queue,
		List<GameObject> previouslyInstantiated
	)
	{
		GameObject instantiated = prefabInstance.InstantiateSelf(previouslyInstantiated);

		if(instantiated == null) { return; }

		prefabInstance.Setup();

		// BUG: If not set dirty here .instantiated is not properly saved
#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			EditorUtility.SetDirty(prefabInstance);
			EditorUtility.SetDirty(prefabInstance.gameObject);
		}
#endif // UNITY_EDITOR

		// Enqueue all child NestedPrefab
		prefabInstance.GetComponentsInChildren<NestedPrefab>(
			includeInactive: true,
			result: queue
		);

		queue.Remove(prefabInstance);
		// TODO: Fix this
		//// Remove all NestedPrefab that shares the
		//// same prefab (this includes the prefab itself
		//// and any child that may be nested)
		//for(int i = queue.Count - 1; i >= 0; --i)
		//{
		//	NestedPrefab iter = queue[i];
		//	if(iter.prefab == prefabInstance.prefab)
		//	{
		//		Dbg.LogErrorIf(
		//			iter != prefabInstance,
		//			iter,
		//			"{0} was already instantiated in this hierarchy. It will not be instantiated to avoid recursion.",
		//			iter.prefab
		//		);
		//		queue.RemoveAt(i);
		//	}
		//}
	}

	protected void Setup()
	{
		ApplyFlagsRecursive();

		AtSetup();
	}

	protected virtual void AtSetup()
	{
	}

	void ApplyFlagsRecursive()
	{
		ApplyFlagsRecursive(
			inheritFlags,
			gameObject.layer,
			tag: (0 != (inheritFlags & InheritFlags.Tag)) ? gameObject.tag : null,
			staticFlags:
#if UNITY_EDITOR
					(0 != (inheritFlags & InheritFlags.StaticFlags)) && !Application.isPlaying ?
					(int)GameObjectUtility.GetStaticEditorFlags(gameObject) :
#endif // UNITY_EDITOR
						0,
			trans: instantiated.transform
		);
	}

	static void ApplyFlagsRecursive(
		InheritFlags inheritFlags,
		int layer,
		string tag,
		int staticFlags,
		Transform trans
	)
	{
		bool dirty = false;

		GameObject to = trans.gameObject;

		if(0 != (inheritFlags & InheritFlags.Layer))
		{
			if(to.layer != layer)
			{
				dirty = true;
				to.layer = layer;
			}
		}

		if(0 != (inheritFlags & InheritFlags.Tag))
		{
			if(!to.CompareTag(tag))
			{
				dirty = true;
				to.tag = tag;
			}
		}

#if UNITY_EDITOR
		if(0 != (inheritFlags & InheritFlags.StaticFlags))
		{
			var flags = (StaticEditorFlags)staticFlags;
			var currFlags = GameObjectUtility.GetStaticEditorFlags(to);

			if(flags != currFlags)
			{
				dirty = true;
				GameObjectUtility.SetStaticEditorFlags(to, flags);
			}
		}

		if(dirty)
		{
			EditorUtility.SetDirty(to);
		}
#endif // UNITY_EDITOR

		for(int i = 0; i < trans.childCount; ++i)
		{
			Transform child = trans.GetChild(i);
			ApplyFlagsRecursive(
				inheritFlags,
				layer,
				tag,
				staticFlags,
				child
			);
		}
	}

	GameObject InstantiateSelf(List<GameObject> previouslyInstantiated)
	{
		if(instantiated != null)
		{
			return instantiated;
		}

		if(prefab == null) { return null; }

		if(previouslyInstantiated.Contains(prefab))
		{
			Dbg.LogError(
				this,
				"{0} was already instantiated in this hierarchy. It will not be instantiated to avoid recursion.",
				this
			);
			return null;
		}

		if(prefab == gameObject)
		{
			Dbg.LogError(
				this,
				"{0} had itself as prefab. It will not be instantiated to avoid recursion.",
				this
			);
			return null;
		}

		previouslyInstantiated.Add(prefab);

#if UNITY_EDITOR
		if(EditorApplication.isPlayingOrWillChangePlaymode)
		{
			instantiated = GameObject.Instantiate(prefab);

		}
		else
		{
			instantiated = PrefabUtility.InstantiatePrefab(
				prefab,
				destinationScene: gameObject.scene
			) as GameObject;
		}
#else
		instantiated = GameObject.Instantiate(prefab);
#endif // UNITY_EDITOR

		if(instantiated != null)
		{
			instantiated.transform.parent = transform;
			instantiated.transform.localPosition = Vector3.zero;
			instantiated.transform.localRotation = Quaternion.identity;
			instantiated.transform.localScale = Vector3.one;
		}

		instantiated.name = prefab.name;

		return instantiated;
	}
	#endregion // Methods
}
