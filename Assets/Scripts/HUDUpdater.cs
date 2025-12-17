using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;

public class HUDUpdater : MonoBehaviour
{
	public TMP_Text scoreDisplay, timer, ringCount, lifeCount, FPSCount;
	public GameObject playerObject;
	int rings, lives;
	string time;
	string ringTextColour = "yellow";
	string timerTextColour = "yellow";
	Player player;
	private void Start()
	{
		player = playerObject.GetComponent<Player>();
		time = player.getTime();
		rings = player.getRings();
		lives = player.getLives();
		StartCoroutine(SetTextColor());
	}
	void LateUpdate()
	{
		time = player.getTime();
		rings = player.getRings();
		if (rings != 0)
		{
			ringTextColour = "yellow";
		}
		scoreDisplay.text = $"<color=yellow>Score</color>{player.getScore()}";
		timer.text = $"<color={timerTextColour}>Time</color> {player.getTime()}";
		ringCount.text = $"<color={ringTextColour}>Ring</color>  {player.getRings(), 3}";
		lifeCount.text = $"<sprite name=Circle> x{lives,2}";
		FPSCount.text = $"{(int)1 / Time.unscaledDeltaTime} FPS";

	}
	IEnumerator SetTextColor()
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
}
