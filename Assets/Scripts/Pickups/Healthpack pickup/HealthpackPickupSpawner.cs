using UnityEngine;

public class HealthpackPickupSpawner : OverridableMonoBehaviour
{
	[SerializeField]
	private HealthpackPickup healthPrefab = null;
	[SerializeField]
	private float spawnTime = 0;
	[SerializeField]
	private LayerMask spawnMask = 0;
	[SerializeField]
	private LayerMask nonSpawnMask = 0;

	private float currentSpawnTime;
	private Player[] players;

	private void Start()
	{
		if (!PhotonNetwork.player.Equals(PhotonNetwork.masterClient) || GameManager.GetInstance().CurrentGameType != GameTypes.ZombieMode)
		{
			enabled = false;
		}

		currentSpawnTime = spawnTime;
	}

	public override void UpdateMe()
	{
		if (players != null)
		{
			if (players.Length <= 0) return;

			for (int i = 0; i < players.Length; i++)
			{
				if (players[i] == null)
				{
					players = null;
					break;
				}

				if (!(players[i].CurrentHealth < players[i].StartingHealth)) continue;

				currentSpawnTime -= Time.deltaTime;

				if (!(currentSpawnTime <= 0)) continue;

				SpawnHealthPack();
				currentSpawnTime = spawnTime;
			}
		}
		else
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (GameManager.GetInstance().GetNetworkManager() == null) return;
				if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers == null &&
					PhotonNetwork.playerList.Length != GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count)
					return;

				players = new Player[GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count];

				GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Values.CopyTo(players, 0);
			}
			else
			{
				players = FindObjectsOfType<Player>();
			}
		}
	}

	private void SpawnHealthPack()
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
}
