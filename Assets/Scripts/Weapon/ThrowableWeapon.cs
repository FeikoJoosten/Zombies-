using UnityEngine;
using System.Collections;

public class ThrowableWeapon : Weapon
{
	[SerializeField]
	private Grenade GrenadePrefab;
	[SerializeField]
	private float throwSpeed;

	private PhotonView photonView;

	void Start()
	{
		if (PhotonNetwork.offlineMode == false)
		{
			photonView = GetComponent<PhotonView>();
		}
	}

	public override void Fire()
	{
		if (IsWaitTimerFinished)
		{
			if (IsAllowedToFire == true && Player.IsReloading == false)
			{
				if (CurrentAmmunitionInMagLeft <= 0)
				{
					if (CurrentTotalAmmunitionLeft > 0)
					{
						Player.ReloadWeapon();
						return;
					}
					else
					{
						return;
					}
				}
				else
				{
					if (HasInfiniteAmmo == false)
					{
						CurrentAmmunitionInMagLeft--;
					}
				}


				if (PhotonNetwork.offlineMode == true)
				{
					Grenade grenade = (Grenade)Instantiate(GrenadePrefab, SpawnLocation.position, SpawnLocation.rotation * GrenadePrefab.transform.rotation);
					grenade.OwnerID = Player.gameObject.GetInstanceID();
					grenade.Rig.AddForce(SpawnLocation.transform.forward * throwSpeed);
				}
				else
				{
					//if (PhotonNetwork.isMasterClient == true)
					//{
					//	Grenade grenade = PhotonNetwork.InstantiateSceneObject(GrenadePrefab.name, SpawnLocation.position, SpawnLocation.rotation * GrenadePrefab.transform.rotation, 0, null).GetComponent<Grenade>();
					//	grenade.GetComponent<Grenade>().OwnerID = Player.photonView.viewID;
					//	grenade.Rig.AddForce(SpawnLocation.transform.forward * throwSpeed);
					//}
					//else
					//{
					//	photonView.RPC("InstantiateGrenade", PhotonTargets.MasterClient, SpawnLocation.position, SpawnLocation.rotation * GrenadePrefab.transform.rotation, Player.photonView.viewID);
					//}
					Grenade grenade = (Grenade)Instantiate(GrenadePrefab, SpawnLocation.position, SpawnLocation.rotation * GrenadePrefab.transform.rotation);
					grenade.OwnerID = Player.photonView.viewID;
					grenade.Rig.AddForce(SpawnLocation.transform.forward * throwSpeed);
					photonView.RPC("InstantiateGrenade", PhotonTargets.Others, Player.photonView.viewID);
				}

				Player.ReloadWeapon();

				StartCoroutine(WaitBeforeFire());

				if (IsAllowedToUseAutoFire == true)
				{
					StartCoroutine(FireRateCountDown());
				}
			}
		}
	}

	[PunRPC]
	void InstantiateGrenade(int ownerID)
	{
		//Grenade grenade = PhotonNetwork.InstantiateSceneObject(GrenadePrefab.name, spawnPosition, spawnRotation, 0, null).GetComponent<Grenade>();
		//grenade.OwnerID = Player.GetInstanceID();
		//grenade.Rig.AddForce(SpawnLocation.transform.forward * throwSpeed);
		Grenade grenade = (Grenade)Instantiate(GrenadePrefab, SpawnLocation.position, SpawnLocation.rotation * GrenadePrefab.transform.rotation);
		grenade.OwnerID = Player.photonView.viewID;
		grenade.Rig.AddForce(SpawnLocation.transform.forward * throwSpeed);
	}
}
