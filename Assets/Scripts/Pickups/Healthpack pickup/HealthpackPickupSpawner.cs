using UnityEngine;
using System.Collections;

public class HealthpackPickupSpawner : OverridableMonoBehaviour
{
	[SerializeField]
	private HealthpackPickup healthPrefab;
	[SerializeField]
	private float spawnTime;
	[SerializeField]
	private LayerMask spawnMask;
	[SerializeField]
	private LayerMask nonSpawnMask;

	private float currentSpawnTime;
	private Player[] players;

	void Start()
	{
		if(PhotonNetwork.player != PhotonNetwork.masterClient)
		{
			enabled = false;
		}

		currentSpawnTime = spawnTime;
	}

	public override void UpdateMe()
	{
		if (players != null)
		{
			if (players.Length > 0)
			{
				for (int i = 0; i < players.Length; i++)
				{
					if(players[i] == null)
					{
						players = null;
						break;
					}

					if (players[i].CurrentHealth < players[i].StartingHealth)
					{
						currentSpawnTime -= Time.deltaTime;

						if (currentSpawnTime <= 0)
						{
							SpawnHealthPack();
							currentSpawnTime = spawnTime;
						}
					}
				}
			} 
		}
		else
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (GameManager.GetInstance().GetNetworkManager() != null)
				{
					if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers != null && PhotonNetwork.playerList.Length == GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count)
					{
						players = new Player[GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count];

						GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Values.CopyTo(players, 0);
					}
				} 
			}
			else
			{
				players = FindObjectsOfType<Player>();
			}
		}
	}

	void SpawnHealthPack()
	{
		if (PhotonNetwork.offlineMode == true)
		{
			Instantiate(healthPrefab, GetASpawnPosition(), healthPrefab.transform.rotation);
		}
		else
		{
			PhotonNetwork.InstantiateSceneObject(healthPrefab.name, GetASpawnPosition(), healthPrefab.transform.rotation, 0, null);
		}
	}

	Vector3 GetASpawnPosition()
	{
		Vector4 spawnArea = GameManager.GetInstance().GetAIManager().SpawnArea;
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
