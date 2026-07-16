using UnityEditor.ShaderGraph;
using UnityEngine;

public class FlyingEnemyProjectile : MonoBehaviour
{
	private GameObject player;
	private Vector2 direction;
	private Vector3 viewPos;
	[SerializeField] private float speed;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void Start()
	{
		player = GameObject.FindWithTag("Player");
		direction = (player.transform.position - transform.position).normalized;
	}

	// Update is called once per frame
	private void Update()
	{
		transform.Translate(speed * Time.deltaTime * direction);
		if (!isVisible()) Destroy(gameObject);
	}

	private bool isVisible()
	{
        viewPos = Camera.main.WorldToViewportPoint(transform.position);
        bool vertical = -Consts.OFF_SCREEN_LOAD_DIST < viewPos.y && viewPos.y < 1f + Consts.OFF_SCREEN_LOAD_DIST;
        bool horizontal = -Consts.OFF_SCREEN_LOAD_DIST < viewPos.x && viewPos.x < 1f + Consts.OFF_SCREEN_LOAD_DIST;
        return vertical && horizontal;
    }
}
