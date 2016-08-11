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
	[SerializeField]
	private RectTransform terroristTab;
	[SerializeField]
	private RectTransform miaTab;
	[SerializeField]
	private RectTransform confirmedDeadTab;
	[SerializeField]
	private Text karmaTitleText;

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

	public void SetupTTTTeamColors()
	{
		foreach(KeyValuePair<int, Player> player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
		{
			Player ownClient = GameManager.GetInstance().GetNetworkManager().OwnClient;

			if (ownClient.CurrentTTTTeam == TTTTeams.Traitor && player.Value.CurrentTTTTeam == TTTTeams.Traitor)
			{
				playerScores[player.Key].ChangeBackgroundColor(Color.red);
			}
			else if ((ownClient.CurrentTTTTeam == TTTTeams.Detective || ownClient.CurrentTTTTeam == TTTTeams.Innocent) && player.Value.CurrentTTTTeam == TTTTeams.Traitor)
			{
				continue;
			}
			else if(player.Value.CurrentTTTTeam == TTTTeams.Detective)
			{
				playerScores[player.Key].ChangeBackgroundColor(Color.blue);
			}
		}
	}

	public void ChangePlayersTeamTab(int viewID, TTTTeamStatus status)
	{
		switch(status)
		{
			case TTTTeamStatus.Terrorist:
			playerScores[viewID].transform.SetParent(terroristTab.transform);
			break;
			case TTTTeamStatus.MissingInAction:
			playerScores[viewID].transform.SetParent(miaTab.transform);
			break;
			case TTTTeamStatus.ConfirmedDead:
			playerScores[viewID].transform.SetParent(confirmedDeadTab.transform);
			break;
		}

		playerScores[viewID].transform.SetAsLastSibling();
	}

	IEnumerator WaitForPlayers()
	{
		playerScores.Clear();

		if(GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
		{
			karmaTitleText.gameObject.SetActive(true);
		}

		while (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count != PhotonNetwork.playerList.Length)
		{
			yield return null;
		}

		uIParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 180 + (60 * PhotonNetwork.playerList.Length));

		foreach(var player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
		{
			playerScores.Add(player.Value.photonView.viewID, Instantiate(playerNamePrefab));
			playerScores[player.Value.photonView.viewID].transform.SetParent(terroristTab.transform);
			playerScores[player.Value.photonView.viewID].transform.SetAsLastSibling();
			playerScores[player.Value.photonView.viewID].GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
			playerScores[player.Value.photonView.viewID].transform.rotation = new Quaternion();
			playerScores[player.Value.photonView.viewID].transform.localScale = Vector3.one;
			playerScores[player.Value.photonView.viewID].UpdatePlayerName(player.Value.name);

			if(GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
			{
				playerScores[player.Value.photonView.viewID].EnableKarmaText();
			}

			ResetKillCountForPlayer(player.Value.photonView.viewID);
			ResetKarmaCountForPlayer(player.Value.photonView.viewID);
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

	[PunRPC]
	public void AddNumberToKarmaCountForPlayer(int playerID, int karmaCount)
	{
		foreach (var player in playerScores)
		{
			if (player.Key == playerID)
			{
				player.Value.UpdateKarmaCount(karmaCount);
			}
		}
	}

	void ResetKillCountForPlayer(int playerID)
	{
		playerScores[playerID].ResetKillCountText();
	}

	void ResetKarmaCountForPlayer(int playerID)
	{
		playerScores[playerID].ResetKarmaText();
	}
}
