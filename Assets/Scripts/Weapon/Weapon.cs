using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
	[SerializeField]
	private float reloadTime = 0;
	[SerializeField]
	private float bulletsPerSecond = 0;
	[SerializeField]
	private float waitBeforeFireTime = 0;
	[SerializeField]
	private int maxAmmunitionInMagCount = 0;
	[SerializeField]
	private int maxAmmunitionCount = 0;
	[SerializeField]
	private int startingAmmunitionCount = 0;
	[SerializeField]
	private bool isAllowedToUseAutoFire = false;
	[SerializeField]
	private bool hasInfiniteAmmo = false;
	[SerializeField]
	private Transform spawnLocation = null;
	[SerializeField]
	private Player player = null;
	[SerializeField]
	private WeaponType weapon = WeaponType.Pistol;

	private bool isAllowedToUse;
	private bool isWaitTimerFinished = true;
	private bool isAllowedToFire = true;
	private int currentTotalAmmunitionLeft;
	private int currentAmmunitionInMagLeft;

	public float ReloadTime
	{
		get { return reloadTime; }
	}
	public int MaxAmmunitionCount
	{
		get { return maxAmmunitionCount; }
	}
	public int MaxAmmunitionInMagCount
	{
		get { return maxAmmunitionInMagCount; }
	}
	public int CurrentTotalAmmunitionLeft
	{
		get { return currentTotalAmmunitionLeft; }
	}
	public int CurrentAmmunitionInMagLeft
	{
		get { return currentAmmunitionInMagLeft; }
		set { currentAmmunitionInMagLeft = value; }
	}
	public bool IsAllowedToUse
	{
		get { return isAllowedToUse; }
		set { isAllowedToUse = value; }
	}
	public bool IsAllowedToUseAutoFire
	{
		get { return isAllowedToUseAutoFire; }
	}
	public bool IsAllowedToFire
	{
		get { return isAllowedToFire; }
	}
	public bool IsWaitTimerFinished
	{
		get { return isWaitTimerFinished; }
		set { isWaitTimerFinished = value; }
	}
	public bool HasInfiniteAmmo
	{
		get { return hasInfiniteAmmo; }
	}
	public Transform SpawnLocation
	{
		get { return spawnLocation; }
	}
	public Player Player
	{
		get { return player; }
	}
	public WeaponType WeaponType
	{
		get { return weapon; }
	}
	
	public void AssignStartingAmmo()
	{
		currentAmmunitionInMagLeft = maxAmmunitionInMagCount;
		currentTotalAmmunitionLeft = startingAmmunitionCount;
	}

	public virtual void Fire()
	{
	}

	public void ReloadWeapon()
	{
		if (hasInfiniteAmmo == true)
		{
			currentAmmunitionInMagLeft = maxAmmunitionInMagCount;
			return;
		}

		int amountToReload = maxAmmunitionInMagCount - currentAmmunitionInMagLeft;
		if (amountToReload <= currentTotalAmmunitionLeft)
		{
			currentAmmunitionInMagLeft += amountToReload;
			currentTotalAmmunitionLeft -= amountToReload;
		}
		else
		{
			currentAmmunitionInMagLeft += currentTotalAmmunitionLeft;
			currentTotalAmmunitionLeft -= currentTotalAmmunitionLeft;
		}
	}

	public void AddAmmoToAmmoPile(int amountToAdd)
	{
		if (currentTotalAmmunitionLeft + amountToAdd > maxAmmunitionCount)
		{
			currentTotalAmmunitionLeft = maxAmmunitionCount;
		}
		else
		{
			currentTotalAmmunitionLeft += amountToAdd;
		}
	}

	public void ClearAmmoPile()
	{
		currentAmmunitionInMagLeft = 0;
		currentTotalAmmunitionLeft = 0;
	}

	public IEnumerator WaitBeforeFire()
	{
		isWaitTimerFinished = false;
		yield return new WaitForSeconds(waitBeforeFireTime);
		isWaitTimerFinished = true;
	}

	public IEnumerator FireRateCountDown()
	{
		isAllowedToFire = false;

		yield return new WaitForSeconds(1 / bulletsPerSecond);

		Fire();
		isAllowedToFire = true;
	}
}
