using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EndSignpost : MonoBehaviour
{
	[SerializeField] private AudioClip signpostSFX;
	private void Start()
	{
		if (signpostSFX == null)
		{
			Debug.Log("forgot to set the sfx genius - " + Consts.ERROR_MESSAGE);
			return;
		}
		GetComponent<AudioSource>().clip = signpostSFX;
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		
		if (collision.CompareTag("Player"))
		{
			GetComponent<SpriteRenderer>().color = new Color(0.35f, 0.64f, 0.93f);
			if (signpostSFX == null)
			{
				Debug.Log("forgot to set the sfx genius - " + Consts.ERROR_MESSAGE);
				return;
			}
			GetComponent<AudioSource>().Play();
		}
	}
}
