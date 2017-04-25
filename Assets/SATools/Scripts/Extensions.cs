using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
	#region Methods
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
	#endregion // Methods
}
