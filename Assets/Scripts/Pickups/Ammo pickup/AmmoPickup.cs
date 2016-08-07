using UnityEngine;
using System.Collections;

public class AmmoPickup : OverridableMonoBehaviour
{
	[SerializeField]
	private int amountToReload;
	[SerializeField]
	private int weaponToReload;
	[SerializeField]
	private float rotationSpeed;
	[SerializeField]
	private float trailMovementSpeed;
	[SerializeField]
	private TrailRenderer trail;
	[SerializeField]
	private ParticleSystem particles;
	[SerializeField]
	private Vector3 trailEndPosition;

	private Vector3 trailStartPosition;
	private float currentMovementSpeed;
	private float currentMovementPercentage;
	private bool gaveAmmo = false;
	private bool shouldShowLineRenderer = true;

	public bool ShouldShowLineRenderer
	{
		get { return shouldShowLineRenderer; }
		set { shouldShowLineRenderer = value; }
	}

	void Start()
	{
		trailStartPosition = trail.transform.localPosition;

		if (shouldShowLineRenderer == false)
		{
			trail.enabled = false;
			particles.Stop(true);
		}
	}

	public override void UpdateMe()
	{
		if (shouldShowLineRenderer == true)
		{
			transform.Rotate(transform.up * rotationSpeed * Time.deltaTime);
			trail.transform.localPosition = Vector3.Lerp(trailStartPosition, trailStartPosition + trailEndPosition, currentMovementPercentage);

			if (currentMovementSpeed < trailMovementSpeed)
			{
				currentMovementSpeed += Time.deltaTime;
				currentMovementPercentage = currentMovementSpeed / trailMovementSpeed;
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Player player = other.GetComponent<Player>();
		if (player != null)
		{
			if (player.GetWeaponInformation(weaponToReload).CurrentTotalAmmunitionLeft >= player.GetWeaponInformation(weaponToReload).MaxAmmunitionCount)
			{
				return;
			}
			else
			{
				if (gaveAmmo == false)
				{
					if (player.CurrentWeapon != null)
					{
						if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
						{
							player.PickupWeapon(weaponToReload);
						}
						player.GetWeaponInformation(weaponToReload).AddAmmoToAmmoPile(amountToReload);
					}
					gaveAmmo = true;

					if (PhotonNetwork.isMasterClient == true)
					{
						UpdateManager.RemoveSpecificItemAndDestroyIt(this); 
					}
				}
			}
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting == true && PhotonNetwork.isMasterClient == true)
		{
			stream.SendNext(shouldShowLineRenderer);
		}
		else
		{
			shouldShowLineRenderer = (bool)stream.ReceiveNext();
			if (shouldShowLineRenderer == false)
			{
				trail.enabled = false;
				particles.Stop(true);
			}
		}
	}
}
