using UnityEngine;
using System.Collections.Generic;

public class AmmoPickupSpawner : OverridableMonoBehaviour
{
	[SerializeField]
	private AmmoPickup[] ammoPrefabs = null;
	[SerializeField]
	private float spawnTime = 0;
	[SerializeField]
	private LayerMask spawnMask = 0;
	[SerializeField]
	private LayerMask nonSpawnMask = 0;
	[SerializeField]
	private int TTTWeaponSpawnCount = 50;

	private List<Weapon> enabledWeapons = new List<Weapon>();
	private Player[] players;
	private float currentSpawnTime;

	private void Start()
	{
		if (!PhotonNetwork.player.Equals(PhotonNetwork.masterClient))
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
					SpawnAmmoPack(true);
					currentSpawnTime = spawnTime;
				}

				switch (GameManager.GetInstance().CurrentGameType)
				{
					case GameTypes.TTT:
						for (int i = TTTWeaponSpawnCount; i >= 0; i--)
						{
							SpawnAmmoPack(false);
							TTTWeaponSpawnCount--;
						}
					break;
				}
			} 
			else
			{
				players = new Player[GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count];
				GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Values.CopyTo(players, 0);
			}
		}
		else
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (GameManager.GetInstance().GetNetworkManager() == null) return;
				if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers == null) return;

				players = new Player[GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count];

				GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Values.CopyTo(players, 0);
			}
			else
			{
				players = FindObjectsOfType<Player>();
			}
		}
	}

	private int CalculateEnabledWeaponsCount()
	{
		enabledWeapons.Clear();
		Player player = players[0];
		int playerEnabledWeaponsCount = 0;

		for (int i = 0; i < player.AllWeapons.Length; i++)
		{
			if (player.AllWeapons[i].IsAllowedToUse == true)
			{
				playerEnabledWeaponsCount++;
			}
		}

		for (int i = 0; i < players.Length; i++)
		{
			int temp = 0;
			for (int j = 0; j < players[i].AllWeapons.Length; j++)
			{
				if (players[i].AllWeapons[j].IsAllowedToUse == true)
				{
					temp++;
				}
			}

			if (temp > playerEnabledWeaponsCount)
			{
				player = players[i];
			}
		}

		for (int i = 0; i < player.AllWeapons.Length; i++)
		{
			if (player.AllWeapons[i].IsAllowedToUse != true) continue;

			if (player.AllWeapons[i].CurrentTotalAmmunitionLeft + player.AllWeapons[i].CurrentAmmunitionInMagLeft < player.AllWeapons[i].MaxAmmunitionCount)
			{
				enabledWeapons.Add(player.AllWeapons[i]);
			}
		}

		return enabledWeapons.Count;
	}

	private void SpawnAmmoPack(bool shouldShowLineRenderer)
	{
		AmmoPickup prefab;

		if (GameManager.GetInstance().CurrentGameType == GameTypes.ZombieMode)
		{
			CalculateEnabledWeaponsCount();
			prefab = ammoPrefabs[Random.Range(0, enabledWeapons.Count - 1)];
		}
		else
		{
			prefab = ammoPrefabs[Random.Range(0, ammoPrefabs.Length - 1)];
		}

		if (prefab == null) return;

		if (PhotonNetwork.offlineMode == true)
		{
			AmmoPickup pickup = (AmmoPickup)Instantiate(prefab, GetASpawnPosition(), prefab.transform.rotation);
			pickup.ShouldShowLineRenderer = shouldShowLineRenderer;
		}
		else
		{
			PhotonNetwork.InstantiateSceneObject(prefab.name, GetASpawnPosition(), prefab.transform.rotation, 0, null).GetComponent<AmmoPickup>().ShouldShowLineRenderer = shouldShowLineRenderer;
		}
	}

	private Vector3 GetASpawnPosition()
	{
		Vector4 spawnArea = GameManager.GetInstance().GetAIManager().SpawnArea;
		Vector3 rayCastPosition = new Vector3(Random.Range(spawnArea.x, spawnArea.z), transform.position.y, Random.Range(spawnArea.y, spawnArea.w));
		Ray ray = new Ray(rayCastPosition, Vector3.down);
		RaycastHit hit;

		if (!Physics.Raycast(ray, out hit, Mathf.Infinity)) return GetASpawnPosition();

		if (nonSpawnMask == (nonSpawnMask | (1 << hit.transform.gameObject.layer)))
		{
			return GetASpawnPosition();
		}

		if (spawnMask == (spawnMask | (1 << hit.transform.gameObject.layer)))
		{
			return hit.point;
		}

		return GetASpawnPosition();
	}

	public void SpawnAmmoPickupOnLocation(int currentWeaponNumber, Vector3 spawnPosition)
	{
		PhotonNetwork.InstantiateSceneObject(ammoPrefabs[currentWeaponNumber].name, spawnPosition, ammoPrefabs[currentWeaponNumber].transform.rotation, 0, null).GetComponent<AmmoPickup>().ShouldShowLineRenderer = true;
	}
}
