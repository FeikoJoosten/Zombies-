using UnityEngine;
using UnityEngine.UI;

public class WaveCounter : OverridableMonoBehaviour
{
	[SerializeField]
	private Text waveCounter;
	[SerializeField]
	private Player player;

	private bool foundAIManager = false;
	private AIManager aiManager;

	public override void UpdateMe()
	{
		if (GameManager.GetInstance().CurrentGameType == GameTypes.ZombieMode)
		{
			if (foundAIManager == false)
			{
				if (FindObjectOfType<AIManager>() != null)
				{
					aiManager = FindObjectOfType<AIManager>();
					foundAIManager = true;
					return;
				}

				return;
			}
			else
			{
				if (aiManager != null)
				{
					waveCounter.text = aiManager.CurrentWave.ToString();
				}
			}
		}
		else if (GameManager.GetInstance().CurrentGameType == GameTypes.TeamDeathMatch)
		{

		}
		else if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
		{
			if(GameManager.GetInstance().TTTWarmingUp == true)
			{
				waveCounter.text = "Warm up";
			}
			else
			{
				switch (player.CurrentTTTTeam)
				{
					case TTTTeams.Innocent:
					waveCounter.text = "Innocent";
					waveCounter.color = Color.green;
					break;
					case TTTTeams.Traitor:
					waveCounter.text = "Traitor";
					waveCounter.color = Color.red;
					break;
					case TTTTeams.Detective:
					waveCounter.text = "Detective";
					waveCounter.color = Color.blue;
					break;
				}
			}
		}
	}
}
