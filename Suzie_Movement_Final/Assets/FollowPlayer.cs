﻿using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	public Vector3 offset;
	public float speed = 10.0f;
	
	private bool attach = true;
	private Vector3 vel;
	private Vector3 targetPos;
	private Transform player;
	private RomanCharState charState;
	
	
	void Awake () 
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		charState = GameObject.FindObjectOfType<RomanCharState>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		targetPos = player.position + offset;

		if (!attach)
		    targetPos.y = transform.position.y;
		
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref vel, speed * Time.deltaTime);

	}
	
	
	private void OnEnable() { RomanCharController.onCharEvent += AttachFollow; }
	private void OnDisable() { RomanCharController.onCharEvent -= AttachFollow; }
	
	private void AttachFollow (GameEvents gEvent)
	{
		if (gEvent == GameEvents.AttachFollow)
			attach = true;
		
		else if (gEvent == GameEvents.DetachFollow)
			attach = false;
	}

}
