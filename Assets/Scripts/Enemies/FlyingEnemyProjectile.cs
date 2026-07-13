using UnityEditor.ShaderGraph;
using UnityEngine;

public class FlyingEnemyProjectile : MonoBehaviour
{
	private GameObject player;
	private Vector2 direction;
	[SerializeField] private float speed;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		player = GameObject.FindWithTag("Player");
		direction = (player.transform.position - transform.position).normalized;
	}

	// Update is called once per frame
	void Update()
	{
		transform.Translate(speed * Time.deltaTime * direction);
	}
}
