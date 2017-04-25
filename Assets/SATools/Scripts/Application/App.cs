using UnityEngine;
using UE = UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// Interface that should be implemented
// by the proper App class. See App
public abstract class AppBase : MonoBehaviour
{
	#region Properties
	// Should be yielded upon until ALL
	// external systems are setup and
	// App is ready to setup
	protected virtual bool readyForSetup
	{
		get { return true; }
	}

	// Should be yielded upon until any
	// and all other scripts in a loaded
	// scene has finished their business
	// before finding and setting up the
	// next AppState
	protected virtual bool readyForSceneLoad
	{
		get { return true; }
	}

	protected virtual string splashSceneName
	{
		get { return null; }
	}

	protected virtual string loadingSceneName
	{
		get { return null; }
	}
	#endregion // Properties

	#region Methods
	protected virtual void AtSetup() { }
	protected virtual void AtFixedUpdate() { }
	protected virtual void AtUpdate() { }
	protected virtual void AtLateUpdate() { }
	protected virtual void AtPostRender() { }
	protected virtual void AtShutdown() { }

	// Should pass any and all variables and references which
	// are required for setup
	protected virtual void PreinitializeState(AppState appState) { }
	#endregion // Methods
}

// The proper implementation should be a
// partial class implementing the methods
// in AppBase
public sealed partial class App : AppBase
{
	#region Types
	#endregion // Types

	#region Fields
	static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	Stopwatch scopeStopwatch;

	Coroutine setupRoutine;
	bool isSetup;

	[NonSerialized]
	string sceneToLoadNextFrame;
	Coroutine loadSceneRoutine;
	AsyncOperation loadOperation;

	AppState currentAppState;
	#endregion // Fields

	#region Static Properties
	public static bool isLoading
	{
		get;
		private set;
	}

	public static string loadMessage
	{
		get;
		private set;
	}

	public static float loadingProgress
	{
		get
		{
			if(instance == null || !isLoading) { return 1.0f; }

			return instance.loadOperation == null ?
				0.0f :
				instance.loadOperation.progress;
		}
	}

	// Will delay starting scene load until
	// this is true
	public static bool delaySceneLoad
	{
		get;
		set;
	}

	// Will delay activating a loading scene until
	// this is true
	public static bool delayActivatingScene
	{
		get;
		set;
	}

	static App instance
	{
		get;
		set;
	}
	#endregion // Static Properties

	#region Properties
	#endregion // Properties

	#region Mono
	protected void Awake()
	{
		if(instance != null)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;

		// Has to be in root hierarchy for DontDestroyOnLoad
		if(transform.parent != null)
		{
			transform.parent = null;
		}
		DontDestroyOnLoad(gameObject);

		StartCoroutine(PostRenderRoutine());
	}

	protected void OnEnable()
	{
		if(instance != this) { return; }

		setupRoutine = StartCoroutine(SetupRoutine());
    }

	protected void OnDisable()
	{
		if(instance != this) { return; }

		Shutdown();
	}

	protected void OnApplicationQuit()
	{
		if(instance != this) { return; }

		Shutdown();
	}

	void OnDestroy()
	{
		if(instance == this)
		{
			instance = null;
		}
	}

	protected void FixedUpdate()
	{
		if(!isSetup || isLoading) { return; }

		AtFixedUpdate();

		if(currentAppState != null)
		{
			currentAppState.AtFixedUpdate();
		}
	}

	protected void Update()
	{
		if(sceneToLoadNextFrame != null)
		{
			string scene = sceneToLoadNextFrame;
			sceneToLoadNextFrame = null;
			if(loadSceneRoutine != null)
			{
				StopCoroutine(loadSceneRoutine);
			}
			loadSceneRoutine = StartCoroutine(
				LoadSceneRoutine(scene)
			);
		}

		if(!isSetup || isLoading) { return; }

		AtUpdate();

		if(currentAppState != null)
		{
			currentAppState.AtUpdate();
		}
	}

	protected void LateUpdate()
	{
		if(!isSetup || isLoading) { return; }

		AtLateUpdate();

		if(currentAppState != null)
		{
			currentAppState.AtLateUpdate();
		}
	}
	#endregion // Mono

	#region Methods
	#region Lifecycle
	IEnumerator SetupRoutine()
	{
		while(!readyForSetup)
		{
			yield return null;
		}
		setupRoutine = null;
		try
		{
			AtSetup();
			isSetup = true;
		}
		catch(Exception exc)
		{
			Dbg.LogExcRelease(this, exc);
			Dbg.LogRelease(this, "APP: Setup failed");
			UE.Debug.Break();
			setupRoutine = null;
			yield break;
		}

		setupRoutine = null;

		OnSceneLoaded();
	}

