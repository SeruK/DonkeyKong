using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
	#region Array
	public static T[] Fill<T>(this T[] array, T value)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = value;
		}

		return array;
	}

	public static T[] FillDefault<T>(this T[] array)
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = default(T);
		}

		return array;
	}

	public static T[] FillNew<T>(this T[] array)
		where T : new()
	{
		for(int i = 0; i < array.Length; ++i)
		{
			array[i] = new T();
		}

		return array;
	}
	#endregion // Array

	#region List
	// Stack-like (LIFO)
	public static void Push<T>(this List<T> list, T item)
	{
		list.Add(item);
	}

	// Stack-like (LIFO)
	public static T Pop<T>(this List<T> list)
	{
		var item = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return item;
	}

	// Queue-like (FIFO)
	public static void Enqueue<T>(this List<T> list, T item)
	{
		list.Add(item);
	}

	// Queue-like (FIFO)
	public static T Dequeue<T>(this List<T> list)
	{
		var item = list[0];
		list.RemoveAt(0);
		return item;
	}

	public static T RandomItem<T>(this IList<T> list)
	{
		if(list.Count == 0)
		{
			return default(T);
		}
		return list[UE.Random.Range(0, list.Count)];
	}

	public static T RandomItem<T>(this T[] array)
	{
		if(array.Length == 0)
		{
			return default(T);
		}
		return array[UE.Random.Range(0, array.Length)];
	}

	public static T TakeRandomItem<T>(this IList<T> list)
	{
		if(list.Count == 0) { return default(T); }

		int index = UE.Random.Range(0, list.Count);
		T val = list[index];
		list.RemoveAt(index);
		return val;
	}
	#endregion // List

	#region Dictionary
	public static Value FindOrDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			return defaultValue;
		}
	}

	public static Value FindOrCall<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, System.Func<Value> defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			return defaultValue();
		}
	}

	public static Value FindOrAddDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			dictionary.Add(key, defaultValue);
			return defaultValue;
		}
	}

	public static Value FindOrAddNew<Key, Value>(this Dictionary<Key, Value> dictionary, Key key)
		where Value : new()
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			Value v = new Value();
			dictionary.Add(key, v);
			return v;
		}
	}

	public static Value FindOrAddDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, System.Func<Value> defaultValue)
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			var def = defaultValue();
			dictionary.Add(key, def);
			return def;
		}
	}

	public static Value? FindOrNullable<Key, Value>(this Dictionary<Key, Value> dictionary, Key key) where Value : struct
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return (Value?)value;
		}
		else
		{
			return (Value?)null;
		}
	}

	public static Value FindOrNull<Key, Value>(this Dictionary<Key, Value> dictionary, Key key) where Value : class
	{
		Value value;
		if(dictionary.TryGetValue(key, out value))
		{
			return value;
		}
		else
		{
			return null;
		}
	}
	#endregion // Dictionary

	#region GameObject
	public static T AddMissingComponent<T>(this GameObject go)
		where T : Component
	{
		var obj = go.GetComponent<T>();
		if(obj == null) { obj = go.AddComponent<T>(); }
		return obj;
	}
	#endregion // GameObject
}
