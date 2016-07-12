using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ControllChanger : MonoBehaviour
{
	[SerializeField]
	private Button controllBindingPrefab;
	[SerializeField]
	private Text controllName;

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
