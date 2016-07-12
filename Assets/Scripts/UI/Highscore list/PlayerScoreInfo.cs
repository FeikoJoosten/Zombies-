using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerScoreInfo : MonoBehaviour
{
	[SerializeField]
	private Text playerName;
	[SerializeField]
	private Text playerKillCount;

	private int currentKillCount = 0;

	public int CurrentKillCount
	{
		get { return currentKillCount; }
	}

	public void UpdatePlayerName(string name)
	{
		playerName.text = name;
	}

	public void UpdateKillCount(int value)
	{
		currentKillCount += value;
		playerKillCount.text = currentKillCount.ToString();
	}
}
