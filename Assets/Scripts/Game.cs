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
	Unit player;
	[SerializeField]
	AudioClip music;
	[SerializeField]
	CameraManager cameraManager;
#pragma warning restore 0649
	#endregion // Serialized Fields

	PickupManager pickups;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	protected override void SetupSystems()
	{
		var source = gameObject.AddMissingComponent<AudioSource>();
		source.loop = true;
		source.clip = music;
		source.Play();

		pickups = PickupManager.Setup(settings.pickups);

		player.RegisterCallbacks(
			onTriggerEnter: PlayerCollided
		);
	}

	protected override void ShutdownSystems()
	{
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
		}
	}
	#endregion // Methods
}
