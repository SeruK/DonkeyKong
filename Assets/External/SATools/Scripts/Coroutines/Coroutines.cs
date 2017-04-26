using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Coroutines : MonoBehaviour
{
	#region Types
	enum StepResult
	{
		Requeue,
		RequeueFixed,
		RequeuePostRender,
		Stopped,
	}

	enum RoutineFlags
	{
		None = 0,
		Nested = 1 << 0,
	}

	struct Routine
	{
		public readonly int id;
		public readonly RoutineFlags flags;
		public readonly IEnumerator enumerator;
		// See Coroutines.Start()
		public readonly Component attachedTo;
		// The last frame this routine was stepped
		// NOTE: PostRender uses this differently
		public int lastFrame;
		// While this is > 0 the routine should not
		// be stepped, and instead this should be
		// decremented
		public float delay;

		public Routine(int id, RoutineFlags flags, IEnumerator enumerator, Component attachedTo)
		{
			this.id = id;
			this.flags = flags;
			this.enumerator = enumerator;
			this.attachedTo = attachedTo;
			this.lastFrame = -1;
			this.delay = -1.0f;
		}
	}

	// Intermediary object used only as return
	// value from WaitForSeconds (Routine struct
	// holds the actual state).
	// See Coroutines.WaitForSeconds()
	class CustomWaitForSeconds
	{
		public bool inUse;
		public float duration;
	}
	#endregion // Types

	#region Static Fields
	static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
	static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
	static readonly CustomWaitForSeconds waitForSeconds = new CustomWaitForSeconds();

	// NOTE: Assigned id:s should always be > 0 since
	//       0 is always considered as "not having an id"
	// NOTE: Static since if started/restarted old ids
	//       should not be considered valid
	static int idCounter;
	#endregion // Static Fields

	#region Fields
	// Set of all running routine ids
	Dictionary<int, Routine> runningRoutines;
	// FIFO queues for each routine type
	// Contains both routines that will
	// be stepped this frame as well as routines
	// that already have been
	List<Routine> updateQueue;
	List<Routine> fixedUpdateQueue;
	List<Routine> postRenderQueue;

	// Running Unity YieldInstructions that
	// are not "natively" supported. Mapped
	// routine id -> Coroutine
	Dictionary<int, Coroutine> runningInterops; 
	#endregion // Fields

	#region Static Properties
	static Coroutines instance;
	#endregion // Static Properties

	#region Properties
	#endregion // Properties

	#region Methods
	#region System
	// Setup needs to be called before any
	// attempts to interact with this class
	// is made.
	public static Coroutines Setup()
	{
		instance = new GameObject("Coroutines").AddComponent<Coroutines>();

		idCounter = 1;

		instance.runningRoutines = new Dictionary<int, Routine>();
		instance.updateQueue = new List<Routine>();
		instance.fixedUpdateQueue = new List<Routine>();
		instance.postRenderQueue = new List<Routine>();
		instance.runningInterops = new Dictionary<int, Coroutine>();

		return instance;
	}

	// Should be called every fixed update
	public void SystemFixedUpdate()
	{
		while(fixedUpdateQueue.Count > 0)
		{
			Routine routine = fixedUpdateQueue[0];

			if(routine.lastFrame == Time.frameCount)
			{
				break;
			}

			fixedUpdateQueue.RemoveAt(0);

			Resolve(ref routine, cameFromPostRender: false);
		}
	}

	// Should be called every normal update
	public void SystemUpdate()
	{
		while(updateQueue.Count > 0)
		{
			Routine routine = updateQueue[0];

			if(routine.lastFrame == Time.frameCount)
			{
				break;
			}

			updateQueue.RemoveAt(0);

			Resolve(ref routine, cameFromPostRender: false);
		}
	}

	// Should be called at the same time a WaitForEndOfFrame
	// would return from its yield, i.e. something like this:
	// IEnumerator PostRenderRoutine()
	// {
	//     while(true)
	//     {
	//         yield return new WaitForEndOfFrame();
	//         Coroutines.SystemPostRender();
	//     }
	// }
	public void SystemPostRender()
	{
		while(postRenderQueue.Count > 0)
		{
			Routine routine = postRenderQueue[0];

			// NOTE: This is intentionally different from other updates
			//       since the post render should actually happen the
			//       same frame it's scheduled
			if(routine.lastFrame > Time.frameCount)
			{
				break;
			}

			postRenderQueue.RemoveAt(0);

			Resolve(ref routine, cameFromPostRender: true);
		}
	}

	public void Shutdown()
	{
		StopAllCoroutines();

		runningRoutines = null;
		updateQueue = null;
		fixedUpdateQueue = null;
		postRenderQueue = null;
		runningInterops = null;
	}
	#endregion // System

	#region Interface
	// Starts a coroutine that is active until stopped
	// or Coroutines.Shutdown() is called
	//
	// The returned CoroutineHandle can be used to
	// check whether the coroutine was actually started
	// and then to interact with it.
	public static CoroutineHandle StartGlobal(IEnumerator routine)
	{
		return instance.StartInternal(routine, attachTo: null);
	}

	// Works like StartGlobal, but will automatically stop if
	// its parent routine is stopped.
	public static CoroutineHandle StartGlobalNested(IEnumerator routine)
	{
		return instance.StartInternal(routine, attachTo: null, flags: RoutineFlags.Nested);
	}

	// Starts a coroutine that is active until stopped,
	// Coroutines.Shutdown() is called or following the
	// same rules for coroutines as those started with
	// Unity:
	// - The component is destroyed
	// - The component's GameObject is deactivated
	// - The component's GameObject is destroyed
	//
	// The returned CoroutineHandle can be used to
	// check whether the coroutine was actually started
	// and then to interact with it.
	public static CoroutineHandle Start(Component attachTo, IEnumerator routine)
	{
		if(attachTo == null) { throw new ArgumentNullException("attachTo"); }

		return instance.StartInternal(routine, attachTo);
	}

	// Works like Start, but will automatically stop if
	// its parent routine is stopped.
	public static CoroutineHandle StartNested(Component attachTo, IEnumerator routine)
	{
		if(attachTo == null) { throw new ArgumentNullException("attachTo"); }

		return instance.StartInternal(routine, attachTo, RoutineFlags.Nested);
	}

	public static void Stop(CoroutineHandle routine)
	{
		instance.StopInternal(routine.id);
	}

	public static bool IsRunning(CoroutineHandle routine)
	{
		return instance.IsRunningInternal(routine.id);
	}

	// Returns an object that can be yielded upon to
	// delay until the following fixed frame.
	// Does not generate any additional garbage.
	public static object WaitForFixedUpdate()
	{
		return waitForFixedUpdate;
	}

	// Returns an object that can be yielded upon to
	// delay until all cameras have rendered.
	// Does not generate any additional garbage.
	// Works the same way as Unity's WaitForEndOfFrame
	public static object WaitForPostRender()
	{
		return waitForEndOfFrame;
	}

	// Returns an object that can be yielded upon to
	// wait until a certain amount of seconds have
	// passed
	// Does not generate any additional garbage.
	// NOTE: Do not under any circumstance store the
	//       return value of this method. Only use
	//       when yielding.
	public static object WaitForSeconds(float seconds)
	{
		if(waitForSeconds.inUse)
		{
			throw new Exception("Tried to use WaitForSeconds while already in use. Did you try to store down the return value of WaitForSeconds()?");
		}

		waitForSeconds.inUse = true;
		waitForSeconds.duration = seconds;

		return waitForSeconds;
	}
	#endregion // Interface

	#region Lifecycle
	CoroutineHandle StartInternal(IEnumerator enumerator, Component attachTo, RoutineFlags flags = RoutineFlags.None)
	{
		if(enumerator == null) { throw new ArgumentNullException("routine"); }

		if(attachTo != null && !attachTo.gameObject.activeInHierarchy)
		{
			Dbg.LogError(
				attachTo,
				"Coroutine couldn't be started because the the game object '{0}' is inactive!",
				GarbageCache.GetName(attachTo.gameObject)
			);

			return new CoroutineHandle();
		}

		var routine = new Routine(
			id: ++idCounter,
			flags: flags,
			enumerator: enumerator,
			attachedTo: attachTo
		);

		bool started = Resolve(ref routine, cameFromPostRender: false);

		return new CoroutineHandle(
			id: started ? routine.id : 0
		);
	}

	void StopInternal(int id)
	{
		Routine? routine = runningRoutines.FindOrNullable(id);
		if(routine == null) { return; }

		runningRoutines.Remove(id);

		RemoveRoutineWithId(id, updateQueue);
		RemoveRoutineWithId(id, fixedUpdateQueue);
		RemoveRoutineWithId(id, postRenderQueue);

		Coroutine interop;
		if(runningInterops.TryGetValue(id, out interop))
		{
			StopCoroutine(interop);
			runningInterops.Remove(id);
		}

		// Stop any running nested coroutine
		object current = routine.Value.enumerator.Current;
		if(current != null && current is CoroutineHandle)
		{
			var handle = (CoroutineHandle)current;
			Routine? nestedRoutine = runningRoutines.FindOrNullable(handle.id);
			
			bool shouldStop =
				nestedRoutine != null &&
				0 != (nestedRoutine.Value.flags & RoutineFlags.Nested);
			if(shouldStop)
			{
				StopInternal(handle.id);
			}
		}
	}

	bool IsRunningInternal(int id)
	{
		return runningRoutines.ContainsKey(id);
	}
	#endregion // Lifecycle

	#region Stepping
	// Steps and requeues a routine
	bool Resolve(ref Routine routine, bool cameFromPostRender)
	{
		// May have been deactivated before step
		if(RoutineGOInactiveOrDestroyed(ref routine))
		{
			StopInternal(routine.id);
			return false;
		}

		StepResult res = Step(ref routine);

		// May have been deactivated during step
		if(RoutineGOInactiveOrDestroyed(ref routine))
		{
			StopInternal(routine.id);
			return false;
		}

		routine.lastFrame = Time.frameCount;

		switch(res)
		{
			case StepResult.Stopped:
			{
				StopInternal(routine.id);
				return false;
			}

			case StepResult.Requeue:
			{
				updateQueue.Add(routine);
				runningRoutines[routine.id] = routine;
				return true;
			}

			case StepResult.RequeueFixed:
			{
				fixedUpdateQueue.Add(routine);
				runningRoutines[routine.id] = routine;
				return true;
			}

			case StepResult.RequeuePostRender:
			{
				// If we came from post render
				// we increment this to avoid
				// endless looping and force
				// it being executed next frame
				if(cameFromPostRender)
				{
					++routine.lastFrame;
				}
				postRenderQueue.Add(routine);
				runningRoutines[routine.id] = routine;
				return true;
			}

			default: { throw new Exception("Unhandled StepResult"); }
		}
	}

	// Steps a routine and returns what should happen
	// to it next
	StepResult Step(ref Routine routine)
	{
		object current = routine.enumerator.Current;

		// Handle nested coroutines
		if(current is CoroutineHandle)
		{
			if(IsRunning((CoroutineHandle)current))
			{
				return StepResult.Requeue;
			}
		}
		else if(current is YieldInstruction)
		{
			if(current is AsyncOperation)
			{
				var asOp = (AsyncOperation)current;
				if(!asOp.isDone)
				{
					return StepResult.Requeue;
				}
			}
			else if(runningInterops.ContainsKey(routine.id))
			{
				return StepResult.Requeue;
			}
		}
		else if(current is CustomYieldInstruction)
		{
			// NOTE: Does not precisely mimic Unity's behavior
			//       since current is not queried start of update
			//       and end of update
			var asYieldInstr = (CustomYieldInstruction)current;
			if(asYieldInstr.keepWaiting)
			{
				return StepResult.Requeue;
			}
		}
		else if(current is CustomWaitForSeconds)
		{
			if(routine.delay > 0.0f)
			{
				routine.delay -= Time.deltaTime;
				return StepResult.Requeue;
			}
		}

		if(!routine.enumerator.MoveNext())
		{
			return StepResult.Stopped;
		}

		current = routine.enumerator.Current;

		// Make sure that if a CustomWaitForSeconds was returned
		// that it is immediately used to free the instance up
		if(current is CustomWaitForSeconds)
		{
			var asWait = (CustomWaitForSeconds)current;

			routine.delay = asWait.duration;
			asWait.inUse = false;
		}
		else if(current is YieldInstruction)
		{
			if(current is WaitForFixedUpdate)
			{
				return StepResult.RequeueFixed;
			}
			else if(current is WaitForEndOfFrame)
			{
				return StepResult.RequeuePostRender;
			}
			else
			{
				Dbg.LogWarnOnce(
					Dbg.Context(current.GetType(), this),
					"Unsupported YieldInstruction type {0}. It will be started using Unity's normal coroutines and have some additional performance overhead. This message will only log once.",
					current.GetType()
				);

				runningInterops[routine.id] = StartCoroutine(
					InteropRoutine(routine.id, (YieldInstruction)current)
				);
			}
		}
		else if(current is IEnumerator)
		{
			throw new Exception("Returning an IEnumerator is not supported. Start a separate coroutine instead.");
		}

		return StepResult.Requeue;
	}
	#endregion // Stepping

	#region Helpers
	IEnumerator InteropRoutine(int routineId, YieldInstruction instruction)
	{
		yield return instruction;
		runningInterops.Remove(routineId);
	}

	static bool RemoveRoutineWithId(int id, List<Routine> routines)
	{
		for(int i = 0; i < routines.Count; ++i)
		{
			Routine r = routines[i];
			if(r.id == id)
			{
				routines.RemoveAt(i);
				return true;
			}
		}

		return false;
	}

	static bool RoutineGOInactiveOrDestroyed(ref Routine routine)
	{
		return
			// Was attached but has been deactivated
			(routine.attachedTo != null && !routine.attachedTo.gameObject.activeInHierarchy) ||
			// Was attached but has been destroyed
			(routine.attachedTo == null && !ReferenceEquals(routine.attachedTo, null));
	}
	#endregion // Helpers
	#endregion // Methods
}
