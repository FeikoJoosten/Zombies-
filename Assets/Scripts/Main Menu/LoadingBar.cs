using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
	[SerializeField]
	private Text ownerText = null;
	[SerializeField]
	private Text progressText = null;
	[SerializeField]
	private Image loadingBar = null;

	public string OwnerText
	{
		set { ownerText.text = value; }
	}
	public string ProgressText
	{
		set { progressText.text = value; }
	}
	public float LoadingBarProgress
	{
		set { loadingBar.fillAmount = value; }
	}
}
