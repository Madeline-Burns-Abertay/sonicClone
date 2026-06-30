using System.Collections;
using UnityEngine;

public class Ring : MonoBehaviour
{
    [SerializeField] float ringCollisionDelay = 0.1f;
    private CircleCollider2D ringHitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ringHitbox = GetComponent<CircleCollider2D>();
        if (GetComponent<Rigidbody2D>().gravityScale > Mathf.Epsilon)
        {
            ringHitbox.enabled = false;
            StartCoroutine(OnRingsDrop());
        }
    }

    IEnumerator OnRingsDrop()
    {
        yield return new WaitForSeconds(ringCollisionDelay);
        ringHitbox.enabled = true;
    }
}
