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
	[Serializable]
	class Settings
	{
		public PickupSettings pickups;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Settings settings;
	[SerializeField]
	CameraManager cameraManager;
	[SerializeField]
	Sound coin;
#pragma warning restore 0649
	#endregion // Serialized Fields

	[NonSerialized]
	PickupManager pickups;
	[NonSerialized]
	SoundManager soundManager;

	[NonSerialized]
	Unit player;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void Preinitialize(
		SoundManager soundManager
	)
	{
		this.soundManager = soundManager;
	}

	protected override void SetupSystems()
	{
		pickups = PickupManager.Setup(settings.pickups);

		var level = FindObjectOfType<Level>();

		Dbg.LogErrorIf(level == null, "No level found");

		if(level != null)
		{
			SetupWithLevel(level);
		}
	}

	void SetupWithLevel(Level level)
	{
		soundManager.Play(level.music, SoundFlag.Looping);

		player = level.player;

		cameraManager.followUnit = player;
		cameraManager.min = level.minPos;
		cameraManager.max = level.maxPos;

		player.RegisterCallbacks(
			onTriggerEnter: PlayerCollided
		);
	}

	protected override void ShutdownSystems()
	{
		soundManager.Clear();

		player.UnregisterCallbacks(
			onTriggerEnter: PlayerCollided
		);

		pickups.Shutdown();
		pickups = null;
	}

	public override void AtUpdate()
	{
		player.DoUpdate();

		SpriteAnimator.SystemUpdate();
	}

	public override void AtLateUpdate()
	{
		cameraManager.SystemLateUpdate();
	}

	void PlayerCollided(Collider2D coll)
	{
		var pickup = coll.GetComponent<Pickup>();

		if(pickup != null)
		{
			pickup.gameObject.SetActive(false);
			soundManager.Play(coin, SoundFlag.OneShot, worldPos: pickup.transform.position);
		}
	}
	#endregion // Methods
}
