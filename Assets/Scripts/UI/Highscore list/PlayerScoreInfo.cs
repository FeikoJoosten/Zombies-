using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreInfo : MonoBehaviour
{
	[SerializeField]
	private Text playerName = null;
	[SerializeField]
	private Text playerKillCount = null;
	[SerializeField]
	private Text playerKarmaCount = null;
	[SerializeField]
	private Image background = null;

	private int currentKillCount = 0;
	private int currentKarmaCount = 1000;

	public int CurrentKillCount
	{
		get { return currentKillCount; }
	}

	public void UpdatePlayerName(string Name)
	{
		playerName.text = Name;
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
