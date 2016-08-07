using UnityEngine;
using System.Collections;

public class PlayerBody : MonoBehaviour
{
	[SerializeField]
	private Player player;
	[SerializeField]
	private PlayerBodyType bodyType;

	void Start()
	{
		if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
		{
			enabled = false;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if(PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		Bullet bullet = other.GetComponent<Bullet>();

		if (bullet != null)
		{
			if (bullet.TouchedObject == false)
			{
				if (PhotonNetwork.offlineMode == false)
				{
					PhotonNetwork.RPC(player.photonView, "RemoveHealthFromPlayerBody", PhotonTargets.Others, false, bullet.Damage, bodyType);
				}
				if (player != null)
				{
					player.RemoveHealthFromPlayerBody(bullet.Damage, bodyType);
				}
				bullet.TouchedObject = true;
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		Grenade grenade = other.GetComponent<Grenade>();

		if (grenade != null)
		{
			if (grenade.TouchedObject == false)
			{
				if (grenade.IsExploded == true)
				{
					if (PhotonNetwork.offlineMode == false)
					{
						PhotonNetwork.RPC(player.photonView, "RemoveHealthFromPlayerBody", PhotonTargets.Others, false, grenade.Damage, bodyType);
					}
					if (player != null)
					{
						player.RemoveHealthFromPlayerBody(grenade.Damage, bodyType);
					}
				}
				grenade.TouchedObject = true;
			}
		}
	}
}
