using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class Game : GameBase
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
	Unit player;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	protected override void SetupSystems()
	{
		base.SetupSystems();
	}

	protected override void ShutdownSystems()
	{
		base.ShutdownSystems();
	}

	public override void AtUpdate()
	{
		player.DoUpdate();
	}
	#endregion // Methods
}
