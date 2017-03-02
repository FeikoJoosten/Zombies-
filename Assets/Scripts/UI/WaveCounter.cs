using UnityEngine;
using UnityEngine.UI;

public class WaveCounter : OverridableMonoBehaviour
{
	[SerializeField]
	private Text waveCounter = null;
	[SerializeField]
	private Player player = null;

	private bool foundAIManager;
	private AIManager aiManager;

	public override void UpdateMe()
	{
		switch (GameManager.GetInstance().CurrentGameType)
		{
			case GameTypes.ZombieMode:
				if (foundAIManager == false)
				{
					if (FindObjectOfType<AIManager>() == null) return;

					aiManager = FindObjectOfType<AIManager>();
					foundAIManager = true;
				}
				else
				{
					if (aiManager != null)
					{
						waveCounter.text = aiManager.CurrentWave.ToString();
					}
				}
				break;
			case GameTypes.TeamDeathMatch:

				break;
			case GameTypes.TTT:
				if(GameManager.GetInstance().TTTWarmingUp == true)
				{
					waveCounter.text = "Warm up";
				}
				else if(GameManager.GetInstance().TTTCoolingDown == true)
				{
					waveCounter.color = Color.white;
					if (GameManager.GetInstance().GetNetworkManager().AllRemainingTraitorPlayers.Count == 0 && GameManager.GetInstance().GetNetworkManager().AllRemainingInnocentPlayers.Count > 0)
					{
						waveCounter.text = "Innocents win";
					}
					else if(GameManager.GetInstance().GetNetworkManager().AllRemainingTraitorPlayers.Count > 0 && GameManager.GetInstance().GetNetworkManager().AllRemainingInnocentPlayers.Count == 0)
					{
						waveCounter.text = "Traitors win";
					}
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
			break;
		}
	}
}
