using System;
using System.Collections;
using System.Collections.Generic;

public static class EnumHelper<T> where T: struct, IConvertible
{
	public static readonly int count;
	public static readonly T[] values;
	public static readonly int[] intValues;
	public static readonly string[] names;

	static EnumHelper()
	{
		Type type = typeof(T);
		if(type.IsEnum)
		{
			values = (T[])Enum.GetValues(type);
			count = values.Length;
			names = Enum.GetNames(type);
			intValues = new int[count];
			for(int i = 0; i < count; ++i)
			{
				intValues[i] = Convert.ToInt32(values[i]);
			}
		}
		else
		{
			Dbg.LogErrorRelease("Used EnumHelper<T> with a type that isn't a enum");
			count = 0;
			values = null;
			intValues = null;
			names = null;
		}
	}

	public static int IndexOf(T value, IEqualityComparer<T> comparer = null)
	{
		if(comparer == null) { comparer = EqualityComparer<T>.Default; }

		for(int i = 0; i < values.Length; ++i)
		{
			T val = values[i];
			if(comparer.Equals(value, val))
			{
				return i;
			}
		}

		return -1;
	}
}
