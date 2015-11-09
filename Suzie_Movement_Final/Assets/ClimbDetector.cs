﻿using UnityEngine;
using System.Collections;

public class ClimbDetector : MonoBehaviour {
	
	public PhysicMaterial wallPhysMaterial;
	public PhysicMaterial groundPhysMaterial;
	
	[HideInInspector]
	public bool climbColliderDetected;
	private RomanCharState charState;
	private CapsuleCollider cCollider;
	private Ray ray;
	private RaycastHit hit;
	private float cColliderHeight;
	private int layerMask = 1 << 10;
	private Vector3 raycastOffset = new Vector3 (0, 1f, 0);
	
	private void Start ()
	{
		charState = GetComponent<RomanCharState>();
		cCollider = GetComponent<CapsuleCollider>();
//		cColliderHeight = GetColliderHeight(cCollider);
		this.enabled = false;
	}
	
	private void Update ()
	{
		Debug.DrawRay(transform.position + raycastOffset,  transform.forward * 2, Color.red); 
		
		if (Physics.Raycast (transform.position + raycastOffset, transform.forward, out hit, 2, layerMask))
		{
			
			EventManager.OnDetectEvent(GameEvent.ClimbColliderDetected, hit);
			print ("event sent");
			
			this.enabled = false;

		}
	
	}
	
	// Hook on to Input event
	private void OnEnable () 
	{ 
		EventManager.onCharEvent += Disable;
	}
	private void OnDisable () 
	{ 
		//EventManager.onCharEvent -= Disable;
	}
	
	private void Disable (GameEvent gameEvent)
	{
		if (gameEvent == GameEvent.Jump)
			this.enabled = true;
		
		else if (gameEvent == GameEvent.Land)
			this.enabled = false;
	}
	
//	private void OnCollisionEnter (Collision col)
//	{
//		float dot = Vector3.Dot(col.contacts[0].normal, transform.forward);
//		print(dot);
//		if (dot > -0.5f && dot < 0.5f)
//		{
//			col.collider.material = groundPhysMaterial;
//			print ("Ground");
//		}
//		else
//		{
//			col.collider.material = wallPhysMaterial;
//			print ("Wall");
//		}
//	}
//	
//	private void OnCollisionExit (Collision col)
//	{
//		col.collider.material = groundPhysMaterial;
//	}
	
	
}
