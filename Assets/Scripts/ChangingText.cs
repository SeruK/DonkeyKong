using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class ChangingText : MonoBehaviour
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
	TextMesh text;
	[SerializeField]
	string[] texts;
	[SerializeField]
	FloatRange interval = new FloatRange(1.0f, 4.0f);
#pragma warning restore 0649
	#endregion // Serialized Fields

	float next;
	int index;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	protected void Update()
	{
		next -= Time.deltaTime;

		if(next <= 0.0f)
		{
			next = interval.GetRandom();

			text.text = texts[index];

			if(++index >= texts.Length)
			{
				index = 0;
				Application.Quit();
			}
		}
	}
	#endregion // Mono

	#region Methods
	#endregion // Methods
}
