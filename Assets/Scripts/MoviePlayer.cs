using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class MoviePlayer : GameSafeBehaviour
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
	GUITexture texture;
	[SerializeField]
	string nextScene;
#pragma warning restore 0649
	#endregion // Serialized Fields

	AudioSource source;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	protected override void AtEnable()
	{
		source = gameObject.AddMissingComponent<AudioSource>();
		StartCoroutine(PlayRoutine());
	}

	IEnumerator PlayRoutine()
	{
		if(Movie.texture == null)
		{
			StartCoroutine(Movie.LoadRoutine());
		}

		while(Movie.texture == null)
		{
			yield return null;
		}

		texture.texture = Movie.texture;

		Movie.texture.Stop();

		source.clip = Movie.texture.audioClip;
		source.Play();
		Movie.texture.Play();

		while(Movie.texture.isPlaying)
		{
			yield return null;
		}

		App.LoadScene(nextScene);
	}

	protected void Update()
	{
		texture.pixelInset = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
	}
	#endregion // Methods
}
