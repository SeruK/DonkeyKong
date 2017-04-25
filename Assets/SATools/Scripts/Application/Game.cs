using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Interface that should be implemented
// by the proper Game class. See Game
public abstract class GameBase : AppState
{
	#region Types
	public enum RestoreResult
	{
		NoSave,
		HadSave,
	}
	#endregion // Types

	#region Methods
	protected virtual void SetupSystems() { }
	protected virtual RestoreResult RestoreSystems() { return RestoreResult.NoSave; }
	protected virtual void ShutdownSystems() { }
	#endregion // Methods
}

// A special AppState implementation
// designated for the main Game. Has
// some convenient interfaces already
// set up
// The proper implementation should be a
// partial class implementing the methods
// in GameBase and AppState
public sealed partial class Game : GameBase
{
	#region Types
	public delegate void WillSetupHandler();
	public delegate void DidSetupHandler();
	public delegate void DidRestoreHandler(RestoreResult result);
	public delegate void WillShutdownHandler();
	public delegate void DidShutdownHandler();
	#endregion // Types

	#region Static
	static Game instance
	{
		get;
		set;
	}

	static event WillSetupHandler onWillSetupSystems;
	static event DidSetupHandler onDidSetupSystems;
	static event DidRestoreHandler onDidRestoreSystem;
	static event WillShutdownHandler onWillShutdownSystems;
	static event DidShutdownHandler onDidShutdownSystems;
	#endregion // Static

	#region Fields
	public static int callbackCounter;

	RestoreResult? restoreResult = null;
	#endregion // Fields

	#region Properties
	public static bool isSetup
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Static Interface
	public static void RegisterCallbacks(
		WillSetupHandler willSetup = null,
		DidSetupHandler didSetup = null,
		DidRestoreHandler didRestore = null,
		WillShutdownHandler willShutdown = null,
		DidShutdownHandler didShutdown = null
	)
	{
		if(willSetup != null) { onWillSetupSystems += willSetup; }
		if(didSetup != null) { onDidSetupSystems += didSetup; }
		if(didRestore != null) { onDidRestoreSystem += didRestore; }
		if(willShutdown != null) { onWillShutdownSystems += willShutdown; }
		if(didShutdown != null) { onDidShutdownSystems += didShutdown; }

#if !IS_RELEASE
		++callbackCounter;
#endif // !IS_RELEASE

		if(instance == null) { return; }

		if(isSetup)
		{
			if(willSetup != null) { willSetup(); }
			if(didSetup != null) { didSetup(); }
		}

		if(instance.restoreResult != null)
		{
			if(didRestore != null) { didRestore(instance.restoreResult.Value); }
		}
	}

	public static void UnregisterCallbacks(
		WillSetupHandler willSetup = null,
		DidSetupHandler didSetup = null,
		DidRestoreHandler didRestore = null,
		WillShutdownHandler willShutdown = null,
		DidShutdownHandler didShutdown = null
	)
	{
#if !IS_RELEASE
		--callbackCounter;
#endif // !IS_RELEASE

		if(willSetup != null) { onWillSetupSystems -= willSetup; }
		if(didSetup != null) { onDidSetupSystems -= didSetup; }
		if(didRestore != null) { onDidRestoreSystem -= didRestore; }
		if(willShutdown != null) { onWillShutdownSystems -= willShutdown; }
		if(didShutdown != null) { onDidShutdownSystems -= didShutdown; }
	}

	public static void ClearCallbacks()
	{
		callbackCounter = 0;
		onWillSetupSystems = null;
		onDidSetupSystems = null;
		onDidRestoreSystem = null;
		onWillShutdownSystems = null;
		onDidShutdownSystems = null;
	}
	#endregion

	#region Mono
	#endregion // Mono

	#region Methods
	public override void AtSetup()
	{
		if(isSetup) { return; }

		instance = this;

		if(onWillSetupSystems != null) { onWillSetupSystems(); }

		SetupSystems();

		isSetup = true;

		if(onDidSetupSystems != null) { onDidSetupSystems(); }

		restoreResult = RestoreSystems();

		if(onDidRestoreSystem != null) { onDidRestoreSystem(restoreResult.Value); }
	}

	public override void AtShutdown()
	{
		if(!isSetup) { return; }

		if(onWillShutdownSystems != null) { onWillShutdownSystems(); }

		ShutdownSystems();

		isSetup = false;
		instance = null;

		if(onDidShutdownSystems != null) { onDidShutdownSystems(); }
	}
	#endregion // Methods
}