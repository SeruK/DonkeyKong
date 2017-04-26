using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class TempList<T>
{
	#region Fields
	static Impl[] buffers = new Impl[3].FillNew();
	#endregion // Fields

	#region Methods
	public static Impl Get()
	{
		for(int i = 0; i < buffers.Length; ++i)
		{
			Impl impl = buffers[i];
			if(!impl.inUse) { return impl.StartUsing(); }
		}

		throw new Exception("No temp buffer available");
	}

	public static void Return(Impl impl)
	{
		impl.buffer.Clear();
	}
	#endregion // Methods

	public class Impl : IDisposable
	{
		public readonly List<T> buffer = new List<T>();

		public bool inUse
		{
			get;
			private set;
		}

		public int Count
		{
			get { return buffer.Count; }
		}

		public Impl StartUsing()
		{
			if(inUse) { throw new Exception("Tried to use buffer already in use"); }

			inUse = true;
			return this;
		}

		public void StopUsing()
		{
			if(!inUse) { throw new Exception("Tried to return buffer not in use"); }

			buffer.Clear();
			inUse = false;
		}

		public void Dispose()
		{
			if(inUse)
			{
				StopUsing();
			}
		}

		public T this[int index]
		{
			get { return buffer[index]; }
			set { buffer[index] = value; }
		}

		public void Add(T item)
		{
			buffer.Add(item);
		}

		public bool Remove(T item)
		{
			return buffer.Remove(item);
		}

		public void RemoveAt(int index)
		{
			buffer.RemoveAt(index);
		}
	}
}
