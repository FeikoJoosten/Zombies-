using UnityEngine;
using UnityEngine.UI;

public class AmmoInfoUI : MonoBehaviour
{
	[SerializeField]
	private Text magText;
	[SerializeField]
	private Text totalAmmoText;

	public void UpdateMagText(int ammoCount)
	{
		magText.text = ammoCount.ToString();
	}

	public void UpdateAmmoText(int ammoCount)
	{
		totalAmmoText.text = ammoCount.ToString();
	}
}
