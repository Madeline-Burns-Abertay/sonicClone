using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	InputAction move, jump, crouch, look;
	[SerializeField] float speed = 1.0f;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		move = InputSystem.actions.FindAction("Move");
		jump = InputSystem.actions.FindAction("Jump");
	}

	// Update is called once per frame
	void Update()
	{
		float horizontal = move.ReadValue<float>();
		rb.AddForce(new Vector2(horizontal, 0) * speed);
	}
}
