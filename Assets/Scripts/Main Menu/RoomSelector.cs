using UnityEngine;
using UnityEngine.UI;

public class RoomSelector : MonoBehaviour
{
	[SerializeField]
	private Text roomName = null;
	[SerializeField]
	private Text currentPlayerCount = null;
	[SerializeField]
	private Text maxPlayerCount = null;
	[SerializeField]
	private Text gameType = null;

	public string RoomName
	{
		get { return roomName.text; }
		set { roomName.text = value; }
	}
	public string CurrentPlayerCount
	{
		set { currentPlayerCount.text = value; }
	}
	public string MaxPlayerCount
	{
		set { maxPlayerCount.text = value; }
	}
	public string GameType
	{
		get { return gameType.text; }
		set { gameType.text = value; }
	}
}
