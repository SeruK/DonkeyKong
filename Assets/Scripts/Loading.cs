using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Loading : MonoBehaviour
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
	[SerializeField]
	Kino.DigitalGlitch digital;
	[SerializeField]
	float loadTime = 5.0f;
	[SerializeField]
	float glitchTime = 6.0f;
	[SerializeField]
	AudioSource source;
	[SerializeField]
	FloatRange sourceFromTo = new FloatRange(0.25f, 0.4f);
	[SerializeField]
	FloatRange sourcePitchFromTo = new FloatRange(1.0f, 2.0f);
#pragma warning restore 0649
	#endregion // Serialized Fields

	float timer;
	float glitchTimer;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void OnEnable()
	{
		App.delayActivatingScene = true;
		timer = loadTime;
		glitchTimer = glitchTime;
	}

	public void Update()
	{
		bool delayScene = true;

		timer -= Time.deltaTime;

		float progress = 0.0f;

		bool movieDone = true;

		if(Movie.shouldBeLoaded)
		{
			movieDone = Movie.texture != null;
		}

		if(App.loadingProgress >= 0.9f && timer <= 0.0f && movieDone)
		{
			if(glitchTimer >= 0.0f)
			{
				glitchTimer -= Time.deltaTime;

				progress = 1.0f - Mathf.Clamp01(glitchTimer / glitchTime);
			}
			else
			{
				delayScene = false;
            }
		}

		digital.intensity = progress;
		source.volume = sourceFromTo.Lerp(progress);
		source.pitch = sourcePitchFromTo.Lerp(progress);

		App.delayActivatingScene = delayScene;
    }
	#endregion // Methods
}
