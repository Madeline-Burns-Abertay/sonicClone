using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;
    public PolygonCollider2D camBounds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
    }
}
