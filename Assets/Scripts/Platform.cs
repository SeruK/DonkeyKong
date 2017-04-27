using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Platform : MonoBehaviour
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
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	protected void OnDrawGizmos()
	{
		var collider = GetComponent<BoxCollider2D>();
		if(collider == null) { return; }

		GizmosHelper.Scope(() =>
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(collider.offset, collider.size);
		});
	}
	#endregion // Methods
}
