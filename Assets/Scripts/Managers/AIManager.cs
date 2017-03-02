using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class AIManager : OverridableMonoBehaviour
{
	[SerializeField]
	private Zombie zombiePrefab = null;
	[SerializeField]
	private LayerMask spawnMask = 0;
	[SerializeField]
	private LayerMask nonSpawnMask = 0;
	[SerializeField]
	private int baseMaxZombiesOnMap = 24;
	[SerializeField]
	private int amountOfExtraZombiesPerPlayer = 6;
	[SerializeField]
	private float spawnRateCalculator = 0.15f;
	[SerializeField]
	private float spawnTime = 5;
	[SerializeField]
	private Vector4 spawnArea = new Vector4(-25.8f, -116.8f, 52.2f, 4);
	[SerializeField]
	private RectTransform minimapDot = null;

	private Dictionary<int, Zombie> allRemainingZombies = new Dictionary<int, Zombie>();
	private bool startedSpawning;
	private float timer = 1;
	private int currentWave;
	private int spawnCap;

	public Dictionary<int, Zombie> AllRemainingZombies
	{
		get { return allRemainingZombies; }
	}
	public Vector4 SpawnArea
	{
		get { return spawnArea; }
	}
	public int CurrentWave
	{
		get { return currentWave; }
	}

	private void Start()
	{
		AIManager[] aiManagers = FindObjectsOfType<AIManager>();

		if (aiManagers.Length > 1)
		{
			Destroy(gameObject);
		}

		switch (GameManager.GetInstance().CurrentGameType)
		{
			case GameTypes.ZombieMode:
				zombiePrefab.StartingHealth = 100;

				if (PhotonNetwork.isMasterClient == true)
				{
					if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count == PhotonNetwork.playerList.Length)
					{
						NextWave();
					}
				}
				break;
		}
	}

	public override void UpdateMe()
	{
		if (PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		foreach (var player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
		{
			if (player.Value == null)
			{
				continue;
			}

			if (player.Value.enabled != false) continue;

			GameManager.GetInstance().GetNetworkManager().FindPlayers();
			break;
		}

		switch (GameManager.GetInstance().CurrentGameType)
		{
			case GameTypes.ZombieMode:
				if (allRemainingZombies.Count > 0)
				{
					AssignTargetForAI();
				}
				break;
		}
	}

	[PunRPC]
	public void CreatePositionPointOnMinimap(Vector3 zombiePosition)
	{
		Instantiate(minimapDot, new Vector3(zombiePosition.x, minimapDot.transform.position.y, zombiePosition.z), minimapDot.transform.rotation);
	}

	private void AssignTargetForAI()
	{
		foreach (var zomb in allRemainingZombies)
		{
			if (zomb.Value == null)
			{
				continue;
			}

			if (zomb.Value.FinishedSpawning == false) return;
			if (zomb.Value.IsDead == true) return;

			if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count == 0)
			{
				return;
			}

			float distance = Vector3.Distance(zomb.Value.transform.position, GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.First().Value.transform.position);
			Transform target = GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.First().Value.transform;

			foreach (var player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
			{
				if (Vector3.Distance(zomb.Value.transform.position, player.Value.transform.position) < distance)
				{
					target = player.Value.transform;
				}
			}

			if (target.position == zomb.Value.Agent.destination) return;

			UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
			UnityEngine.AI.NavMesh.CalculatePath(zomb.Value.transform.position, target.position, UnityEngine.AI.NavMesh.AllAreas, path);

			if (PhotonNetwork.offlineMode == false)
			{
				photonView.RPC("AssignTargetForSpecificAI", PhotonTargets.All, path.corners, zomb.Key);
			}
			else
			{
				zomb.Value.Agent.SetPath(path);
			}
		}
	}

	[PunRPC]
	private void AssignTargetForSpecificAI(Vector3[] path, int AINumber)
	{
		if (allRemainingZombies.ContainsKey(AINumber) == false || allRemainingZombies[AINumber] == null)
		{
			return;
		}

		UnityEngine.AI.NavMeshPath p = new UnityEngine.AI.NavMeshPath();
		Zombie zomb = allRemainingZombies[AINumber];

		if (zomb.Agent.isOnNavMesh == false || zomb.Agent.enabled == false || path == null)
		{
			return;
		}

		if (path.Length == 0) return;

		zomb.Agent.CalculatePath(path[path.Length - 1], p);

		FieldInfo a = typeof(UnityEngine.AI.NavMeshPath).GetField("m_corners", BindingFlags.Public |
											 BindingFlags.NonPublic |
											 BindingFlags.Instance);

		if (a == null) return;

		a.SetValue(p, new Vector3[path.Length], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, System.Globalization.CultureInfo.CurrentCulture);

		for (int i = 0; i < path.Length; i++)
		{
			p.corners[i] = path[i];
		}
		zomb.Agent.SetPath(p);
	}

	private void UpdateAIStats()
	{
		if (currentWave <= 1)
		{
			zombiePrefab.StartingHealth = 100;
		}
		else
		{
			zombiePrefab.StartingHealth = zombiePrefab.StartingHealth * zombiePrefab.HealthMultiplier;
		}
	}

	public void NextWave()
	{
		if (startedSpawning == false)
		{
			StartCoroutine(SpawnWave());
		}
	}

	private IEnumerator SpawnWave()
	{
		if (currentWave == 0)
		{
			if (spawnCap == 0)
			{
				spawnCap = baseMaxZombiesOnMap + (amountOfExtraZombiesPerPlayer * (PhotonNetwork.playerList.Length - 1));
			}
			zombiePrefab.StartingHealth = 100;
		}

		startedSpawning = true;
		timer = 0;

		currentWave++;

		int amountToSpawn = Mathf.RoundToInt(spawnCap * (currentWave * spawnRateCalculator));

		while (amountToSpawn != 0)
		{
			if (amountToSpawn <= spawnCap)
			{
				while (timer < (spawnTime * 2))
				{
					timer++;
					if (Mathf.Round(timer) % spawnTime == 0)
					{
						int spawnValue = (amountToSpawn == Mathf.RoundToInt(spawnCap * (currentWave * spawnRateCalculator)))
							? Mathf.RoundToInt(amountToSpawn * 0.5f)
							: amountToSpawn;

						amountToSpawn -= spawnValue;
						StartCoroutine(SpawnZombie(spawnValue));
					}

					yield return new WaitForSeconds(1);
				}
			}
			else
			{
				while (timer < (spawnTime * 2))
				{
					timer++;
					if (Mathf.Round(timer) % spawnTime == 0)
					{
						int spawnValue = Mathf.RoundToInt(spawnCap * 0.5f);
						amountToSpawn -= spawnValue;

						StartCoroutine(SpawnZombie(spawnValue));
					}

					yield return new WaitForSeconds(1);
				}

				while (amountToSpawn > 0)
				{
					if (allRemainingZombies.Count < spawnCap)
					{
						StartCoroutine(SpawnZombie(1));
						amountToSpawn--;
					}

					yield return null;
				}
			}

			yield return null;
		}

		UpdateAIStats();
		startedSpawning = false;
	}

	private IEnumerator SpawnZombie(int amount)
	{
		int currentSpawnValue = 0;

		for (int i = 0; i < amount; i++)
		{
			if (PhotonNetwork.offlineMode == true)
			{
				Instantiate(zombiePrefab, GetASpawnPosition(), zombiePrefab.transform.rotation);
			}
			else
			{
				PhotonNetwork.InstantiateSceneObject(zombiePrefab.name, GetASpawnPosition(), zombiePrefab.transform.rotation, 0, null).GetComponent<Zombie>();
			}
			currentSpawnValue++;
		}

		yield return currentSpawnValue > amount;
	}

	private Vector3 GetASpawnPosition()
	{
		Vector3 rayCastPosition = new Vector3(Random.Range(spawnArea.x, spawnArea.z), transform.position.y, Random.Range(spawnArea.y, spawnArea.w));
		Ray ray = new Ray(rayCastPosition, Vector3.down);
		RaycastHit hit;

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

	public void AddZombie(int viewID, Zombie zomb)
	{
		allRemainingZombies.Add(viewID, zomb);
	}

	public void RemoveZombie(Zombie zomb)
	{
		allRemainingZombies.Remove(zomb.photonView.viewID);
		if (PhotonNetwork.isMasterClient == true)
		{
			UpdateManager.RemoveSpecificItemAndDestroyIt(zomb);
		}
		else
		{
			UpdateManager.RemoveSpecificItem(zomb);
		}
	}

	public void DestroyAllRemainingZomies()
	{
		foreach (var zomb in allRemainingZombies)
		{
			UpdateManager.RemoveSpecificItemAndDestroyIt(zomb.Value);
		}

		allRemainingZombies.Clear();
	}

	private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			if (!PhotonNetwork.player.Equals(PhotonNetwork.masterClient)) return;
			stream.SendNext(currentWave);
		}
		else
		{
			if (PhotonNetwork.player.Equals(PhotonNetwork.masterClient)) return;
			currentWave = (int)stream.ReceiveNext();
		}
	}
}