﻿using UnityEngine;
using System.Collections;
// Camera used for 3rd person follow. Smoothing is optinal
public class RomanCameraController : MonoBehaviour {
	
	
	//---------------------------------------------------------------------------------------------------------------------------
	// Public Variables
	//---------------------------------------------------------------------------------------------------------------------------	
	public bool smoothing;					// Will the camera smooth its movement?				
	public Vector3 theOffset;					// How much to offset the camera from the follow

	public Transform follow = null;			// Object to follow
	public Transform player = null;			// Object to follow

	[Range(0,20)]
	public float camFollowSpeed = 10.0f;
	//public float camLookAtSpeed = 10.0f;	// How fast the camera lerps to look at the follow

	[Range(0,200)]
	public float orbitSpeed = 10.0f;

	// climbing ------------------------------------------------------
	public float climbSpeedSmooth = 5.0f;

	//---------------------------------------------------------------------------------------------------------------------------
	// Private Variables
	//---------------------------------------------------------------------------------------------------------------------------	

	private Vector3 targetPos = Vector3.zero;
	
	private Vector3 vel;       					// velocity needed for smooth damping the cam's position
	
	private Quaternion targetRotation;
	private Vector3 vecDifference;
	
	private float speed;
	private float rotVel;
	
	//---------------------------------------------------------------------------------------------------------------------------
	// Private Methods
	//---------------------------------------------------------------------------------------------------------------------------	
	
	public enum CamState
	{
		Free,
		Climbing
	}
	
	[HideInInspector]
	public CamState state = CamState.Free;

	//private RomanCharState charState;

	// Use this for initialization
	private void Start () 
	{
		if (follow == null)
			follow = GameObject.FindGameObjectWithTag("Follow").transform;

		if (player == null)
			player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	private void LateUpdate () 
	{
		switch (state)
		{
			case CamState.Free:
				vecDifference = Vector3.Normalize(transform.position - follow.position) * -theOffset.z;

				if (smoothing)
				{
					targetPos = Vector3.Lerp (transform.position, follow.position + vecDifference, camFollowSpeed * Time.deltaTime);
					targetPos.y = Mathf.Lerp (targetPos.y, follow.position.y + theOffset.y, 5.0f * Time.deltaTime);
				}
				else
				{
					targetPos = follow.position + vecDifference;
					targetPos.y = follow.position.y + theOffset.y;
				}
				
				transform.position = targetPos;

				//Smoothly rotate towards the target point.
				targetRotation = Quaternion.LookRotation(follow.position - transform.position);
				
				if (smoothing)
				{
					transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camFollowSpeed * Time.deltaTime);
				}
				else
				{
					transform.rotation = targetRotation;
				}
				
				speed = Mathf.SmoothDamp (speed, InputController.orbitH * 5, ref rotVel, Time.deltaTime);
				transform.RotateAround (follow.position, Vector3.up, speed);
				break;

			case CamState.Climbing:
				
				targetPos = follow.position + follow.forward * theOffset.z;
				if (smoothing)
				{
					transform.position = Vector3.SmoothDamp (transform.position, targetPos, ref vel, climbSpeedSmooth * Time.deltaTime);
					transform.eulerAngles = Vector3.SmoothDamp (transform.eulerAngles, follow.eulerAngles, ref vel, climbSpeedSmooth * Time.deltaTime);
				}
				else
				{
					transform.position = targetPos;
					transform.eulerAngles = follow.eulerAngles;
				}
			
				break;
		}

	}
	
	private void SetState (CamState s)
	{
		state = s;
	}
	
	private void OnEnable ()
	{
		EventManager.onCharEvent += SetCameraMode;
//		EventManager.onInputEvent += SetCameraMode;
	}
	
	private void OnDisable()
	{
		EventManager.onCharEvent -= SetCameraMode;
//		EventManager.onInputEvent -= SetCameraMode;
	}
	
	private void SetCameraMode (GameEvent gEvent)
	{
		if (gEvent == GameEvent.StartClimbing)
		{
			SetState(CamState.Climbing);
		}
		
		if (gEvent == GameEvent.Land)
		{
			print ("camer: stop climbing");
			SetState(CamState.Free);
		}
	}
}





