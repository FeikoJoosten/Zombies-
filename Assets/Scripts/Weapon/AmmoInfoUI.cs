using UnityEngine;
using UnityEngine.UI;

public class AmmoInfoUI : MonoBehaviour
{
	[SerializeField]
	private Text magText = null;
	[SerializeField]
	private Text totalAmmoText = null;

	public void UpdateMagText(int ammoCount)
	{
		magText.text = ammoCount.ToString();
	}

	public void UpdateAmmoText(int ammoCount)
	{
		totalAmmoText.text = ammoCount.ToString();
	}
}
