using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
	private int score, rings, time;
	private bool outOfTime;

	private const int timeLimitMins = 10; // y'know what? no. i'm not gonna let the time limit be anything but 10 mins
	void Start()
	{
		score = rings = time = 0;
		outOfTime = false;
		StartCoroutine(incrementTimer());
	}

	// -------------------------------------
	// rings
	// -------------------------------------

	public int getRings() { return rings; }
	public void collectRing() { rings++; }
	public void resetRings() { rings = 0; }

	// -------------------------------------
	// score
	// -------------------------------------

	public string getScore() { return score.ToString().PadLeft(7); }
	public void resetScore() { score = 0; }
	public void addScore(int points)
	{
		score = Mathf.Min(score + (points * 10), 9999990); // 7 digit score display, the last of which is always zero
	}

	// -------------------------------------
	// time
	// -------------------------------------
	public bool isOutOfTime() { return outOfTime; }
	private IEnumerator incrementTimer()
	{
		while (time < timeLimitMins * 60 - 1)
		{
			yield return new WaitForSeconds(1f);
			time++;
		}
		outOfTime = true;
	}
	public string getTime() // we only need the formatted version. it's fine. leave it. it's fine
	{
		int mins = Mathf.FloorToInt(time / 60);
		int secs = (int)time % 60;
		return $"{mins}:{secs:00}";
	}
}
