using UnityEngine;
using System.Collections;

public class InGameManager : OverridableMonoBehaviour
{
	[SerializeField]
	private Player playerPrefab;
	[SerializeField]
	private Transform[] spawnlocations;

	public override void UpdateMe()
	{
		if(GameManager.GetInstance().InGame == false)
		{
			if(PhotonNetwork.offlineMode == true)
			{
				Instantiate(playerPrefab, spawnlocations[Random.Range(0, spawnlocations.Length)].position, playerPrefab.transform.rotation);
			}
			else
			{
				GameManager.GetInstance().GetNetworkManager().SpawnPlayer(playerPrefab, spawnlocations[PhotonNetwork.player.ID].position, playerPrefab.transform.rotation);
			}
			GameManager.GetInstance().InGame = true;
			enabled = false;
		}
	}
}
