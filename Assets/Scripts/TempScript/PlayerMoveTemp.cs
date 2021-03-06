﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveTemp : MonoBehaviour
{
	public float MoveSpeed = 5f;
    public AudioSource Walk;

    private Rigidbody m_rigidBody;
    private Ray rotationRay;
    private Animator anim;

    private GameObject playerCamera;

	// Use this for initialization
	void Start()
	{
		m_rigidBody = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");

    }

	void Update()
	{
        if (playerCamera == null)
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

		// Create a plane where player at
		Plane playerPlane = new Plane(Vector3.up, transform.position);

		// Get the ray of where mouse is on the screen
		Ray cameraRay = playerCamera.GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);

		float hitdistance = 0f;
		// Test if the camera ray intersects the plane, and also give a
		// distance where the centre of the mouse point to the point of 
		// interset and get the point using the distance
		if (playerPlane.Raycast(cameraRay, out hitdistance))
		{
			Vector3 targetPoint = cameraRay.GetPoint(hitdistance);

			// Rotate the object towards that point of intersect
			transform.LookAt(targetPoint);
		}
	}

	void FixedUpdate()
	{
        float horizontalInput = Input.GetAxisRaw("Horizontal");
		float verticalInput = Input.GetAxisRaw("Vertical");
		Vector3 inputVec = new Vector3(horizontalInput, 0f, verticalInput);



        // Process Movement
        if (inputVec.magnitude != 0 && inputVec.magnitude < MoveSpeed)
		{
            anim.SetBool("Run", true);
			m_rigidBody.velocity = inputVec.normalized * MoveSpeed * Time.fixedDeltaTime;
        }
        else
        {
            anim.SetBool("Run", false);
        }
	}
}
