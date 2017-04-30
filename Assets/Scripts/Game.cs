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

		[SerializeField]
		public string reloadSceneName;
		[SerializeField]
		public float reloadSceneTime = 20.0f;
	}

	[Serializable]
	class Prefabs
	{
		public CameraManager camera;
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
	Prefabs prefabs;
	[SerializeField]
	GameSounds sounds;
#pragma warning restore 0649
	#endregion // Serialized Fields

	[NonSerialized]
	PickupManager pickups;
	[NonSerialized]
	SoundManager soundManager;
	[NonSerialized]
	CameraManager cameraManager;

	Level level;
	[NonSerialized]
	Unit player;
	[NonSerialized]
	string loadScene;
	[NonSerialized]
	SoundHandle musicSfx;
	[NonSerialized]
	float noInputTimer;
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

		cameraManager = FindObjectOfType<CameraManager>();

		if(cameraManager == null)
		{
			cameraManager = Instantiate(prefabs.camera);
		}

		level = FindObjectOfType<Level>();

		Dbg.LogErrorIf(level == null, "No level found");

		if(level != null)
		{
			SetupWithLevel(level);
		}

		noInputTimer = settings.reloadSceneTime;
	}

	void SetupWithLevel(Level level)
	{
		musicSfx = soundManager.Play(level.music, SoundFlag.Looping);

		player = level.player;

		cameraManager.followUnit = player;
		cameraManager.min = level.minPos;
		cameraManager.max = level.maxPos;

		cameraManager.backgroundRend.sprite = level.background;

		if(player != null)
		{
			player.RegisterCallbacks(
				onTriggerEnter: PlayerCollided
			);

			player.SetDirection(level.startDir);
		}
	}

	protected override void ShutdownSystems()
	{
		musicSfx = new SoundHandle();
        soundManager.Clear();

		if(player != null)
		{
			player.UnregisterCallbacks(
				onTriggerEnter: PlayerCollided
			);
		}

		pickups.Shutdown();
		pickups = null;

		level = null;
	}

	public override void AtUpdate()
	{
		bool hadInput = false; ;

		if(player != null)
		{
			player.DoUpdate(out hadInput);
        }

		if(level != null && level.restartIfNoInput)
		{
			if(hadInput)
			{
				noInputTimer = settings.reloadSceneTime;
			}

			noInputTimer -= Time.deltaTime;

			if(noInputTimer <= 0.0f)
			{
				loadScene = settings.reloadSceneName;
			}
		}

		SpriteAnimator.SystemUpdate();

		if(loadScene != null)
		{
			string load = loadScene;
			loadScene = null;
			App.LoadScene(load);
		}
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
			Sound sound = null;

			if(pickup.kind == Pickup.Kind.Banana)
			{
				sound = sounds.pickups.banana;
			}
			else if(pickup.kind == Pickup.Kind.BananaBunch)
			{
				sound = sounds.pickups.bananaBunch;
			}
			else
			{
				sound = sounds.pickups.letter;
			}

			soundManager.Play(sound, SoundFlag.OneShot, worldPos: pickup.transform.position);
		}

		var sceneTrigger = coll.GetComponent<SceneTrigger>();

		if(sceneTrigger != null)
		{
			if(sceneTrigger.shouldPreloadMovie)
			{
				Movie.shouldBeLoaded = true;
				Coroutines.StartGlobal(Movie.LoadRoutine());
			}
			loadScene = sceneTrigger.sceneName;
		}
	}
	#endregion // Methods
}
