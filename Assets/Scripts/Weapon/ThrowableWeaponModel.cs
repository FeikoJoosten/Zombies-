using UnityEngine;
using System.Collections;

public class ThrowableWeaponModel : MonoBehaviour
{
	[SerializeField]
	private float destroyTime = 3;

	private AmmoPickupSpawner ammoSpawner;
	private int weaponNumber;

	public int WeaponNumber
	{
		get { return weaponNumber;}
		set { weaponNumber = value; }
	}
	// Use this for initialization
	void Start()
	{
		ammoSpawner = FindObjectOfType<AmmoPickupSpawner>();
		StartCoroutine(DestroyWeapon());
	}

	IEnumerator DestroyWeapon()
	{
		yield return new WaitForSeconds(destroyTime);

		if (PhotonNetwork.isMasterClient == true)
		{
			ammoSpawner.SpawnAmmoPickupOnLocation(weaponNumber, transform.position); 
		}
		Destroy(gameObject);
	}
}
