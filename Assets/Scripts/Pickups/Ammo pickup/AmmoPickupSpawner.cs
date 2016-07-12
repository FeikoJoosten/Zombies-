using UnityEngine;
using System.Collections.Generic;

public class AmmoPickupSpawner : OverridableMonoBehaviour
{
	[SerializeField]
	private AmmoPickup[] ammoPrefabs;
	[SerializeField]
	private float spawnTime;
	[SerializeField]
	private LayerMask spawnMask;
	[SerializeField]
	private LayerMask nonSpawnMask;

	private List<Weapon> enabledWeapons = new List<Weapon>();
	private Player[] players;
	private float currentSpawnTime;

	void Start()
	{
		if (PhotonNetwork.player != PhotonNetwork.masterClient)
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
				currentSpawnTime -= Time.deltaTime;

				if (currentSpawnTime <= 0)
				{
					SpawnAmmoPack();
					currentSpawnTime = spawnTime;
				}
			} 
		}
		else
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (GameManager.GetInstance().GetNetworkManager() != null)
				{
					if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers != null)
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

	void SpawnAmmoPack()
	{
		enabledWeapons.Clear();
		Player player = players[0];
		int playerEnabledWeaponsCount = 0;

		for(int i = 0; i < player.AllWeapons.Length; i++)
		{
			if(player.AllWeapons[i].IsAllowedToUse == true)
			{
				playerEnabledWeaponsCount++;
			}
		}

		for(int i = 0; i < players.Length; i++)
		{
			int temp = 0;
			for(int j = 0; j < players[i].AllWeapons.Length; j++)
			{
				if(players[i].AllWeapons[j].IsAllowedToUse == true)
				{
					temp++;
				}
			}

			if(temp > playerEnabledWeaponsCount)
			{
				player = players[i];
			}
		}

		for (int i = 0; i < player.AllWeapons.Length; i++)
		{
			if (player.AllWeapons[i].IsAllowedToUse == true)
			{
				if (player.AllWeapons[i].CurrentTotalAmmunitionLeft + player.AllWeapons[i].CurrentAmmunitionInMagLeft < player.AllWeapons[i].MaxAmmunitionCount)
				{
					enabledWeapons.Add(player.AllWeapons[i]);
				}
			}
		}

		AmmoPickup prefab = ammoPrefabs[Random.Range(0, enabledWeapons.Count - 1)];
		if (prefab != null)
		{
			if (PhotonNetwork.offlineMode == true)
			{
				Instantiate(prefab, GetASpawnPosition(), prefab.transform.rotation);
			}
			else
			{
				PhotonNetwork.InstantiateSceneObject(prefab.name, GetASpawnPosition(), prefab.transform.rotation, 0, null);
			}
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
