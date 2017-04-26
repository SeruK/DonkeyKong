using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Pickup : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Elements
	{
		public SpriteRenderer renderer;
	}
#pragma warning restore 0649
	#endregion // Serialized Types

	public enum Kind
	{
		Banana,
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I,
		J,
		K,
		L,
		M,
		N,
		O,
		P,
		Q,
		R,
		S,
		T,
		U,
		V,
		W,
		X,
		Y,
		Z,
	}
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Elements elements;
	[SerializeField]
	public Kind kind;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void Setup(PickupSettings settings)
	{
		elements.renderer.sprite = settings.GetSprite(kind);
	}
	#endregion // Methods
}
