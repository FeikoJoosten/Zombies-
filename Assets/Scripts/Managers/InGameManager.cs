using UnityEngine;

public class InGameManager : OverridableMonoBehaviour
{
	[SerializeField]
	private Player playerPrefab = null;
	[SerializeField]
	private Transform[] spawnlocations = null;
	[SerializeField]
	private LayerMask spawnMask = 0;
	[SerializeField]
	private LayerMask nonSpawnMask = 0;
	[SerializeField]
	private Vector4 spawnArea = new Vector4(-25.8F, -116.8F, 52.2F, 4);

	private bool spawnedPlayer;

	public bool SpawnedPlayer
	{
		get { return spawnedPlayer; }
		set { spawnedPlayer = value; }
	}

	public override void UpdateMe()
	{
		if (spawnedPlayer == true)
		{
			return;
		}

		if (GameManager.GetInstance().InGame != false) return;

		if (PhotonNetwork.offlineMode == true)
		{
			Instantiate(playerPrefab, spawnlocations[Random.Range(0, spawnlocations.Length)].position, playerPrefab.transform.rotation);
		}
		else
		{
			switch (GameManager.GetInstance().CurrentGameType)
			{
				case GameTypes.ZombieMode:
					GameManager.GetInstance().GetNetworkManager().SpawnPlayer(playerPrefab, spawnlocations[PhotonNetwork.player.ID].position, playerPrefab.transform.rotation);
					break;
				case GameTypes.TTT:
					GameManager.GetInstance().GetNetworkManager().SpawnPlayer(playerPrefab, GetASpawnPosition(), playerPrefab.transform.rotation);
					break;
			}
			spawnedPlayer = true;
		}
		GameManager.GetInstance().InGame = true;
	}

	public void SpawnNewPlayers(int amountToSpawn)
	{
		for (int i = 0; i < amountToSpawn; i++)
		{
			GameManager.GetInstance().GetNetworkManager().SpawnPlayer(playerPrefab, GetASpawnPosition(), playerPrefab.transform.rotation, true);
		}
	}

	private Vector3 GetASpawnPosition()
	{
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
