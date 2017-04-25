using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
	#region Methods
	public static V? FindOrNullable<K, V>(this Dictionary<K, V> dict, K key)
		where V : struct
	{
		return dict.ContainsKey(key) ? dict[key] : (V?)null;
	}
	#endregion // Methods
}
