using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// To avoid random Unity garbage
public static class GarbageCache
{
	#region Fields
	static readonly Dictionary<UE.Object, string> nameLookup = new Dictionary<UE.Object, string>();
	static readonly Dictionary<GameObject, string> tagLookup = new Dictionary<GameObject, string>();
	#endregion // Fields

	#region Methods
	public static string GetName(UE.Object obj)
	{
		string name;
		if(nameLookup.TryGetValue(obj, out name))
		{
			return name;
		}

		name = obj.name;
		nameLookup[obj] = name;
		return name;
	}

	public static string GetTag(GameObject obj)
	{
		string tag;
		if(tagLookup.TryGetValue(obj, out tag))
		{
			return tag;
		}

		tag = obj.tag;
		tagLookup[obj] = tag;
		return tag;
	}

	public static void ClearFor(UE.Object obj)
	{
		nameLookup.Remove(obj);
		if(obj is GameObject) { tagLookup.Remove((GameObject)obj); }
	}

	public static void Clear()
	{
		nameLookup.Clear();
		tagLookup.Clear();
	}
	#endregion // Methods
}
