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
		public class Persistent
		{
			public Vector2 velocity;
			public bool grounded;
			public int direction = 1;
		}

		public class Momentary
		{
			public bool jumped;

			public void Reset()
			{
				jumped = false;
			}
		}

		public Persistent persistent = new Persistent();
		public Momentary momentary = new Momentary();
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
		state.momentary.Reset();

		UpdateMovement();
		UpdateAnimation();
	}

	void UpdateMovement()
	{
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		bool jump = Input.GetButtonDown("Jump");

		if(Mathf.Abs(input.x) > 0.0f)
		{
			int sign = Math.Sign(input.x);

			state.persistent.direction = sign;

			if(sign == 1 && state.persistent.velocity.x < stats.moveSpeed)
			{
				float diffX = input.x * stats.moveAcc * Time.deltaTime;
				state.persistent.velocity.x += diffX;
			}
			else if(sign == -1 && state.persistent.velocity.x > -stats.moveSpeed)
			{
				float diffX = input.x * stats.moveAcc * Time.deltaTime;
				state.persistent.velocity.x += diffX;
			}
		}
		else
		{
			float diffX = stats.moveDec * Time.deltaTime;
			if(diffX > Mathf.Abs(state.persistent.velocity.x))
			{
				state.persistent.velocity.x = 0.0f;
			}
			else
			{
				state.persistent.velocity.x = -Mathf.Sign(state.persistent.velocity.x) * diffX;
			}
		}

		state.persistent.velocity += (Physics2D.gravity * Time.deltaTime) * stats.gravityScale;

		if(jump && state.persistent.grounded)
		{
			state.momentary.jumped = true;
			state.persistent.velocity.y += stats.jumpStrength;
		}

		elements.controller.move(state.persistent.velocity * Time.deltaTime);

		state.persistent.grounded = elements.controller.isGrounded;

		if(state.persistent.grounded) { state.persistent.velocity.y = 0.0f; }
	}

	void UpdateAnimation()
	{
		SpriteAnimator anim = elements.animator;

		if(state.momentary.jumped)
		{
			anim.PlayAnimation("Jump");
		}

		if(!state.persistent.grounded)
		{
			anim.SetDefaultAnimation("Fall");
		}
		else if(Mathf.Abs(state.persistent.velocity.x) > 0.0f)
		{
			anim.SetDefaultAnimation("Run");
		}
		else
		{
			anim.SetDefaultAnimation("Idle");
		}

		anim.SetDirection(state.persistent.direction);
	}
	#endregion // Methods
}
