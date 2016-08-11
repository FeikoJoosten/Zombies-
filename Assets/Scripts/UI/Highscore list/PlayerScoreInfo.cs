using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerScoreInfo : MonoBehaviour
{
	[SerializeField]
	private Text playerName;
	[SerializeField]
	private Text playerKillCount;
	[SerializeField]
	private Text playerKarmaCount;
	[SerializeField]
	private Image background;

	private int currentKillCount = 0;
	private int currentKarmaCount = 1000;

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

	public void UpdateKarmaCount(int value)
	{
		currentKarmaCount += value;
		playerKarmaCount.text = currentKarmaCount.ToString();
	}

	public void EnableKarmaText()
	{
		playerKarmaCount.gameObject.SetActive(true);
	}

	public void ChangeBackgroundColor(Color colorToUse)
	{
		Color tempColor = new Color(colorToUse.r, colorToUse.g, colorToUse.b, background.color.a);
		background.color = tempColor;
	}

	public void ResetKarmaText()
	{
		currentKarmaCount = 1000;
		playerKarmaCount.text = currentKarmaCount.ToString();
	}

	public void ResetKillCountText()
	{
		currentKillCount = 0;
		playerKillCount.text = currentKillCount.ToString();
	}
}
