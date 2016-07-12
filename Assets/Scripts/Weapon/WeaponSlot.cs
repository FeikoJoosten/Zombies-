using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponSlot : MonoBehaviour
{
	[SerializeField]
	private Weapon holdedWeapon;
	[SerializeField]
	private Image activeWeaponImage;
	[SerializeField]
	private Image inactiveWeaponImage;

	public Weapon HoldedWeapon
	{
		get { return holdedWeapon; }
	}
	public Image ActiveWeaponImage
	{
		get { return activeWeaponImage; }
	}
	public Image InactiveWeaponImage
	{
		get { return inactiveWeaponImage; }
	}
}
