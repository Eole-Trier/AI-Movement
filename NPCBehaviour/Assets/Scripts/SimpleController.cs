using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SimpleController : MonoBehaviour {

	public float moveSpeed = 6;

	Rigidbody rb;
	Camera viewCamera;
	Vector3 velocity;

	void Start () {
		rb = GetComponent<Rigidbody> ();
		viewCamera = Camera.main;
	}

	void Update () {
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
        mousePos.y = 0;
        transform.LookAt (mousePos + Vector3.up * transform.position.y);
		velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;
	}

	void FixedUpdate() {
		rb.MovePosition (rb.position + velocity * Time.fixedDeltaTime);
	}
}