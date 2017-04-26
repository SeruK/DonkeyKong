using UnityEngine;
using UE = UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[CreateAssetMenu(menuName = "SA/Donkey Kong/PickupSettings")]
public sealed class PickupSettings : ScriptableObject
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class InstanceData
	{
		public Sprite[] sprites;
	}

	[Serializable]
	class InstanceDatas : EnumItems<Pickup.Kind, InstanceData>
	{
#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(InstanceDatas))]
		class ItemDrawer : Drawer
		{
		}
#endif // UNITY_EDITOR
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	[FormerlySerializedAs("sprites")]
	InstanceDatas instanceDatas;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public Sprite[] GetSprites(Pickup.Kind kind)
	{
		return instanceDatas[kind].sprites;
	}
	#endregion // Methods
}
