using System;
using System.Collections.Generic;

#region dynamic_pool
public class DynamicPool<T> where T : class {
	List<T> free;
	List<T> used;
	
	public delegate T Creater();
	
	public Action<T> onUse;
	public Action<T> onFree;
	
	public readonly Creater creater;
	
	public DynamicPool( Creater creater ) {
		this.creater = creater;
		free = new List<T>();
		used = new List<T>();
	}
	
	public void Add( int items = 10 ) {
		for( int i = 0; i < items; ++i ) {
			T item = creater();
			if( item != null ) {
				free.Add( item );
				if( onFree != null ) {
					onFree( item );
				}
			}
		}
	}

	//test
	public void Add(T item) {
		if( item != null ) {
			free.Add( item );
			if( onFree != null ) {
				onFree( item );
			}
		}
	}
	
	public T Get() {
		T item = null;
		if( free.Count > 0 ) {
			item = free.Pop();
		} else {
			item = creater();
		}
		
		if( item != null ) {
			used.Add( item );
			if( onUse != null ) {
				onUse( item );
			}
		}
		
		return item;
	}
	
	public void Free( T item ) {
		if( IsUsing( item )) {
			used.Remove( item );
			free.Push( item );
			if( onFree != null ) {
				onFree( item );
			}
		}
	}
	
	public bool IsUsing( T item ) {
		return used.Contains( item );
	}

	public bool Contains(T item)
	{
		return free.Contains(item) || used.Contains(item);
	}
	
	public void FreeAll() {
		for( int i = 0; i < used.Count; ++i ) {
			T item = used[ i ];
			//
			free.Push( item );
			if( onFree != null ) {
				onFree( item );
			}
		}
		
		used.Clear();
	}

	// If, f.ex., some external force destroyed the
	// item while inside the pool
	// Removes without running any callbacks
	public bool RemoveUnusable(T item)
	{
		bool removed = used.Remove(item);
		removed |= free.Remove(item);
		return removed;
	}

	public void ExecuteOnUsed( Action<T> callback ) {
		for( int i = 0; i < used.Count; ++i ) {
			callback( used[ i ] );
		}
	}
	
	public void ExecuteOnFree( Action<T> callback ) {
		foreach( var item in free ) {
			callback( item );
		}
	}
	
	public void ExecuteOnAll( Action<T> callback ) {
		ExecuteOnUsed( callback );
		ExecuteOnFree( callback );
	}
}
#endregion

#region global_pool
public static class GlobalPool<T> where T : class, new()
{
	// Using this instead of GlobalPool inheritance to keep
	// Add(), Get() etc. static (would collide with DynamicPool methods)
	private class TypeConstrainedPool<TT> : DynamicPool<TT> where TT : class, new()
	{
		public TypeConstrainedPool(DynamicPool<TT>.Creater creater)
			: base(creater)
		{
#if UNITY_EDITOR
			if(typeof(TT) == typeof(UnityEngine.Object) || typeof(TT).IsSubclassOf(typeof(UnityEngine.Object)))
			{
				throw new Exception("Cannot create a GlobalPool of Unity Objects.");
			}
#endif
		}
	}

	private static readonly TypeConstrainedPool<T> pool = new TypeConstrainedPool<T>(() => new T());

	public static void Add(int items = 10)
	{
		pool.Add(items);
	}

	public static T Get()
	{
		return pool.Get();
	}

	public static void Free(T item)
	{
		pool.Free(item);
	}

	public static bool IsUsing(T item)
	{
		return pool.IsUsing(item);
	}

	public static void FreeAll()
	{
		pool.FreeAll();
	}

	public static void ExecuteOnUsed(Action<T> callback)
	{
		pool.ExecuteOnUsed(callback);
	}

	public static void ExecuteOnFree(Action<T> callback)
	{
		pool.ExecuteOnFree(callback);
	}

	public static void ExecuteOnAll(Action<T> callback)
	{
		pool.ExecuteOnAll(callback);
	}
}
#endregion

#region static_pool
public class StaticPool<T> where T : class {
	Stack<T> free;
	List<T> used;
	
	public delegate T Creater();
	
	public struct Description {
		public Description( int count, Creater creater ) {
			this.creater = creater;
			this.count = count;
			this.outOfObjError = null;
			this.onFree = null;
			this.onUse = null;
		}
		
		public Action<T> onUse;
		public Action<T> onFree;
		public Creater creater;
		public Action outOfObjError;
		public int count;
	}
	
	Description desc;
	
	public StaticPool( Description description ) {
		if( description.creater == null ) {
			throw new ArgumentNullException("description.creater cannot be null");
		}
		
		this.desc = description;
		
		free = new Stack<T>();
		used = new List<T>();
		
		for( int i = 0; i < desc.count; ++i ) {
			T item = desc.creater();
			if( item != null ) {
				free.Push( item );
				if( desc.onFree != null ) {
					desc.onFree( item );
				}
			}
		}
	}
	
	public T Get() {
		T item = null;
		if( free.Count > 0 ) {
			item = free.Pop();
		} else {
			if( desc.outOfObjError != null ) {
				desc.outOfObjError();
			}
			return null;
		}
		
		used.Add( item );
		if( desc.onUse != null ) {
			desc.onUse( item );
		}
		
		return item;
	}
	
	public void Free( T item ) {
		if( IsUsing( item )) {
			used.Remove( item );
			free.Push( item );
			if( desc.onFree != null ) {
				desc.onFree( item );
			}
		}
	}
	
	public bool IsUsing( T item ) {
		return used.Contains( item );
	}
	
	public int Count {
		get { return free.Count; }
	}
	
	public bool IsEmpty {
		get { return free.Count == 0; }
	}
	
	public void FreeAll() {
		for( int i = 0; i < used.Count; ++i ) {
			T item = used[ i ];
			
			free.Push( item );
			if( desc.onFree != null ) {
				desc.onFree( item );
			}
		}
		
		used.Clear();
	}
	
	public void ExecuteOnUsed( Action<T> callback ) {
		for( int i = 0; i < used.Count; ++i ) {
			callback( used[ i ] );
		}
	}
	
	public void ExecuteOnFree( Action<T> callback ) {
		foreach( var item in free ) {
			callback( item );
		}
	}
	
	public void ExecuteOnAll( Action<T> callback ) {
		ExecuteOnUsed( callback );
		ExecuteOnFree( callback );
	}
}
#endregion

