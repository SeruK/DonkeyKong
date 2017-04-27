using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct SoundHandle
{
	public readonly int id;
	public readonly Sound sound;

	public bool hasValue
	{
		get { return id != 0; }
	}

	public SoundHandle(SoundInstance sfx)
	{
		id = sfx.id;
		sound = sfx.sound;
	}
}

public enum SoundFlag
{
	OneShot = 1 << 0,
	Looping = 1 << 1,
}

public sealed class SoundInstance : MonoBehaviour
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
#pragma warning restore 0649
	#endregion // Serialized Fields

	public int id;
	public Sound sound;
	public AudioSource source;

	public float volume;
	public float pitch;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void Setup(Sound sound)
	{
		this.sound = sound;
		this.volume = sound.volume.GetRandom();
		this.pitch = sound.pitch.GetRandom();

		this.source.clip = sound.clips.RandomItem();
		this.source.loop = sound.loop;

		SetVolume(1.0f);
		SetPitch(1.0f);
	}

	public void SetVolume(float value)
	{
		source.volume = volume * value;
	}

	public void SetPitch(float value)
	{
		source.pitch = pitch * value;
	}
	#endregion // Methods
}