	void Shutdown()
	{
		if(setupRoutine != null)
		{
			StopCoroutine(setupRoutine);
			setupRoutine = null;
		}

		if(!isSetup) { return; }

		try
		{
			AtShutdown();
		}
		catch(Exception exc)
		{
			Dbg.LogExcRelease(this, exc);
			Dbg.LogRelease(this, "APP: Shutdown failed");
		}
		// Not going to be any more setup
		isSetup = false;
	}

	IEnumerator PostRenderRoutine()
	{
		while(true)
		{
			yield return waitForEndOfFrame;
			PostRender();
		}
	}

	void PostRender()
	{
		if(!isSetup || isLoading) { return; }

		AtPostRender();

		if(currentAppState != null)
		{
			currentAppState.AtPostRender();
		}
    }
	#endregion // Lifecycle

	#region State Lifecycle
	void FindAndSetupState()
	{
		// Start off just picking a sibling component
		// if present
		currentAppState = GetComponent<AppState>();

		// Then find all in the scene
		AppState[] statesInScene = FindObjectsOfType<AppState>();

		// If a state is found that is not the sibling
		// prioritize that one
		for(int i = 0; i < statesInScene.Length; ++i)
		{
			var state = statesInScene[i];
			if(state != currentAppState)
			{
				currentAppState = state;
				break;
			}
		}

		if(currentAppState == null)
		{
			Dbg.LogError(this, "APP: Unable to find an AppState in scene or as sibling component");
			return;
		}

		Dbg.Log(this, "APP: Starting with state {0}", currentAppState);

		if(currentAppState.transform != transform)
		{
			currentAppState.transform.parent = transform;
		}

		PreinitializeState(currentAppState);

		currentAppState.AtSetup();
    }

	void ShutdownState()
	{
		if(currentAppState != null)
		{
			if(currentAppState.transform == transform)
			{
				currentAppState.transform.parent = null;
			}

			currentAppState.AtShutdown();
			currentAppState = null;
		}
	}
	#endregion // Static Lifecycle

	#region Level Loading
	public static void ReloadScene()
	{
		string currentSceneName = SceneManager.GetActiveScene().name;

		LoadScene(currentSceneName);
	}

	public static void LoadScene(string scene)
	{
		instance.LoadSceneInternal(scene);
	}

	public void LoadSceneInternal(string scene)
	{
		Dbg.LogErrorReleaseIf(isLoading, this, "Tried to start new scene load while already loading scene");
		Dbg.LogErrorReleaseIf(setupRoutine != null, this, "Tried to start new scene load during setup");

		if(isLoading || setupRoutine != null)
		{
			return;
		}

		sceneToLoadNextFrame = scene;
	}

	IEnumerator LoadSceneRoutine(string scene)
	{
		Dbg.LogRelease(this, "APP: Will load scene {0}", scene);

		OnSceneWillUnload();

		isLoading = true;

		BeginLoadingScope("Loading load scene");
		SceneManager.LoadScene(loadingSceneName);

		// The actual loading starts 'next frame'
		yield return null;
		EndLoadingScope();

		Dbg.LogWarnReleaseIf(
			Game.callbackCounter != 0,
			this,
			"APP: Game had {0} callbacks still registered. These will be cleared, but should be properly taken care of.",
			Game.callbackCounter
		);

		Game.ClearCallbacks();

		BeginLoadingScope("Unloading unused assets");
		loadOperation = Resources.UnloadUnusedAssets();
		loadMessage = "Unloading Unused Assets";
		yield return loadOperation;
		loadOperation = null;
		EndLoadingScope();

		// Wait if something wants to fade in or such
		// before loading starts
		while(delaySceneLoad)
		{
			yield return null;
		}

		loadMessage = scene;
		loadOperation = SceneManager.LoadSceneAsync(scene);

		if(loadOperation == null)
		{
			loadSceneRoutine = null;
			loadOperation = null;

			Dbg.LogErrorRelease(this, "APP: Failed to start loading {0}, was probably not added to settings", scene);
			yield break;
		}

		loadOperation.allowSceneActivation = false;

		BeginLoadingScope("Loading scene");
		// AsyncOp will never return isDone if
		// allowSceneActivation is false, so
		// wait until 0.9
		while(loadOperation.progress < 0.9f)
		{
			yield return null;
		}
		EndLoadingScope();

		// Give other stuff a frame to delay
		// the scene
		yield return null;

		while(delayActivatingScene)
		{
			yield return null;
		}

		loadOperation.allowSceneActivation = true;

		// Now wait until isDone is true
		while(!loadOperation.isDone)
		{
			yield return null;
		}

		// Let scene load method piggyback on this 
		// coroutine but still reset variables,
		// a new scene load may be initiated from
		// OnSceneLoaded
		isLoading = false;
		loadSceneRoutine = null;
		loadOperation = null;

		OnSceneLoaded();
	}

