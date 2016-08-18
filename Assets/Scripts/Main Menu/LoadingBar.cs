using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingBar : MonoBehaviour
{
	[SerializeField]
	private Text ownerText;
	[SerializeField]
	private Text progressText;
	[SerializeField]
	private Image loadingBar;

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
