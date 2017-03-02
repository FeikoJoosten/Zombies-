using UnityEngine;
using UnityEngine.UI;

public class ControllChanger : MonoBehaviour
{
	[SerializeField]
	private Button controllBindingPrefab = null;
	[SerializeField]
	private Text controllName = null;

	public Button ControllBinding
	{
		get { return controllBindingPrefab; }
	}
	public string ControllName
	{
		get { return controllName.text; }
		set { controllName.text = value; }
	}
}
