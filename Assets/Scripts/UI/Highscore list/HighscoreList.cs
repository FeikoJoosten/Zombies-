using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class HighscoreList : Photon.MonoBehaviour
{
	[SerializeField]
	private PlayerScoreInfo playerNamePrefab;
	[SerializeField]
	private RectTransform prefabParent;
	[SerializeField]
	private RectTransform uIParent;

	public GameObject UIParent
	{
		get { return uIParent.gameObject; }
	}

	private Dictionary<int, PlayerScoreInfo> playerScores = new Dictionary<int, PlayerScoreInfo>();

	public void CreateSingleplayerHighScoreList(int playerID)
	{
		playerScores.Add(playerID, Instantiate(playerNamePrefab));
		playerScores[playerID].transform.SetParent(prefabParent.transform);
		playerScores[playerID].GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
		playerScores[playerID].transform.rotation = new Quaternion();
		playerScores[playerID].transform.localScale = Vector3.one;
		playerScores[playerID].UpdatePlayerName(PhotonNetwork.player.name);
		ResetKillCountForPlayer(playerID);
	}

	public void CreateMultiplayerHighScoreList()
	{
		StartCoroutine(WaitForPlayers());
	}

	IEnumerator WaitForPlayers()
	{
		playerScores.Clear();

		while (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count != PhotonNetwork.playerList.Length)
		{
			yield return null;
		}

		foreach(var player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
		{
			playerScores.Add(player.Value.photonView.viewID, Instantiate(playerNamePrefab));
			playerScores[player.Value.photonView.viewID].transform.SetParent(prefabParent.transform);
			playerScores[player.Value.photonView.viewID].GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
			playerScores[player.Value.photonView.viewID].transform.rotation = new Quaternion();
			playerScores[player.Value.photonView.viewID].transform.localScale = Vector3.one;
			playerScores[player.Value.photonView.viewID].UpdatePlayerName(player.Value.name);
			ResetKillCountForPlayer(player.Value.photonView.viewID);
		}

		UIParent.gameObject.SetActive(false);
	}

	[PunRPC]
	public void AddNumberToKillCountForPlayer(int playerID, int killCount)
	{
		foreach (var player in playerScores)
		{
			if (player.Key == playerID)
			{
				player.Value.UpdateKillCount(killCount);
			}
		}
	}

	void ResetKillCountForPlayer(int playerID)
	{
		playerScores[playerID].UpdateKillCount(0);
	}
}
