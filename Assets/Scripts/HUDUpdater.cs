using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;

public class HUDUpdater : MonoBehaviour
{
	public TMP_Text scoreDisplay, timer, ringCount, lifeCount;
	public GameObject playerObject;
	uint rings;
	string time;
	string ringTextColour = "yellow";
	string timerTextColour = "yellow";
	Player player;
	private void Start()
	{
		player = playerObject.GetComponent<Player>();
		StartCoroutine(SetTextColor());
	}
	void Update()
	{
		time = player.getTime();
		rings = player.getRings();
		if (rings == 0)
		{
			ringTextColour = "yellow";
		}
		scoreDisplay.text = $"<color=yellow>Score</color>{player.getScore()}";
		timer.text = $"<color={timerTextColour}>Time</color> {player.getTime()}";
		ringCount.text = $"<color={ringTextColour}>Ring</color>  {player.getRings(), 3}";

	}
	IEnumerator SetTextColor()
	{
		while (true)
		{
			if (rings == 0)
			{
				ringTextColour = (ringTextColour == "yellow" ? "red" : "yellow");
			}
			if (player.getTime()[0] == '9')
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
