using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct CoroutineHandle
{
	public readonly int id;

	// This value is false if the coroutine was
	// not started
	public bool hasValue
	{
		get { return id > 0; }
	}

	public CoroutineHandle(int id)
	{
		this.id = id;
	}
}
