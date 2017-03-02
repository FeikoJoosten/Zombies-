using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : OverridableMonoBehaviour
{
	private int TTTSpawnCount;
	private Player masterClient;
	private Player ownClient;
	private Dictionary<int, Player> allRemainingPlayers = new Dictionary<int, Player>();
	private Dictionary<int, int> currentKarmaCounts = new Dictionary<int, int>();
	private List<Player> allRemainingInnocentPlayers = new List<Player>();
	private List<Player> allRemainingTraitorPlayers = new List<Player>();


	public Player MasterClient
	{
		get { return masterClient; }
	}
	public Player OwnClient
	{
		get { return ownClient; }
	}
	public Dictionary<int, Player> AllRemainingPlayers
	{
		get { return allRemainingPlayers; }
	}
	public List<Player> AllRemainingInnocentPlayers
	{
		get { return allRemainingInnocentPlayers; }
	}
	public List<Player> AllRemainingTraitorPlayers
	{
		get { return allRemainingTraitorPlayers; }
	}

	private bool TTTGameStarted;

	public override void UpdateMe()
	{
		if (GameManager.GetInstance().InGame != true) return;

		if (PhotonNetwork.offlineMode == false)
		{
			if (allRemainingPlayers.Count != PhotonNetwork.playerList.Length)
			{
				FindPlayers();
			}

			if (PhotonNetwork.isMasterClient != true) return;

			if (allRemainingPlayers.Count == PhotonNetwork.playerList.Length && TTTGameStarted == false)
			{
				StartCoroutine(StartTTTCountdown());
			}
		}
		else
		{
			if (allRemainingPlayers.Count == 0)
			{
				FindPlayers();
			}
		}
	}

	public void FindPlayers()
	{
		allRemainingPlayers.Clear();

		Player[] players = FindObjectsOfType<Player>();

		if (players == null) return;

		for (int i = 0; i < players.Length; i++)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (PhotonPlayer.Find(players[i].photonView.OwnerActorNr).IsMasterClient)
				{
					masterClient = players[i];
				}

				if (PhotonPlayer.Find(players[i].photonView.OwnerActorNr).ID == PhotonNetwork.player.ID)
				{
					ownClient = players[i];
				}
			}

			if (allRemainingPlayers.ContainsValue(players[i]) != false) continue;

			if (players[i].enabled == true)
			{
				allRemainingPlayers.Add(
					PhotonNetwork.offlineMode == true ? players[i].GetInstanceID() : players[i].photonView.viewID, players[i]);
			}
		}
	}

	public void CreateRoom(bool isVisible, bool isOpen, int maxPlayers)
	{
		string roomName = PhotonNetwork.player.NickName;

		RoomOptions roomOptions = new RoomOptions
		{
			IsVisible = isVisible,
			IsOpen = isOpen,
			MaxPlayers = Convert.ToByte(maxPlayers),
			CleanupCacheOnLeave = true,
			CustomRoomPropertiesForLobby = new string[1] {"gameType"},
			CustomRoomProperties =
				new ExitGames.Client.Photon.Hashtable(1) {{"gameType", GameManager.GetInstance().CurrentGameType}}
		};



		PhotonNetwork.CreateRoom(CheckForViableRoomname(roomName), roomOptions, TypedLobby.Default);
	}

	private string CheckForViableRoomname(string nameToValidate)
	{
		int counter = 0;

		for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
		{
			if (nameToValidate == PhotonNetwork.GetRoomList()[i].Name)
			{
				counter++;
			}
		}

		return (counter == 0) ? nameToValidate : nameToValidate + " " + counter;
	}

	private void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if (player.Equals(PhotonNetwork.masterClient) == true)
		{
			PhotonNetwork.SetMasterClient(PhotonNetwork.playerList[0]);
		}
	}

	public void JoinRoom(string roomToJoin)
	{
		PhotonNetwork.JoinRoom(roomToJoin);
	}

	public AsyncOperation StartGame(int levelToLoad)
	{
		PhotonNetwork.room.IsVisible = false;
		return PhotonNetwork.LoadLevelAsync(levelToLoad);
	}

	public void ExitGameAndGoBackToMainMenu()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene(0);
	}

	public void SpawnPlayer(Player playerPrefab, Vector3 spawnPosition, Quaternion spawnRotation)
	{
		PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation, 0);
	}

	public void SpawnPlayer(Player playerPrefab, Vector3 spawnPosition, Quaternion spawnRotation, bool secondTTTRound)
	{
		Player player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation, 0).GetComponent<Player>();
		player.photonView.TransferOwnership(PhotonNetwork.playerList[TTTSpawnCount]);

		if (TTTSpawnCount == 0)
		{
			Debug.Log("Assigning new masterclient");
			PhotonNetwork.SetMasterClient(PhotonPlayer.Find(player.photonView.OwnerActorNr));
		}
		TTTSpawnCount++;
	}

	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.Disconnect();
	}

	public void LeaveLobby()
	{
		PhotonNetwork.LeaveLobby();
	}

	public void RemovePlayerFromTeam(Player playerToRemove)
	{
		currentKarmaCounts.Add(playerToRemove.photonView.viewID, playerToRemove.KarmaCount);

		if (playerToRemove.CurrentTTTTeam == TTTTeams.Traitor)
		{
			allRemainingTraitorPlayers.Remove(playerToRemove);
		}
		else
		{
			allRemainingInnocentPlayers.Remove(playerToRemove);
		}
	}

	private void ResetTTTGame()
	{
		TTTSpawnCount = 0;

		for (int i = FindObjectsOfType<AmmoPickup>().Length; i > 0; i--)
		{
			UpdateManager.RemoveSpecificItemAndDestroyIt(FindObjectsOfType<AmmoPickup>()[i - 1]);
		}

		for (int i = FindObjectsOfType<Bullet>().Length; i > 0; i--)
		{
			UpdateManager.RemoveSpecificItemAndDestroyIt(FindObjectsOfType<Bullet>()[i - 1]);
		}

		for (int i = FindObjectsOfType<Grenade>().Length; i > 0; i--)
		{
			UpdateManager.RemoveSpecificItemAndDestroyIt(FindObjectsOfType<Grenade>()[i - 1]);
		}

		List<Player> oldPlayers = new List<Player>();

		foreach(KeyValuePair<int, Player> player in allRemainingPlayers)
		{
			for(int i = 0; i < player.Value.PlayerCameras.Length; i++)
			{
				player.Value.PlayerCameras[i].enabled = false;
			}

			oldPlayers.Add(player.Value);
		}
		
		FindObjectOfType<InGameManager>().SpawnNewPlayers(PhotonNetwork.playerList.Length);

		List<int> karmaValues = new List<int>();

		Player masterC = null;

		foreach (Player player in oldPlayers)
		{
			Debug.Log("Removing all players");
			if(player.photonView.owner.IsMasterClient == true)
			{
				masterC = player;
				continue;
			}
			UpdateManager.RemoveSpecificItemAndDestroyIt(player);
		}

		if (masterC != null)
		{
			Destroy(masterC.gameObject); 
		}

		allRemainingPlayers.Clear();

		if (TTTGameStarted == false)
		{
			StartCoroutine(StartTTTCountdown());
		}
	}

	private IEnumerator StartTTTCountdown()
	{
		yield return GameManager.GetInstance().InGame = true;

		yield return new WaitForSeconds(GameManager.GetInstance().TTTWarmupTime);
		GameManager.GetInstance().TTTWarmingUp = false;

		if (PhotonNetwork.isMasterClient == true)
		{
			AssignTeams();
		}

		TTTGameStarted = true;
	}

	private IEnumerator StartTTTCooldown()
	{
		TTTGameStarted = false;

		yield return new WaitForSeconds(GameManager.GetInstance().TTTWarmupTime);

		ResetTTTGame();
	}

	public void LaunchTTTVictoryScreen()
	{
		if (TTTGameStarted == true)
		{
			StartCoroutine(StartTTTCooldown());
		}
	}

	private void AssignTeams()
	{
		int numberOfTraitors = Mathf.FloorToInt(PhotonNetwork.playerList.Length * GameManager.GetInstance().TerroristSpawnRate);
		int numberOfDetectives = Mathf.FloorToInt(PhotonNetwork.playerList.Length * GameManager.GetInstance().DetectiveSpawnRate);
		List<Player> allPlayers = new List<Player>(PhotonNetwork.playerList.Length);

		foreach (KeyValuePair<int, Player> player in allRemainingPlayers)
		{
			allPlayers.Add(player.Value);
		}

		for (int i = 0; i < numberOfTraitors; i++)
		{
			Player selectedPlayer = allPlayers[UnityEngine.Random.Range(0, allPlayers.Count)];
			selectedPlayer.photonView.RPC("UpdateTeamStatus", PhotonTargets.All, TTTTeams.Traitor);
			allRemainingTraitorPlayers.Add(selectedPlayer);
			allPlayers.Remove(selectedPlayer);
		}

		for (int i = 0; i < numberOfDetectives; i++)
		{
			Player selectedPlayer = allPlayers[UnityEngine.Random.Range(0, allPlayers.Count)];
			selectedPlayer.photonView.RPC("UpdateTeamStatus", PhotonTargets.All, TTTTeams.Detective);
			allRemainingInnocentPlayers.Add(selectedPlayer);
			allPlayers.Remove(selectedPlayer);
		}

		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player selectedPlayer = allPlayers[UnityEngine.Random.Range(0, allPlayers.Count)];
			selectedPlayer.photonView.RPC("UpdateTeamStatus", PhotonTargets.All, TTTTeams.Innocent);
			allRemainingInnocentPlayers.Add(selectedPlayer);
			allPlayers.Remove(selectedPlayer);
		}

		foreach (KeyValuePair<int, Player> player in allRemainingPlayers)
		{
			player.Value.SetupTTTTeamColors();

			if (ownClient.photonView.viewID != player.Key)
			{
				if (ownClient.CurrentTTTTeam == TTTTeams.Traitor && player.Value.CurrentTTTTeam == TTTTeams.Traitor)
				{
					player.Value.MinimapDot.SetActive(true);
				}
				else
				{
					player.Value.MinimapDot.SetActive(false);
				}
			}
		}
	}
}
