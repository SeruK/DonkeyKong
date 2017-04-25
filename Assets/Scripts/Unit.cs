using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;

public sealed class Unit : MonoBehaviour
{
	#region Types
	#region Serialized Types
#pragma warning disable 0649
	[Serializable]
	class Elements
	{
		[SerializeField]
		public CharacterController2D controller;
		[SerializeField]
		public SpriteAnimator animator;
	}

	[Serializable]
	class Stats
	{
		public float moveSpeed = 3.0f;
		public float moveAcc = 1.0f;
		public float moveDec = 1.0f;

		public float gravityScale = 1.0f;

		public float jumpStrength = 10.0f;
	}

	[Serializable]
	class State
	{
		public Vector2 velocity;
		public bool grounded;
	}
#pragma warning restore 0649
	#endregion // Serialized Types
	#endregion // Types

	#region Fields
	#region Serialized Fields
#pragma warning disable 0649
	[SerializeField]
	Elements elements;
	[SerializeField]
	Stats stats;

	[Space(20.0f)]
	[SerializeField]
	State state;
#pragma warning restore 0649
	#endregion // Serialized Fields
	#endregion // Fields

	#region Properties
	#endregion // Properties

	#region Mono
	#endregion // Mono

	#region Methods
	public void DoUpdate()
	{
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		bool jump = Input.GetButtonDown("Jump");

		if(Mathf.Abs(input.x) > 0.0f)
		{
			int sign = Math.Sign(input.x);

			if(sign == 1 && state.velocity.x < stats.moveSpeed)
			{
				float diffX = input.x * stats.moveAcc * Time.deltaTime;
				state.velocity.x += diffX;
			}
			else if(sign == -1 && state.velocity.x > -stats.moveSpeed)
			{
				float diffX = input.x * stats.moveAcc * Time.deltaTime;
				state.velocity.x += diffX;
			}
		}
		else
		{
			float diffX = stats.moveDec * Time.deltaTime;
			if(diffX > Mathf.Abs(state.velocity.x))
			{
				state.velocity.x = 0.0f;
			}
			else
			{
				state.velocity.x = -Mathf.Sign(state.velocity.x) * diffX;
			}
		}

		state.velocity += (Physics2D.gravity * Time.deltaTime) * stats.gravityScale;

		if(jump)
		{
			state.velocity.y += stats.jumpStrength;
		}

		elements.controller.move(state.velocity * Time.deltaTime);

		state.grounded = elements.controller.isGrounded;

		if(state.grounded) { state.velocity.y = 0.0f; }
	}
	#endregion // Methods
}
