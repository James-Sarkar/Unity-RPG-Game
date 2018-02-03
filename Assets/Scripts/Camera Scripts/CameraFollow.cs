using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public float followHeight = 8f, followDistance = 6f;

	private Transform player;

	private float targetHeight, currentHeight, currentRotation;

	void Awake () {
		player = GameObject.FindGameObjectWithTag ("Player").transform;
	}

	void Update () {
		targetHeight = player.position.y + followHeight;

		currentRotation = transform.eulerAngles.y;

		currentHeight = Mathf.Lerp (transform.position.y, targetHeight, 0.9f * Time.deltaTime);

		Quaternion euler = Quaternion.Euler (0f, currentRotation, 0f);

		Vector3 targetPosition = player.position - (euler * Vector3.forward) * followDistance;

		targetPosition.y = currentHeight;

		transform.position = targetPosition;
		transform.LookAt (player);
	}
}
