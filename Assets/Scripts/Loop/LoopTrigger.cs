using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LoopTriggerHandler : MonoBehaviour
{
	private bool triggered;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void Start() { resetTriggerState(); }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			triggered = true;
			//Debug.Log($"player touched {gameObject.name}");
		}
	}

	public void resetTriggerState() { triggered = false; }

	public bool getTriggerState() { return triggered; }
}
