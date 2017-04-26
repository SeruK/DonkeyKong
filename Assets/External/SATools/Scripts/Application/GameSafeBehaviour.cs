using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameSafeBehaviour : MonoBehaviour
{
	#region Overrides
	// The order of these callbacks are guaranteed

	// Always called on Awake (before AtAwake) - should be used for setup that
	// does not rely on another system having started yet
	// Intended for subclasses that other than being GameSafeBehaviour
	// should be considered base classes
	protected virtual void AtPreAwake() { }
	// Always called on Awake (after AtPreAwake) - should be used for setup that
	// does not rely on another system having started yet
	protected virtual void AtAwake() { }
	// This is run after all systems have been started and
	// it is safe to use them
	protected virtual void AtSetup() { }

	// Works as a normal OnEnable but is always run after
	// systems are fully setup, before AtEnable
	// Intended for subclasses that other than being GameSafeBehaviour
	// should be considered base classes
	protected virtual void AtPreEnable() { }
	// Works as a normal OnEnable but is always run after
	// systems are fully setup, after AtPreEnable
	protected virtual void AtEnable() { }

	// Works as a normal OnDisable but will only be run
	// if AtEnable has run first, before AtPostDisable
	protected virtual void AtDisable() { }
	// Works as a normal OnDisable but will only be run
	// if AtEnable has run first, after AtDisable
	// Intended for subclasses that other than being GameSafeBehaviour
	// should be considered base classes
	protected virtual void AtPostDisable() { }

	// Runs either right before systems are shutdown (and hence
	// are no longer safe to use), or right before the object
	// is destroyed - whichever is first
	protected virtual void AtShutdown() { }
	// Called OnDestroy, before AtPostDestroy
	// Like in Awake, should be used to clean up anything that
	// does not rely on systems running
	protected virtual void AtDestroy() { }
	// Called OnDestroy, after AtDestroy
	// Like in Awake, should be used to clean up anything that
	// does not rely on systems running
	// Intended for subclasses that other than being GameSafeBehaviour
	// should be considered base classes
	protected virtual void AtPostDestroy() { }
	#endregion // Overrides

	#region Fields
	// To ensure AtEnable()/AtDisable() always runs after AtSetup()
	bool hasSetup;
	bool ranOnEnable;
	#endregion // Fields

	#region Methods
	protected void Awake()
	{
		try
		{
			AtPreAwake();
			AtAwake();
		}
		finally
		{
			Game.RegisterCallbacks(
				didSetup: OnSetup,
				willShutdown: OnShutdown
			);
		}
	}

	protected void OnSetup()
	{
		// Don't setup twice
		if(hasSetup) { return; }

		hasSetup = true;

		try
		{
			AtSetup();
		}
		finally
		{
			// If the object is not active at this time,
			// OnEnable will run as usual next time it
			// is activated
			if(isActiveAndEnabled) { OnEnable(); }
		}
	}

	protected void OnEnable()
	{
		// Will manually be called once Setup is
		// guaranteed to have been run
		if(!hasSetup) { return; }

		// AtSetup() could already have run OnEnabled()
		// if it went straight from its Awake()
		if(ranOnEnable) { return; }

		ranOnEnable = true;

		AtPreEnable();
		AtEnable();
	}

	protected void OnDisable()
	{
		// Should only run if OnEnable has been
		// run, if Setup has not run then neither
		// has OnEnable
		if(!hasSetup || !ranOnEnable) { return; }

		ranOnEnable = false;

		AtDisable();
		AtPostDisable();
	}

	protected void OnShutdown()
	{
		if(ranOnEnable) { OnDisable(); }

		// Should only run if OnSetup has been run
		if(!hasSetup) { return; }

		hasSetup = false;

		AtShutdown();
	}

	protected void OnDestroy()
	{
		try
		{
			// The object could be destroyed before the game
			// has actually shut down. In this case, ensure
			// that Shutdown happens before Destroy
			if(hasSetup)
			{
				OnShutdown();
			}
		}
		finally
		{
			try
			{
				AtDestroy();
				AtPostDestroy();
			}
			finally
			{
				Game.UnregisterCallbacks(
					didSetup: OnSetup,
					willShutdown: OnShutdown
				);
			}
		}
	}
	#endregion // Methods
}
