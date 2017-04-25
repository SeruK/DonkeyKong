using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class ArrayExtensions
{
	#region Methods
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
	#endregion // Methods
}
