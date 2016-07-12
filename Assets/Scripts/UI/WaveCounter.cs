using UnityEngine;
using UnityEngine.UI;

public class WaveCounter : OverridableMonoBehaviour
{
	[SerializeField]
	private Text waveCounter;

	private bool foundAIManager = false;
	private AIManager aiManager;

	public override void UpdateMe()
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
}
