using System.Collections;
using UnityEngine;

public class Ring : MonoBehaviour
{
    private CircleCollider2D ringHitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ringHitbox = GetComponent<CircleCollider2D>();
        if (GetComponent<Rigidbody2D>().gravityScale != 0)
        {
            ringHitbox.enabled = false;
            StartCoroutine(OnRingsDrop());
        }
    }

    IEnumerator OnRingsDrop()
    {
        yield return new WaitForSeconds(0.5f);
        ringHitbox.enabled = true;
    }
}
