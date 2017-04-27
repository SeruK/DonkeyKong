using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class CameraManager : MonoBehaviour
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
	public float speed = 0.5f;

#pragma warning restore 0649
	#endregion // Serialized Fields

	[NonSerialized]
	public Unit followUnit;
	[NonSerialized]
	public Vector2 min = new Vector2(float.MinValue, float.MinValue);
	[NonSerialized]
	public Vector2 max = new Vector2(float.MaxValue, float.MaxValue);
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void Setup()
	{
	}

	public void Shutdown()
	{
	}

	public void SystemLateUpdate()
	{
		Vector3? targetPos = null;

		if(followUnit != null)
		{
			targetPos = followUnit.transform.position;
		}

		if(targetPos != null)
		{
			MoveTowardsTargetPos(targetPos.Value);
		}
	}

	void MoveTowardsTargetPos(Vector3 targetPos)
	{
		targetPos.x = Mathf.Clamp(targetPos.x, min.x, max.x);
		targetPos.y = Mathf.Clamp(targetPos.y, min.y, max.y);

		transform.position = Vector3.Lerp(transform.position, targetPos, speed);
	}
	#endregion // Methods
}
