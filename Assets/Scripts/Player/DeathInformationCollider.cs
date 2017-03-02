using UnityEngine;

public class DeathInformationCollider : MonoBehaviour
{
	[SerializeField]
	private Player player = null;

	public Player Player
	{
		get { return player; }
	}
}
