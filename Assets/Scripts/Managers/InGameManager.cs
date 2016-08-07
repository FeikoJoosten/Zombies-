using UnityEngine;
using System.Collections;

public class InGameManager : OverridableMonoBehaviour
{
	[SerializeField]
	private Player playerPrefab;
	[SerializeField]
	private Transform[] spawnlocations;
	[SerializeField]
	private LayerMask spawnMask;
	[SerializeField]
	private LayerMask nonSpawnMask;
	[SerializeField]
	private Vector4 spawnArea = new Vector4(-25.8F, -116.8F, 52.2F, 4);

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
				if (GameManager.GetInstance().CurrentGameType == GameTypes.ZombieMode)
				{
					GameManager.GetInstance().GetNetworkManager().SpawnPlayer(playerPrefab, spawnlocations[PhotonNetwork.player.ID].position, playerPrefab.transform.rotation); 
				}
				else if(GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
				{
					GameManager.GetInstance().GetNetworkManager().SpawnPlayer(playerPrefab, GetASpawnPosition(), playerPrefab.transform.rotation);
				}
			}
			GameManager.GetInstance().InGame = true;
			enabled = false;
		}
	}

	Vector3 GetASpawnPosition()
	{
		Vector3 rayCastPosition = new Vector3(Random.Range(spawnArea.x, spawnArea.z), transform.position.y, Random.Range(spawnArea.y, spawnArea.w));
		Ray ray = new Ray(rayCastPosition, Vector3.down);
		RaycastHit hit = new RaycastHit();

		if (Physics.Raycast(ray, out hit, Mathf.Infinity))
		{
			if (nonSpawnMask == (nonSpawnMask | (1 << hit.transform.gameObject.layer)))
			{
				return GetASpawnPosition();
			}
			else
			{
				if (spawnMask == (spawnMask | (1 << hit.transform.gameObject.layer)))
				{
					return hit.point;
				}
				else
				{
					return GetASpawnPosition();
				}
			}
		}
		else
		{
			return GetASpawnPosition();
		}
	}
}
