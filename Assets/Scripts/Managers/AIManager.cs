using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class AIManager : OverridableMonoBehaviour
{
	[SerializeField]
	private Zombie zombiePrefab;
	[SerializeField]
	private LayerMask spawnMask;
	[SerializeField]
	private LayerMask nonSpawnMask;
	[SerializeField]
	private int baseMaxZombiesOnMap;
	[SerializeField]
	private int amountOfExtraZombiesPerPlayer;
	[SerializeField]
	private float spawnRateCalculator;
	[SerializeField]
	private float spawnTime;
	[SerializeField]
	private Vector4 spawnArea;
	[SerializeField]
	private RectTransform minimapDot;

	private Dictionary<int, Zombie> allRemainingZombies = new Dictionary<int, Zombie>();
	private bool startedSpawning = false;
	private float timer = 1;
	private int currentWave;
	private int spawnCap = 0;

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

	void Start()
	{
		AIManager[] aiManagers = FindObjectsOfType<AIManager>();

		if (aiManagers.Length > 1)
		{
			Destroy(gameObject);
		}

		zombiePrefab.StartingHealth = 100;

		if (PhotonNetwork.isMasterClient == true)
		{
			if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count == PhotonNetwork.playerList.Length)
			{
				NextWave();
			}
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
			if (player.Value.enabled == false)
			{
				GameManager.GetInstance().GetNetworkManager().FindPlayers();
				break;
			}
		}

		if (allRemainingZombies.Count > 0)
		{
			AssignTargetForAI();
		}
	}

	[PunRPC]
	public void CreatePositionPointOnMinimap(Vector3 zombiePosition)
	{
		Instantiate(minimapDot, new Vector3(zombiePosition.x, minimapDot.transform.position.y, zombiePosition.z), minimapDot.transform.rotation);
	}

	void AssignTargetForAI()
	{
		foreach (var zomb in allRemainingZombies)
		{
			if (zomb.Value == null)
			{
				continue;
			}

			if (zomb.Value.FinishedSpawning == true)
			{
				if (zomb.Value.IsDead == false)
				{
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

					if (target.position != zomb.Value.Agent.destination)
					{
						NavMeshPath path = new NavMeshPath();
						NavMesh.CalculatePath(zomb.Value.transform.position, target.position, NavMesh.AllAreas, path);

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
			}
		}
	}

	[PunRPC]
	void AssignTargetForSpecificAI(Vector3[] path, int AINumber)
	{
		if (allRemainingZombies.ContainsKey(AINumber) == false || allRemainingZombies[AINumber] == null)
		{
			return;
		}

		NavMeshPath p = new NavMeshPath();
		Zombie zomb = allRemainingZombies[AINumber];

		if (zomb.Agent.isOnNavMesh == false || zomb.Agent.enabled == false || path == null)
		{
			return;
		}

		if (path.Length != 0)
		{
			zomb.Agent.CalculatePath(path[path.Length - 1], p);

			FieldInfo a = typeof(NavMeshPath).GetField("m_corners", BindingFlags.Public |
												 BindingFlags.NonPublic |
												 BindingFlags.Instance);

			a.SetValue(p, new Vector3[path.Length], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, System.Globalization.CultureInfo.CurrentCulture);

			for (int i = 0; i < path.Length; i++)
			{
				p.corners[i] = path[i];
			}
			zomb.Agent.SetPath(p);
		}
	}

	void UpdateAIStats()
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

	IEnumerator SpawnWave()
	{
		int amountToSpawn = 0;

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

		amountToSpawn = Mathf.RoundToInt(spawnCap * (currentWave * spawnRateCalculator));

		while (amountToSpawn != 0)
		{
			if (amountToSpawn <= spawnCap)
			{
				while (timer < (spawnTime * 2))
				{
					timer++;
					if (Mathf.Round(timer) % spawnTime == 0)
					{
						int spawnValue = 0;

						if (amountToSpawn == Mathf.RoundToInt(spawnCap * (currentWave * spawnRateCalculator)))
						{
							spawnValue = Mathf.RoundToInt(amountToSpawn / 2);
						}
						else
						{
							spawnValue = amountToSpawn;
						}

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
						int spawnValue = 0;

						spawnValue = Mathf.RoundToInt(spawnCap / 2);
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

	IEnumerator SpawnZombie(int amount)
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

		yield return currentSpawnValue > amount ? true : false;
	}

	Vector3 GetASpawnPosition()
	{
		Vector3 rayCastPosition = new Vector3(UnityEngine.Random.Range(spawnArea.x, spawnArea.z), transform.position.y, UnityEngine.Random.Range(spawnArea.y, spawnArea.w));
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

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			if (PhotonNetwork.player == PhotonNetwork.masterClient)
			{
				stream.SendNext(currentWave);
			}
		}
		else
		{
			if (PhotonNetwork.player != PhotonNetwork.masterClient)
			{
				currentWave = (int)stream.ReceiveNext();
			}
		}
	}
}