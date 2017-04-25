using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class CoroutineBase<T>
	where
	T : CoroutineBase<T>,
	new()
{
	#region Types
	#endregion // Types

	#region Static Fields
	protected static readonly List<T> activeRoutines = new List<T>();
	protected static readonly List<T> inactiveRoutines = new List<T>();
	#endregion // Static Fields

	#region Fields
	IEnumerator routine;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	protected static T StartInactive(IEnumerator routine)
	{
		if(inactiveRoutines.Count == 0)
		{
			inactiveRoutines.Add(new T());
		}

		var coroutine = (T)inactiveRoutines[0];

		coroutine.StartInternal(routine);

		return coroutine;
	}

	protected static void UpdateAll()
	{
		for(int i = activeRoutines.Count - 1; i >= 0; --i)
		{
			var routine = (T)activeRoutines[i];
			routine.Update();
		}
	}

	protected virtual void StartInternal(IEnumerator routine)
	{
		if(this.routine != null) { throw new Exception("Coroutine was already started"); }

		this.routine = routine;
		inactiveRoutines.Remove((T)this);
		activeRoutines.Add((T)this);
	}

	public virtual void Stop()
	{
		if(this.routine == null) { throw new Exception("Coroutine was not started"); }

		this.routine = null;
		inactiveRoutines.Add((T)this);
		activeRoutines.Remove((T)this);
	}

	protected virtual void Update()
	{
		if(!routine.MoveNext())
		{
			Stop();
		}
	}
	#endregion // Methods
}
