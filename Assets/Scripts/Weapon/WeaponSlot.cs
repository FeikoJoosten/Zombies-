using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
	[SerializeField]
	private Weapon holdedWeapon = null;
	[SerializeField]
	private Image activeWeaponImage = null;
	[SerializeField]
	private Image inactiveWeaponImage = null;

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
