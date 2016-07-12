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
	private Vector3 trailEndPosition;

	private Vector3 trailStartPosition;
	private float currentMovementSpeed;
	private float currentMovementPercentage;
	private bool gaveAmmo = false;

	void Start()
	{
		trailStartPosition = trail.transform.localPosition;
	}

	public override void UpdateMe()
	{
		transform.Rotate(transform.up * rotationSpeed * Time.deltaTime);
		trail.transform.localPosition = Vector3.Lerp(trailStartPosition, trailStartPosition + trailEndPosition, currentMovementPercentage);

		if(currentMovementSpeed < trailMovementSpeed)
		{
			currentMovementSpeed += Time.deltaTime;
			currentMovementPercentage = currentMovementSpeed / trailMovementSpeed;
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
				if(gaveAmmo == false)
				{
					if (player.CurrentWeapon != null)
					{
						player.GetWeaponInformation(weaponToReload).AddAmmoToAmmoPile(amountToReload); 
					}
					gaveAmmo = true;

					UpdateManager.RemoveSpecificItemAndDestroyIt(this);
				}
			}
		}
	}
}
