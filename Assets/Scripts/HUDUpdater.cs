using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;

public class HUDUpdater : MonoBehaviour
{
	[SerializeField] private TMP_Text scoreDisplay, timer, ringCount, lifeCount, debugOutput;
	[SerializeField] private bool debug;
	[SerializeField] private GameObject player;
	private int rings, lives;
	private string time;
	private string ringTextColour = "yellow";
	private string timerTextColour = "yellow";
	private PlayerController playerController;
	private PlayerScore score;
	private uint frameCount;
	private float avgFPS;
	private void Start()
	{
		playerController = player.GetComponent<PlayerController>();
		score = player.GetComponent<PlayerScore>();
		time = score.getTime();
		rings = score.getRings();
		lives = playerController.getLives();
		StartCoroutine(SetTextColor());
		if (debug) StartCoroutine(GetAverageFramerate(1f));
	}
	private void LateUpdate()
	{
		time = score.getTime();
		rings = score.getRings();
		if (rings != 0)
		{
			ringTextColour = "yellow";
		}
		scoreDisplay.text = $"<color=yellow>Score</color>{score.getScore()}";
		timer.text = $"<color={timerTextColour}>Time</color> {score.getTime()}";
		ringCount.text = $"<color={ringTextColour}>Ring</color>  {score.getRings(), 3}";
		lifeCount.text = $"<sprite name=Circle> x{lives,2}";
		if (debug) debugOutput.text = 
				$"<color=yellow>X:</color>{player.transform.position.x}\n" +
				$"<color=yellow>Y:</color>{player.transform.position.y}\n" +
				$"{(int) avgFPS} FPS";

	}
	private IEnumerator SetTextColor()
	{
		while (true)
		{
			if (rings == 0)
			{
				ringTextColour = (ringTextColour == "yellow" ? "red" : "yellow");
			}

			if (time[0] == '9')
			{
				timerTextColour = (timerTextColour == "yellow" ? "red" : "yellow");
			}
			else
			{
				timerTextColour = "yellow";
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator GetAverageFramerate(float duration)
	{
		float timer = 0f;
		frameCount = 0;
		while (debug)
		{
			while (timer < duration)
			{
				yield return new WaitForEndOfFrame();
				timer += Time.unscaledDeltaTime;
				frameCount++;
			}
			avgFPS = 1 / frameCount;
			timer = 0f; // dumbass
		}
	}
}
