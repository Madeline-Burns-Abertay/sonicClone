using UnityEngine;

public class GroundChecker : MonoBehaviour
{
	[SerializeField] private LayerMask layer;
	private bool grounded;
	private Collider2D hitbox;
	private void Start()
	{
		grounded = true;
		setCollisionLayer(layer);
		hitbox = GetComponent<Collider2D>();
	}
	public void setCollisionLayer(LayerMask layer)
	{
		hitbox.excludeLayers = ~layer;
		hitbox.includeLayers = layer; // one of these is probably redundant but i don't wanna risk it
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		grounded = true;
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		grounded = false;
	}
	public bool isGrounded() { return grounded; }

	private void OnDrawGizmos()
	{
		Gizmos.color = (grounded ? Color.green : Color.red);
		Gizmos.DrawWireCube(transform.position, transform.localScale / 2);
	}
}
