using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Loop : MonoBehaviour
{
	[SerializeField] private GameObject foregroundHalf, backgroundHalf;
	private Collider2D foregroundHitbox, backgroundHitbox;
	[SerializeField] private GameObject enterTrigger, topTrigger, exitTrigger;
	private LoopTriggerHandler entryHandler, topHandler, exitHandler;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void Start()
	{
		foregroundHitbox = foregroundHalf.GetComponent<Collider2D>();
		backgroundHitbox = backgroundHalf.GetComponent<Collider2D>();

		entryHandler = enterTrigger.GetComponent<LoopTriggerHandler>();
		topHandler = topTrigger.GetComponent<LoopTriggerHandler>();
		exitHandler = exitTrigger.GetComponent<LoopTriggerHandler>();
	}

	// Update is called once per frame
	private void Update()
	{
		bool enter = entryHandler.getTriggerState(), exit = exitHandler.getTriggerState(), top = topHandler.getTriggerState();
		if (enter)
		{
			backgroundHitbox.enabled = top;
			foregroundHitbox.enabled = true;
		}
		if (exit)
		{
			foregroundHitbox.enabled = top;
			backgroundHitbox.enabled = true;
		}
		if (top)
		{
			backgroundHitbox.enabled = enter;
			foregroundHitbox.enabled = exit;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player")) resetTriggers();
	}

	private void resetTriggers()
	{
		entryHandler.resetTriggerState();
		topHandler.resetTriggerState();
		exitHandler.resetTriggerState();
	}
}
