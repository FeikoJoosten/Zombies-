using UnityEngine;
using System.Collections;

public class DeathInformationCollider : MonoBehaviour
{
	[SerializeField]
	private Player player;

	public Player Player
	{
		get { return player; }
	}
}
