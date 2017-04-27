using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Shaker : MonoBehaviour
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
	Vector3 force = Vector3.one;
#pragma warning restore 0649
	#endregion // Serialized Fields

	Vector3 orig;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	protected void OnEnable()
	{
		orig = transform.localPosition;
    }

	protected void OnDisable()
	{
		transform.localPosition = orig;
	}

	protected void Update()
	{
		Vector3 diff = new Vector3(
			force.x == 0.0f ? 0.0f : UE.Random.Range(-force.x, force.x),
			force.y == 0.0f ? 0.0f : UE.Random.Range(-force.y, force.y),
			force.z == 0.0f ? 0.0f : UE.Random.Range(-force.z, force.z)
		);
		transform.localPosition = orig + diff;
    }
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
