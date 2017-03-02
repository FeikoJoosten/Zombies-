using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	[SerializeField]
	private Player player = null;
	[SerializeField]
	private PlayerBodyType bodyType = PlayerBodyType.Body;

	private void Start()
	{
		if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
		{
			enabled = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		Bullet bullet = other.GetComponent<Bullet>();

		if (bullet == null || bullet.TouchedObject != false) return;

		if (PhotonNetwork.offlineMode == false)
		{
			PhotonNetwork.RPC(player.photonView, "RemoveHealthFromPlayerBody", PhotonTargets.Others, false, bullet.Damage, bodyType);
			if(player.CurrentTTTTeam != TTTTeams.Traitor && GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers[bullet.OwnerID].CurrentTTTTeam != TTTTeams.Traitor)
			{
				PhotonNetwork.RPC(player.photonView, "RemoveKarmaPoints", PhotonTargets.Others, false, bullet.Damage, player.photonView.viewID);
			}
		}
		if (player != null)
		{
			player.RemoveHealthFromPlayerBody(bullet.Damage, bodyType, bullet.BulletType);
			if (player.CurrentTTTTeam != TTTTeams.Traitor && GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers[bullet.OwnerID].CurrentTTTTeam != TTTTeams.Traitor)
			{
				player.RemoveKarmaPoints((int)bullet.Damage, player.photonView.viewID);
			}
		}
		bullet.TouchedObject = true;
	}

	private void OnTriggerStay(Collider other)
	{
		if (PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		Grenade grenade = other.GetComponent<Grenade>();

		if (grenade == null || grenade.TouchedObject != false) return;

		if (grenade.IsExploded == true)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				PhotonNetwork.RPC(player.photonView, "RemoveHealthFromPlayerBody", PhotonTargets.Others, false, grenade.Damage, bodyType);
				if (player.CurrentTTTTeam != TTTTeams.Traitor && GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers[grenade.OwnerID].CurrentTTTTeam != TTTTeams.Traitor)
				{
					PhotonNetwork.RPC(player.photonView, "RemoveKarmaPoints", PhotonTargets.Others, false, grenade.Damage, player.photonView.viewID);
				}
			}
			if (player != null)
			{
				player.RemoveHealthFromPlayerBody(grenade.Damage, bodyType, grenade.GrenadeType);
				if (player.CurrentTTTTeam != TTTTeams.Traitor && GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers[grenade.OwnerID].CurrentTTTTeam != TTTTeams.Traitor)
				{
					player.RemoveKarmaPoints((int)grenade.Damage, player.photonView.viewID);
				}
			}
		}
		grenade.TouchedObject = true;
	}
}
