using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomSelector : MonoBehaviour
{
	[SerializeField]
	private Text roomName;
	[SerializeField]
	private Text currentPlayerCount;
	[SerializeField]
	private Text maxPlayerCount;

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
}
