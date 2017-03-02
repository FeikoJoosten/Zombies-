using UnityEngine;
using UnityEngine.EventSystems;

public class RebindButton : MonoBehaviour, IPointerClickHandler
{
	public InControl.PlayerAction action { get; set; }
	public InControl.BindingSource binding { get; set; }

	private MainMenuManager mainMenuManager;
	private PauseMenuManager pauseMenuManager;

	private void Start()
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
