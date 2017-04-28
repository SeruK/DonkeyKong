using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class Movie
{
	#region Types
	#endregion // Types

	#region Fields
	public static bool shouldBeLoaded;

	[NonSerialized]
	public static MovieTexture texture;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public static IEnumerator LoadRoutine()
	{
		var www = new WWW("file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "DonkeyKong.ogg"));
		if(!string.IsNullOrEmpty(www.error))
		{
			Dbg.LogError(www.error);
			yield break;
		}

		var movie = www.movie;
		while(!movie.isReadyToPlay)
		{
			yield return null;
		}

		texture = movie;
	}
	#endregion // Methods
}
