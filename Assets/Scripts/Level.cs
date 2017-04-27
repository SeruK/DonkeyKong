using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Level : MonoBehaviour
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
	public Unit player;
	[SerializeField]
	public Sound music;
	[SerializeField]
	public Vector2 minPos = new Vector2(float.MinValue, float.MinValue);
	[SerializeField]
	public Vector2 maxPos = new Vector2(float.MaxValue, float.MaxValue);
	[SerializeField]
	public string nextScene;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