	void OnSceneLoaded()
	{
		if(setupRoutine != null)
		{
			Dbg.LogErrorRelease(this, "APP: Tried to start setup routine while in setup routine");
			return;
		}

		setupRoutine = StartCoroutine(
			OnSceneLoadedRoutine()
		);
	}

	IEnumerator OnSceneLoadedRoutine()
	{
		Dbg.LogRelease(this, "APP: Scene loaded");

		while(!readyForSceneLoad)
		{
			yield return null;
		}

		Scene activeScene = SceneManager.GetActiveScene();

		if(activeScene.name == splashSceneName)
		{
			Dbg.LogRelease(this, "APP: Scene was splash");

			// Null here already because after this we're done
			setupRoutine = null;

			for(int buildIndex = 0; buildIndex < SceneManager.sceneCountInBuildSettings; ++buildIndex)
			{
				string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
				bool validSceneName =
					!string.IsNullOrEmpty(sceneName) &&
					sceneName != loadingSceneName &&
					sceneName != splashSceneName
				;
				if(validSceneName)
				{
					Dbg.LogRelease(this, "APP: Found first scene \"{0}\"", sceneName);
					LoadScene(sceneName);
					yield break;
				}
			}

			Dbg.LogErrorRelease(this, "APP: Did not find a first scene to load");
			yield break;
		}

		Dbg.LogRelease(this, "APP: Disabling Gravity");
		Vector3 gravityToRestore = Physics.gravity;
		Physics.gravity = Vector3.zero;

		try
		{
			FindAndSetupState();
		}
		catch(Exception exc)
		{
			Dbg.LogExcRelease(exc);
			Dbg.LogErrorRelease(this, "APP: State setup failed");

			UE.Debug.Break();
		}

		Dbg.LogRelease(this, "APP: Restoring Gravity");
		Physics.gravity = gravityToRestore;
		setupRoutine = null;
	}

	void OnSceneWillUnload()
	{
		Dbg.LogRelease(this, "APP: Scene will unload");

		ShutdownState();
	}
	#endregion // Level Loading

	#region Children
	void Child<T>(ref T assign, T obj) where T : Component
	{
		assign = obj;
		obj.transform.SetParent(transform);
	}

	void Child<T>(T obj) where T : Component
	{
		obj.transform.SetParent(transform);
	}

	T InstantiateAndChild<T>(T prefab) where T : Component
	{
		var go = Instantiate(prefab);
		go.transform.parent = transform;
		return go;
	}

	void DestroyAndNull<T>(ref T obj) where T : Component
	{
		GameObject.Destroy(obj.gameObject);
		obj = null;
	}
	#endregion // Children

	#region Logging
	public bool ShouldSquelchLog(LogType logType, Exception exc, Dbg.Message message)
	{
#if DO_LOGGING
		return false;
#else
		return logType == LogType.Log;
#endif
	}

	[Conditional("DO_LOGGING")]
	void BeginUpdateScope(string name)
	{
		UnityEngine.Profiling.Profiler.BeginSample(name);
	}

	[Conditional("DO_LOGGING")]
	void EndUpdateScope()
	{
		UnityEngine.Profiling.Profiler.EndSample();
	}

	[Conditional("DO_LOGGING")]
	void BeginSetupScope(string name)
	{
		Dbg.LogRelease(this, "APP: Setting up {0}...", name);
		scopeStopwatch = Stopwatch.StartNew();
	}

	[Conditional("DO_LOGGING")]
	void EndSetupScope()
	{
		scopeStopwatch.Stop();
		Dbg.LogRelease(this, "APP: Took {0:00}.{1:000}s", scopeStopwatch.Elapsed.Seconds, scopeStopwatch.Elapsed.Milliseconds);
	}

	[Conditional("DO_LOGGING")]
	void BeginLoadingScope(string msg)
	{
		Dbg.LogRelease(this, "APP: Loading - {0}...", msg);
		scopeStopwatch = Stopwatch.StartNew();
	}

	[Conditional("DO_LOGGING")]
	void EndLoadingScope()
	{
		scopeStopwatch.Stop();
		Dbg.LogRelease(this, "APP: Took {0:00}.{1:000}s", scopeStopwatch.Elapsed.Seconds, scopeStopwatch.Elapsed.Milliseconds);
	}

	[Conditional("DO_LOGGING")]
	void BeginShutdownScope(string name)
	{
		Dbg.LogRelease(this, "APP: Shutting down {0}...", name);
		scopeStopwatch = Stopwatch.StartNew();
	}

	[Conditional("DO_LOGGING")]
	void EndShutdownScope()
	{
		scopeStopwatch.Stop();
		Dbg.LogRelease(this, "APP: Took {0:00}.{1:000}s", scopeStopwatch.Elapsed.Seconds, scopeStopwatch.Elapsed.Milliseconds);
	}
	#endregion // Logging
	#endregion // Methods
}