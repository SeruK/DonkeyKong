using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public sealed class Pickup : GameSafeBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Elements
	{
		public SpriteAnimator animator;
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
		BananaBunch,
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
#if UNITY_EDITOR
	protected void OnValidate()
	{
		if(!Application.isPlaying)
		{
			var editorSettings = AssetDatabase.LoadAssetAtPath<PickupSettings>("Assets/Core/PickupSettings.asset");
			if(editorSettings != null && elements.animator != null && elements.animator.rend != null)
			{
				Sprite[] sprites = editorSettings.GetSprites(kind);
				if(sprites != null && sprites.Length > 0 && sprites[0] != null)
				{
					elements.animator.rend.sprite = sprites[0];
				}
			}
		}
	}
#endif // UNITYEDITOR
#endregion // Mono

	#region Methods
	protected override void AtEnable()
	{
		PickupManager.instance.Register(this);
	}

	protected override void AtDisable()
	{
		PickupManager.instance.Unregister(this);
	}

	public void Setup(PickupSettings settings)
	{
		Sprite[] sprites = settings.GetSprites(kind);
		if(sprites != null)
		{
			elements.animator.AddAnimation("Idle", looping: true, sprites: settings.GetSprites(kind));
			elements.animator.PlayAnimation("Idle");
		}
	}
	#endregion // Methods
}
