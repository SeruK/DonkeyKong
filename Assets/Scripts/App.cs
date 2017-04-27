using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed partial class App : AppBase
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Settings
	{
		[SerializeField]
		public SoundSettings sound;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Settings settings;
#pragma warning restore 0649
	#endregion // Serialized Fields

	SoundManager soundManager;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	protected override void AtSetup()
	{
		soundManager = SoundManager.Setup(settings.sound);
    }

	protected override void AtShutdown()
	{
		soundManager.Shutdown();
	}

	protected override void PreinitializeState(AppState appState)
	{
		var game = appState as Game;
		if(game != null)
		{
			game.Preinitialize(
				soundManager
			);
		}
	}

	protected override void AtUpdate()
	{
		soundManager.SystemUpdate();
	}
	#endregion // Methods
}
