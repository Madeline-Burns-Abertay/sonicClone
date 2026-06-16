using UnityEngine;

public class layerSwitch : MonoBehaviour
{
    [SerializeField] private bool sendToBG;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector3 newPos = new Vector3(collision.transform.position.x, collision.transform.position.y, (sendToBG ? 1 : 0));
            Quaternion rot = collision.transform.rotation;
            collision.transform.SetLocalPositionAndRotation(newPos, rot);

        }
    }
}
