using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class SoundManager : MonoBehaviour
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

	DynamicPool<SoundInstance> soundPool;

	Dictionary<int, SoundInstance> idToSound;
	List<SoundInstance> playingSounds;

	int idCounter;
	#endregion // Fields

	#region Properties
	public static SoundManager instance
	{
		get;
		private set;
	}
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public static SoundManager Setup(SoundSettings settings)
	{
		instance = new GameObject("SoundManager").AddComponent<SoundManager>();

		instance.idToSound = new Dictionary<int, SoundInstance>();
		instance.playingSounds = new List<SoundInstance>();

		instance.idCounter = 0;

		instance.soundPool = new DynamicPool<SoundInstance>(() =>
		{
			var sound = new GameObject("SFX").AddComponent<SoundInstance>();

			sound.source = sound.gameObject.AddComponent<AudioSource>();

			sound.transform.parent = instance.transform;
			sound.gameObject.SetActive(false);
			
			return sound;
		});

		instance.soundPool.onFree += (SoundInstance sound) =>
		{
			sound.source.Stop();
			sound.source.clip = null;
			sound.id = 0;

			sound.transform.parent = instance.transform;

			sound.gameObject.SetActive(false);
		};

		return instance;
	}

	public void SystemUpdate()
	{
		for(int i = playingSounds.Count - 1; i >= 0; --i)
		{
			var sfx = playingSounds[i];

			if(sfx.timeLeft > 0.0f)
			{
				sfx.timeLeft -= Time.deltaTime;
				if(sfx.timeLeft <= 0.0f)
				{
					RemoveSound(sfx);
				}
			}
		}
	}

	public void Shutdown()
	{
		Destroy(gameObject);
		instance = null;
	}

	#region Interface
	public SoundHandle Play(Sound sound, SoundFlag flags, Transform parent = null, Vector3? worldPos = null)
	{
		if(sound == null) { return new SoundHandle(); }
		if(sound.clips.Length == 0) { return new SoundHandle(); }

		if(!sound.loop && 0 == (flags & SoundFlag.OneShot))
		{
			Dbg.LogWarnOnce(sound, "Tried to play {0} as one shot", sound);
			return new SoundHandle();
		}

		if(sound.loop && 0 == (flags & SoundFlag.Looping))
		{
			Dbg.LogWarnOnce(sound, "Tried to play {0} as looping", sound);
			return new SoundHandle();
		}

		var sfx = GetSfxInst();

		sfx.sound = sound;
		sfx.name = "SFX_" + sound.name;
		if(parent != null)
		{
			sfx.gameObject.transform.parent = parent;
		}

		sfx.transform.position = worldPos ?? (parent == null ? Vector3.zero : parent.position);

		sfx.Setup(sound);
		sfx.source.Play();

		return new SoundHandle(sfx);
	}

	public void Stop(SoundHandle handle)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			RemoveSound(sfx);
		}
    }

	public void SetVolume(SoundHandle handle, float volume)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.SetVolume(volume);
		}
	}

	public void SetPitch(SoundHandle handle, float pitch)
	{
		var sfx = GetSfx(handle);

		if(sfx != null)
		{
			sfx.SetVolume(pitch);
		}
	}

	public void Clear()
	{
		for(int i = playingSounds.Count - 1; i >= 0; --i)
		{
			var sfx = playingSounds[i];

			Destroy(sfx.gameObject);

			soundPool.RemoveUnusable(sfx);
        }

		playingSounds.Clear();
		idToSound.Clear();
	}
	#endregion // Interface

	void RemoveSound(SoundInstance inst)
	{
		playingSounds.Remove(inst);
		idToSound.Remove(inst.id);

		ReturnSfxInst(inst);
	}

	SoundInstance GetSfx(SoundHandle handle)
	{
		return GetSfx(handle.id);
	}

	SoundInstance GetSfx(int id)
	{
		return idToSound.FindOrNull(id);
	}

	SoundInstance GetSfxInst()
	{
		var sfx = soundPool.Get();
		sfx.id = ++idCounter;
		idToSound[sfx.id] = sfx;
		playingSounds.Add(sfx);
        sfx.gameObject.SetActive(true);
		return sfx;
	}

	void ReturnSfxInst(SoundInstance inst)
	{
		soundPool.Free(inst);
	}
	#endregion // Methods
}
