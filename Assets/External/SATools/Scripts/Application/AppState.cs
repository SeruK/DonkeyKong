using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// A base class for a specific state of the
// app, such as the Game state or the MainMenu
public abstract class AppState : MonoBehaviour
{
	// All methods are called by App
	public virtual void AtSetup() { }
	public virtual void AtFixedUpdate() { }
	public virtual void AtUpdate() { }
	public virtual void AtLateUpdate() { }
	public virtual void AtPostRender() { }
	public virtual void AtShutdown() { }
}
