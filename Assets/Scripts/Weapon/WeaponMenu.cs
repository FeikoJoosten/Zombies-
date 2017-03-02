using UnityEngine;
using UnityEngine.UI;

public class WeaponMenu : OverridableMonoBehaviour
{
	[SerializeField]
	private Image weaponSelector = null;
	[SerializeField]
	private float selectorMovementSpeed = 0;
	[SerializeField]
	private WeaponSlot[] weaponSlots = null;

	private int currentSelectedWeaponSlot;
	private bool isEnabled;

	public bool IsEnabled
	{
		get { return isEnabled; }
	}

	private void Start()
	{
		for (int i = 0; i < weaponSlots.Length; i++)
		{
			if (weaponSlots[i].HoldedWeapon.IsAllowedToUse == true)
			{
				weaponSlots[i].ActiveWeaponImage.gameObject.SetActive(true);
				weaponSlots[i].InactiveWeaponImage.gameObject.SetActive(false);
			}
			else
			{
				weaponSlots[i].ActiveWeaponImage.gameObject.SetActive(false);
				weaponSlots[i].InactiveWeaponImage.gameObject.SetActive(true);
			}
		}

		ResetWeaponSelector();
	}

	public override void UpdateMe()
	{
		if (weaponSelector.transform.position != Vector3.zero)
		{
			weaponSelector.transform.localPosition = Vector3.Lerp(weaponSelector.transform.localPosition, Vector3.zero, selectorMovementSpeed * Time.deltaTime);
		}
	}

	public void ToggleActiveState()
	{
		if (weaponSelector.gameObject.activeInHierarchy == true)
		{
			weaponSelector.gameObject.SetActive(false);
			isEnabled = false;
		}
		else
		{
			weaponSelector.gameObject.SetActive(true);
			isEnabled = true;
		}

		for (int i = 0; i < weaponSlots.Length; i++)
		{
			if (weaponSlots[i].gameObject.activeInHierarchy == true)
			{
				weaponSlots[i].ActiveWeaponImage.gameObject.SetActive(false);
				weaponSlots[i].InactiveWeaponImage.gameObject.SetActive(false);
				weaponSlots[i].gameObject.SetActive(false);
			}
			else
			{
				weaponSlots[i].gameObject.SetActive(true);
				if (weaponSlots[i].HoldedWeapon.IsAllowedToUse == true)
				{
					weaponSlots[i].ActiveWeaponImage.gameObject.SetActive(true);
					weaponSlots[i].InactiveWeaponImage.gameObject.SetActive(false);
				}
				else
				{
					weaponSlots[i].InactiveWeaponImage.gameObject.SetActive(true);
					weaponSlots[i].ActiveWeaponImage.gameObject.SetActive(false);
				}
			}
		}
	}

	public void ResetWeaponSelector()
	{
		weaponSelector.transform.SetParent(weaponSlots[currentSelectedWeaponSlot].transform);
		weaponSelector.transform.SetAsFirstSibling();
		weaponSelector.transform.position = Vector3.zero;
	}

	public int ReturnCurrentSelectedWeapon()
	{
		return currentSelectedWeaponSlot;
	}

	public void SelectSpecificWeapon(int weaponToSelect)
	{
		currentSelectedWeaponSlot = weaponToSelect;
		ResetWeaponSelector();
		weaponSelector.transform.position = weaponSlots[weaponToSelect].transform.position;
	}

	public void MoveWeaponSelectorToTheRight()
	{
		for (int i = currentSelectedWeaponSlot + 1; i < weaponSlots.Length; i++)
		{
			if (!weaponSlots[i].HoldedWeapon.IsAllowedToUse) continue;

			currentSelectedWeaponSlot = i;
			weaponSelector.transform.SetParent(weaponSlots[currentSelectedWeaponSlot].transform);
			weaponSelector.transform.SetAsFirstSibling();
			return;
		}
		for (int i = 0; i < currentSelectedWeaponSlot + 1; i++)
		{
			if (!weaponSlots[i].HoldedWeapon.IsAllowedToUse) continue;

			currentSelectedWeaponSlot = i;
			weaponSelector.transform.SetParent(weaponSlots[currentSelectedWeaponSlot].transform);
			weaponSelector.transform.SetAsFirstSibling();
			return;
		}
	}

	public void MoveWeaponSelectorToTheLeft()
	{
		for (int i = currentSelectedWeaponSlot - 1; i >= 0; i--)
		{
			if (!weaponSlots[i].HoldedWeapon.IsAllowedToUse) continue;

			currentSelectedWeaponSlot = i;
			weaponSelector.transform.SetParent(weaponSlots[currentSelectedWeaponSlot].transform);
			weaponSelector.transform.SetAsFirstSibling();
			return;
		}
		for (int i = weaponSlots.Length - 1; i >= currentSelectedWeaponSlot - 1; i--)
		{
			if (!weaponSlots[i].HoldedWeapon.IsAllowedToUse) continue;

			currentSelectedWeaponSlot = i;
			weaponSelector.transform.SetParent(weaponSlots[currentSelectedWeaponSlot].transform);
			weaponSelector.transform.SetAsFirstSibling();
			return;
		}
	}
}
