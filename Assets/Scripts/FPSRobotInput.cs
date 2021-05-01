﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRobotInput : MonoBehaviour
{
	public bool DisableFocus { get; set; }

	public IRobotController controller;
	public IRobotController follower;
	public bool controllable;
	public bool isTrainingMode;

	GameObject objectiveParent;
	Renderer grid;
	RaycastHit rayHit;
	bool braking;
	float lastMouseTime;
	float hMouseMove;

	void Start ()
	{
		objectiveParent = GameObject.Find ( "Objectives" );
		GameObject gridObject = GameObject.Find ( "Grid_v2" );
		if ( gridObject != null )
			grid = gridObject.GetComponent<Renderer> ();
		if ( grid != null )
			grid.enabled = false;
		controller.PickupProgress = -1;
//		Cursor.lockState = CursorLockMode.Locked;
//		controllable = true;
	}

	void LateUpdate ()
	{
		// check for close app
		if ( Input.GetButtonDown ( "Quit" ) )
		{
			Application.Quit ();
		}

		if ( Input.GetButtonDown ( "Grid" ) )
		{
			if ( grid != null )
				grid.enabled = !grid.enabled;
		}

//		if ( Input.GetKeyDown ( KeyCode.O ) )
//		{
//			if ( objectiveParent != null )
//				objectiveParent.SetActive ( !objectiveParent.activeSelf );
//		}

		if ( DisableFocus )
			return;

		bool tempControl = false;
		if ( !isTrainingMode )
		{
			controllable = tempControl = Input.GetButton ( "Vertical" ) || Input.GetButton ( "Horizontal" );
		}
		
		// check if we're not focused on our robot
		if ( controllable )
		{
			// check for rotation input
			float mouseX = Input.GetAxis ( "Mouse X" );
			float mouseY = Input.GetAxis ( "Mouse Y" );
			float keyH = Input.GetAxis ( "Horizontal" );
			bool isSaving = controller.IsSaving;
			if ( isTrainingMode || tempControl )
			{
				bool isRMBDown = Input.GetMouseButton ( 1 );
				if ( mouseX != 0 && !isRMBDown )
				{
					lastMouseTime = Time.time;
					hMouseMove += 4 * Time.deltaTime * Mathf.Sign ( mouseX );
					hMouseMove = Mathf.Clamp ( hMouseMove, -1f, 1f );
				} else
				if ( Time.time - lastMouseTime > 0.03f )
				{
					hMouseMove = Mathf.MoveTowards ( hMouseMove, 0, Time.deltaTime * 4 );
				}

				float hMove = Mathf.Clamp ( keyH + hMouseMove, -1f, 1f );
				if ( !isSaving )
					controller.Rotate ( hMove );

				if ( isRMBDown )
					controller.RotateCamera ( mouseX, mouseY );
			}
//				controller.RotateCamera ( 0, mouseY );
			else
//			if ( !isTrainingMode )
				controller.RotateCamera ( mouseX, mouseY );

			// check for camera zoom
			float wheel = Input.GetAxis ( "Mouse ScrollWheel" );
			if ( wheel != 0 )
				controller.ZoomCamera ( -wheel );

			// check to reset zoom
			if ( Input.GetMouseButtonDown ( 2 ) )
				controller.ResetZoom ();

			// check for movement input
			if ( controller.allowStrafe )
			{
				Vector3 move = new Vector3 ( Input.GetAxis ( "Horizontal" ), 0, Input.GetAxis ( "Vertical" ) ) * Time.deltaTime;
				move = controller.TransformDirection ( move );
//				controller.Move ( move );
				
			} else
			{
				if ( braking && Input.GetButtonUp ( "Vertical" ) )
				{
					Input.ResetInputAxes ();
				}
					
				float throttle = Input.GetAxis ( "Vertical" );
				if ( throttle != 0 && Mathf.Abs ( controller.Speed ) > 0.01f && Mathf.Sign ( throttle ) != Mathf.Sign ( controller.Speed ) )
					braking = true;
//				else
//					braking = false;
//				if ( braking = true )
				if ( throttle == 0 )
					braking = false;
//				Debug.Log ( "vert: " + throttle + " speed: " + controller.Speed + " vsign " + Mathf.Sign ( throttle ) + " vspd " + Mathf.Sign ( controller.Speed ) + " braking " + braking );

//				Debug.Log ( "braking: " + braking);


				if ( !isSaving )
				{
					if ( controller.allowSprint )
					{
						throttle *= Mathf.Lerp ( 1, controller.sprintMultiplier, Input.GetAxis ( "Sprint" ) );
					}
					if ( braking )
						controller.Move ( 0, Mathf.Abs ( throttle ) );
					else
						controller.Move ( throttle, 0 );
				}
//				controller.Move ( forward );
			}

//			if ( Input.GetKeyDown ( KeyCode.Space ) )
//			{
//				controller.FixedTurn ( 180, 3 );
//			}

			// check for camera switch key
			if ( Input.GetButtonDown ( "Switch Camera" ) )
				controller.SwitchCamera ();

			// check for unfocus input
			if ( Input.GetButtonDown ( "Unfocus" ) )
			{
				Unfocus ();
				return;
			}

//			Ray ray = controller.camera
//			Physics.Raycast (  )
		} else
		{
			if ( !isTrainingMode )
			{
				if ( Input.GetMouseButtonDown ( 1 ) )
					Focus ();
				else
				if ( Input.GetMouseButtonUp ( 1 ) )
					Unfocus ();
				// check for rotation input
				float mouseX = Input.GetAxis ( "Mouse X" );
				float mouseY = Input.GetAxis ( "Mouse Y" );
				if ( Input.GetMouseButton ( 1 ) )
					controller.RotateCamera ( mouseX, mouseY );
				// check for camera zoom
				float wheel = Input.GetAxis ( "Mouse ScrollWheel" );
				if ( wheel != 0 )
					controller.ZoomCamera ( -wheel );

				// check to reset zoom
				if ( Input.GetMouseButtonDown ( 2 ) )
					controller.ResetZoom ();
			}
			braking = false;
		}
		// check for sample pickup
		if ( isTrainingMode )
		{
			if ( Input.GetButtonDown ( "Sample Pickup" ) || Input.GetButtonDown ( "Focus / Pickup" ) )
			{
				if ( controllable && controller.IsNearObjective )
				{
					controller.PickupObjective ( OnPickedUpObjective );
				}
			}
			
			// check for focus input
			if ( Input.GetButtonDown ( "Focus / Pickup" ) )
			{
				Focus ();
			}
			if ( Input.GetButtonDown ( "Unfocus" ) )
			{
//				if ( controllable )
				Unfocus ();
			}
		}
//		if ( Input.GetKeyDown ( KeyCode.Escape ) )
//		{
//			if ( controllable )
//				Unfocus ();
//			else
//				Focus ();
//		}
	}

	public void Focus ()
	{
		Cursor.lockState = CursorLockMode.Locked;
		if ( isTrainingMode )
			controllable = true;
	}

	public void Unfocus ()
	{
		controllable = false;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		controller.Move ( 0, 1 );
		controller.Rotate ( 0 );
	}

	void OnPickedUpObjective (PickupSample objective)
	{
//		follower.CarryObjective ( objective );
	}
}