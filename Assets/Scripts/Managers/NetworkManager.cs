using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : OverridableMonoBehaviour
{
	private Player masterClient;
	private Player ownClient;
	private Dictionary<int, Player> allRemainingPlayers = new Dictionary<int, Player>();

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

	public override void UpdateMe()
	{
		if(SceneManager.GetActiveScene().buildIndex == 1)
		{
			if(PhotonNetwork.offlineMode == false)
			{
				if (allRemainingPlayers.Count != PhotonNetwork.playerList.Length)
				{
					FindPlayers();
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
	}

	public void FindPlayers()
	{
		allRemainingPlayers.Clear();

		Player[] players = FindObjectsOfType<Player>();
		
		if (players != null)
		{
			for (int i = 0; i < players.Length; i++)
			{
				if (PhotonNetwork.offlineMode == false)
				{
					if (PhotonPlayer.Find(players[i].photonView.OwnerActorNr).isMasterClient)
					{
						masterClient = players[i];
					}

					if (PhotonPlayer.Find(players[i].photonView.OwnerActorNr).ID == PhotonNetwork.player.ID)
					{
						ownClient = players[i];
					}
				}

				if (allRemainingPlayers.ContainsValue(players[i]) == false)
				{
					if (players[i].enabled == true)
					{
						if (PhotonNetwork.offlineMode == true)
						{
							allRemainingPlayers.Add(players[i].GetInstanceID(), players[i]);
						}
						else
						{
							allRemainingPlayers.Add(players[i].photonView.viewID, players[i]);
						}
					}
					else
					{
						continue;
					}
				}
				else
				{
					continue;
				}
			}
		}
	}

	public void CreateRoom(bool isVisible, bool isOpen, int maxPlayers)
	{
		string roomName = PhotonNetwork.player.name;

		RoomOptions roomOptions = new RoomOptions();

		roomOptions.isVisible = isVisible;
		roomOptions.isOpen = isOpen;
		roomOptions.maxPlayers = Convert.ToByte(maxPlayers);

		PhotonNetwork.CreateRoom(CheckForViableRoomname(roomName), roomOptions, null);
	}

	string CheckForViableRoomname(string nameToValidate)
	{
		int counter = 0;

		for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
		{
			if(nameToValidate == PhotonNetwork.GetRoomList()[i].name)
			{
				counter++;
			}
		}

		if(counter == 0)
		{
			return nameToValidate;
		}
		else
		{
			return CheckForViableRoomname(nameToValidate + " 1");
		}
	}

	void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if(player == PhotonNetwork.masterClient)
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
		PhotonNetwork.room.visible = false;
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

	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.Disconnect();
	}

	public void LeaveLobby()
	{
		PhotonNetwork.LeaveLobby();
	}
}
