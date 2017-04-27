using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class MoveFromTo : GameSafeBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Transform trans;
	[SerializeField]
	Vector3 from;
	[SerializeField]
	Vector3 to;
	[SerializeField]
	float duration = 10.0f;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	protected override void AtEnable()
	{
		StartCoroutine(Routine());
    }

	IEnumerator Routine()
	{
		float t = 0.0f;
		while(t < duration)
		{
			trans.position = Vector3.Lerp(from, to, t / duration);
			t += Time.deltaTime;
			yield return null;
		}
	}
	#endregion // Methods
}
