using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        if (0f < viewPos.x && viewPos.x < 1f && 0f < viewPos.y && viewPos.y < 1f)
            GetComponent<Rigidbody2D>().linearVelocityX = -speed;
    }
}
