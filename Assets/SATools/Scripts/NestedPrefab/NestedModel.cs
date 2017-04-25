using UnityEngine;
using UE = UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class NestedModel : NestedPrefab
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class RendererData
	{
		[SerializeField]
		public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
		[SerializeField]
		public bool receiveShadows = true;

		[Space(5.0f)]
		[SerializeField]
		public bool overrideMaterials;
		[SerializeField]
		public Material[] materials;
		[SerializeField]
		public bool useSharedMaterial;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	AnimatorUpdateMode animationUpdateMode = AnimatorUpdateMode.Normal;
	[SerializeField]
	AnimatorCullingMode animationCullingMode = AnimatorCullingMode.CullCompletely;
	[SerializeField]
	RuntimeAnimatorController controller;

	[Space(5.0f)]
	[SerializeField]
	RendererData[] rendererData = new RendererData[1];
#pragma warning restore 0649
	#endregion // Serialized Fields

	[NonSerialized]
	Renderer[] renderers;
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Methods
	protected override void AtSetup()
	{
		SetupAnimator();
		SetupRenderers();
	}

	void SetupAnimator()
	{
		var animator = GetComponentInChildren<Animator>();

		if(controller == null)
		{
			DestroyComponent(animator);
		}
		else
		{
			if(animator == null)
			{
				animator = instantiated.AddComponent<Animator>();
			}

			animator.runtimeAnimatorController = controller;
			animator.updateMode = animationUpdateMode;
			animator.cullingMode = animationCullingMode;
		}
	}

	void SetupRenderers()
	{
		using(var renderers = TempList<Renderer>.Get())
		{
			GetComponentsInChildren<Renderer>(
				includeInactive: true,
				result: renderers.buffer
			);

			this.renderers = renderers.buffer.ToArray();

			if(rendererData.Length == 0)
			{
				return;
			}

			int dataIndex = 0;

			for(int i = 0; i < renderers.Count; ++i)
			{
				Renderer rend = renderers[i];

				if(!(rend is MeshRenderer) && !(rend is SkinnedMeshRenderer))
				{
					continue;
				}

				if(dataIndex >= rendererData.Length)
				{
					break;
				}

				RendererData data = rendererData[dataIndex++];

				rend.shadowCastingMode = data.shadowCastingMode;
				rend.receiveShadows = data.receiveShadows;

				if(Application.isPlaying && data.overrideMaterials)
				{
					if(data.useSharedMaterial)
					{
						rend.sharedMaterials = data.materials;
					}
					else
					{
						rend.materials = data.materials;
					}
				}
			}
		}
	}

	void DestroyComponent<T>(T obj) where T : UE.Object
	{
#if UNITY_EDITOR
		if(Application.isPlaying)
		{
			Destroy(obj);
		}
		else
		{
			DestroyImmediate(obj);
		}
#else
		Destroy(obj);
#endif // UNITY_EDITOR
	}
	#endregion // Methods
}
