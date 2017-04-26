using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class PickupManager : MonoBehaviour
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

	PickupSettings settings;
	#endregion // Fields

	#region Static Properties
	public static PickupManager instance
	{
		get;
		private set;
	}
	#endregion // Static Properties

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	#region System
	public static PickupManager Setup(
		PickupSettings settings
	)
	{
		instance = new GameObject("PickupManager").AddComponent<PickupManager>();

		instance.settings = settings;

		return instance;
	}

	public void Shutdown()
	{
		instance = null;
	}

	public void Register(Pickup pickup)
	{
		pickup.Setup(settings);
	}

	public void Unregister(Pickup pickup)
	{
	}
	#endregion // System
	#endregion // Methods
}
