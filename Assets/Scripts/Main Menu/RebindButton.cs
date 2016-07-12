using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class RebindButton : MonoBehaviour, IPointerClickHandler
{
	public InControl.PlayerAction action { get; set; }
	public InControl.BindingSource binding { get; set; }

	MainMenuManager mainMenuManager;
	PauseMenuManager pauseMenuManager;

	void Start()
	{
		mainMenuManager = FindObjectOfType<MainMenuManager>();
		pauseMenuManager = FindObjectOfType<PauseMenuManager>();
	}

	public void OnPointerClick(PointerEventData data)
	{
		if (mainMenuManager != null)
		{
			mainMenuManager.ChangeBinding(new System.Collections.Generic.KeyValuePair<InControl.PlayerAction, InControl.BindingSource>(action, binding)); 
		}

		if(pauseMenuManager != null)
		{
			pauseMenuManager.ChangeBinding(new System.Collections.Generic.KeyValuePair<InControl.PlayerAction, InControl.BindingSource>(action, binding));
		}
	}
}
