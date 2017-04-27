using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "SA/Donkey Kong/Game Sounds")]
public sealed class GameSounds : ScriptableObject
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	public class PickupData
	{
		[SerializeField]
		public Sound banana;
		[SerializeField]
		public Sound bananaBunch;
		[SerializeField]
		public Sound letter;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	public PickupData pickups;
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
