using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class SpriteAnimator : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Animation
	{
		public string name;
		public Sprite[] sprites;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	SpriteRenderer rend;
	[SerializeField]
	Animation[] anims;
#pragma warning restore 0649
	#endregion // Serialized Fields

	Dictionary<string, Animation> nameToAnim;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	void UpdateAnimDict()
	{
	}
	#endregion // Methods
}
