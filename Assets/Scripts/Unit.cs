using UnityEngine;
using UE = UnityEngine;
using UnityEngine.Serialization;
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
	class Definition
	{
		public float moveSpeed = 3.0f;
		public float moveAcc = 1.0f;
		public float moveDec = 1.0f;

		public float gravityScale = 1.0f;

		public float jumpStrength = 10.0f;
	}

	[Serializable]
	class Elements
	{
		[SerializeField]
		public CharacterController2D controller;
		[SerializeField]
		public SpriteAnimator animator;
	}

	[Serializable]
	class State
	{
		public class Persistent
		{
			public Vector2 velocity;
			public bool grounded;
			public int direction = 1;
			public bool canJump;
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
	[FormerlySerializedAs("stats")]
	Definition definition;

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
	#region Controller Callbacks
	public void RegisterCallbacks(
        Action<RaycastHit2D> onControllerCollided = null,
		Action<Collider2D> onTriggerEnter = null,
		Action<Collider2D> onTriggerStay = null,
		Action<Collider2D> onTriggerExit = null
	)
	{
		if(onControllerCollided != null)
		{
			elements.controller.onControllerCollidedEvent += onControllerCollided;
		}

		if(onTriggerEnter != null)
		{
			elements.controller.onTriggerEnterEvent += onTriggerEnter;
		}

		if(onTriggerStay != null)
		{
			elements.controller.onTriggerStayEvent += onTriggerStay;
		}

		if(onTriggerExit != null)
		{
			elements.controller.onTriggerExitEvent += onTriggerExit;
		}
	}

	public void UnregisterCallbacks(
		Action<RaycastHit2D> onControllerCollided = null,
		Action<Collider2D> onTriggerEnter = null,
		Action<Collider2D> onTriggerStay = null,
		Action<Collider2D> onTriggerExit = null
	)
	{
		if(onControllerCollided != null)
		{
			elements.controller.onControllerCollidedEvent -= onControllerCollided;
		}

		if(onTriggerEnter != null)
		{
			elements.controller.onTriggerEnterEvent -= onTriggerEnter;
		}

		if(onTriggerStay != null)
		{
			elements.controller.onTriggerStayEvent -= onTriggerStay;
		}

		if(onTriggerExit != null)
		{
			elements.controller.onTriggerExitEvent -= onTriggerExit;
		}
	}
	#endregion // Controller Callback

	public void SetDirection(int dir)
	{
		state.persistent.direction = dir;
	}

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

			if(sign == 1 && state.persistent.velocity.x < definition.moveSpeed)
			{
				float diffX = input.x * definition.moveAcc * Time.deltaTime;
				state.persistent.velocity.x += diffX;
			}
			else if(sign == -1 && state.persistent.velocity.x > -definition.moveSpeed)
			{
				float diffX = input.x * definition.moveAcc * Time.deltaTime;
				state.persistent.velocity.x += diffX;
			}
		}
		else
		{
			float diffX = definition.moveDec * Time.deltaTime;
			if(diffX > Mathf.Abs(state.persistent.velocity.x))
			{
				state.persistent.velocity.x = 0.0f;
			}
			else
			{
				state.persistent.velocity.x = -Mathf.Sign(state.persistent.velocity.x) * diffX;
			}
		}

		state.persistent.velocity += (Physics2D.gravity * Time.deltaTime) * definition.gravityScale;

		if(jump && state.persistent.canJump)
		{
			state.momentary.jumped = true;
			state.persistent.canJump = false;
			state.persistent.velocity.y += definition.jumpStrength;
		}

		elements.controller.move(state.persistent.velocity * Time.deltaTime);

		state.persistent.grounded = elements.controller.isGrounded;

		if(state.persistent.grounded)
		{
			state.persistent.canJump = true;
			state.persistent.velocity.y = 0.0f;
		}
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
