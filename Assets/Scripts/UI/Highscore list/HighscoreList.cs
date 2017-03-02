using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HighscoreList : Photon.MonoBehaviour
{
	[SerializeField]
	private PlayerScoreInfo playerNamePrefab = null;
	[SerializeField]
	private RectTransform prefabParent = null;
	[SerializeField]
	private RectTransform uIParent = null;
	[SerializeField]
	private RectTransform terroristTab = null;
	[SerializeField]
	private RectTransform miaTab = null;
	[SerializeField]
	private RectTransform confirmedDeadTab = null;
	[SerializeField]
	private ScrollRect scrollRect = null;
	[SerializeField]
	private Text karmaTitleText = null;

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
		playerScores[playerID].UpdatePlayerName(PhotonNetwork.player.NickName);
		ResetKillCountForPlayer(playerID);
	}

	public void CreateMultiplayerHighScoreList()
	{
		StartCoroutine(WaitForPlayers());
	}

	public void SetupTTTTeamColors()
	{
		foreach (KeyValuePair<int, Player> player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
		{
			Player ownClient = GameManager.GetInstance().GetNetworkManager().OwnClient;

			if (ownClient.CurrentTTTTeam == TTTTeams.Traitor && player.Value.CurrentTTTTeam == TTTTeams.Traitor)
			{
				playerScores[player.Key].ChangeBackgroundColor(Color.red);
			}
			else if (player.Value.CurrentTTTTeam == TTTTeams.Detective)
			{
				playerScores[player.Key].ChangeBackgroundColor(Color.blue);
			}
		}
	}

	public void ChangePlayersTeamTabOnDeath(int viewID, TTTTeams ownPlayersTeamStatus)
	{
		if (ownPlayersTeamStatus != TTTTeams.Traitor) return;

		playerScores[viewID].transform.SetParent(miaTab.transform);
		playerScores[viewID].transform.SetAsLastSibling();
	}

	public void ChangePlayersTeamTabOnDeathConfirmation(int viewID)
	{
		if(playerScores[viewID].transform.parent == confirmedDeadTab.transform)
		{
			return;
		}

		playerScores[viewID].transform.SetParent(confirmedDeadTab.transform);

		if(GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers[viewID].CurrentTTTTeam == TTTTeams.Traitor)
		{
			playerScores[viewID].ChangeBackgroundColor(Color.red);
		}

		playerScores[viewID].transform.SetAsLastSibling();
	}

	private IEnumerator WaitForPlayers()
	{
		playerScores.Clear();

		if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
		{
			karmaTitleText.gameObject.SetActive(true);
			terroristTab.gameObject.SetActive(true);
			miaTab.gameObject.SetActive(true);
			confirmedDeadTab.gameObject.SetActive(true);
		}
		else
		{
			karmaTitleText.gameObject.SetActive(false);
			terroristTab.gameObject.SetActive(false);
			miaTab.gameObject.SetActive(false);
			confirmedDeadTab.gameObject.SetActive(false);
		}

		while (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count != PhotonNetwork.playerList.Length)
		{
			yield return null;
		}

		scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 180 + (60 * PhotonNetwork.playerList.Length));

		foreach (var player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
		{
			playerScores.Add(player.Value.photonView.viewID, Instantiate(playerNamePrefab));

			if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
			{
				playerScores[player.Value.photonView.viewID].transform.SetParent(terroristTab.transform);
			}

			playerScores[player.Value.photonView.viewID].transform.SetAsLastSibling();
			playerScores[player.Value.photonView.viewID].GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
			playerScores[player.Value.photonView.viewID].transform.rotation = new Quaternion();
			playerScores[player.Value.photonView.viewID].transform.localScale = Vector3.one;
			playerScores[player.Value.photonView.viewID].UpdatePlayerName(player.Value.name);

			if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
			{
				playerScores[player.Value.photonView.viewID].EnableKarmaText();
				ResetKarmaCountForPlayer(player.Value.photonView.viewID);
			}

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

	private void ResetKillCountForPlayer(int playerID)
	{
		playerScores[playerID].ResetKillCountText();
	}

	private void ResetKarmaCountForPlayer(int playerID)
	{
		playerScores[playerID].ResetKarmaText();
	}
}
