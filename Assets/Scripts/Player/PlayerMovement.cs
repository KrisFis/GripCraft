using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GripEngine.GameManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {

	private float moveSpeed = 5.0f;
	private float jumpHeight = 60.0f;

	private const float viewLimit = 85.0f;

	private bool Sprint, Jump, isGrounded;

	private Vector3 movement, rotation;
	private float cameraRotation, currCameraRotation;
	private float distanceToGround;
	
	private Rigidbody rb;
	public Camera cam;

	void Start()
	{
		movement = rotation = Vector3.zero;
		cameraRotation = currCameraRotation = 0.0f;
		Sprint = Jump = false;

		rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		CalculateMovement();
	}

	private void CalculateMovement()
	{
		/* Calculate movement */
		Vector3 mVer = transform.right * Input.GetAxisRaw("Horizontal");
		Vector3 mHor = transform.forward * Input.GetAxisRaw("Vertical");

		movement = (mHor + mVer).normalized * moveSpeed;

		/* Calculate rotation */
		rotation = new Vector3(0f, Input.GetAxisRaw("Mouse X"), 0f) * Settings.cameraVerticalSpeed;

		cameraRotation = Input.GetAxisRaw("Mouse Y") * Settings.cameraHorizonstalSpeed;

		/* Calculate sprint */
		if(Input.GetButtonDown("Sprint"))
			Sprint = true;
		else if(Input.GetButtonUp("Sprint"))
			Sprint = false;

		/* Calculate jump */
		if(Input.GetButtonDown("Jump") && isGrounded)
			Jump = true;
	}

	void FixedUpdate()
	{
		PerformMove();
	}

	private void PerformMove()
	{
		if(Sprint)
		{
			if(Jump)
			{
				rb.AddForce(transform.up * jumpHeight * 1.15f, ForceMode.Impulse);
			}

			if(movement != Vector3.zero)
			{
				rb.MovePosition(rb.position + (movement * 1.5f) * Time.fixedDeltaTime);
			}
		}
		else
		{
			if(Jump)
			{
				rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
			}

			if(movement != Vector3.zero)
			{
				rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
			}
		}

		rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));

		CameraRotate();
		Jump = false;
	}

	private void CameraRotate()
	{
		currCameraRotation -= cameraRotation;
		currCameraRotation = Mathf.Clamp(currCameraRotation, -viewLimit, viewLimit);

		cam.transform.localEulerAngles = new Vector3(currCameraRotation,0,0);
	}

	void OnCollisionStay(Collision _collision)
	{
		if(_collision.gameObject.tag != "Block")
			return;

		isGrounded = true;
	}

	void OnCollisionExit(Collision _collision)
	{
		if(_collision.gameObject.tag != "Block")
			return;

		isGrounded = false;
	}
}
