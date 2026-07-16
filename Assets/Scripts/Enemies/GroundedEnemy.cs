using UnityEngine;

// simplest type of enemy - stays on the ground and just moves left
public class GroundedEnemy : Enemy
{
	[SerializeField] private float speed;
	protected override void Start()
	{
		base.Start();
	}
	protected override void enemyBehaviour()
	{
		rb.AddForce(speed * Vector2.left * Time.deltaTime);
	}


}
