using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PauseMenuManager : OverridableMonoBehaviour
{
	[SerializeField]
	private RectTransform playerControllsHolder = null;
	[SerializeField]
	private ControllChanger playerControllerChangerPrefab = null;
	[SerializeField]
	private Dropdown screenResolutionDropdown = null;
	[SerializeField]
	private RectTransform mainMenu = null;
	[SerializeField]
	private RectTransform optionsMenu = null;
	[SerializeField]
	private RectTransform videoMenu = null;
	[SerializeField]
	private RectTransform exitMenu = null;
	[SerializeField]
	private RectTransform audioMenu = null;
	[SerializeField]
	private RectTransform controlsMenu = null;
	[SerializeField]
	private Toggle useFullscreenToggle = null;
	[SerializeField]
	private Dropdown gameQualityDropdown = null;
	[SerializeField]
	private Player player = null;
	[SerializeField]
	private Slider SFXVolumeSlider = null;
	[SerializeField]
	private Slider MusicVolumeSlider = null;
	[SerializeField]
	private Text SFXVolumeText = null;
	[SerializeField]
	private Text MusicVolumeText = null;

	private int skippedResolutions;
	private PlayerActions playerActions;
	private Dictionary<Button, Dictionary<InControl.PlayerAction, InControl.BindingSource>> bindingButtons = new Dictionary<Button, Dictionary<InControl.PlayerAction, InControl.BindingSource>>();


	private void Start()
	{
		#region GenerateControlPrefabs
		playerActions = PlayerActions.CreateWithDefaultBindings();
		InControl.InputManager.OnDeviceAttached += InputManager_OnDeviceAttached;

		if (PlayerPrefs.HasKey("Bindings"))
		{
			playerActions.Load(PlayerPrefs.GetString("Bindings"));
		}

		RedrawControllsMenu();
		#endregion

		#region GenerateVideoPrefabs
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (Screen.resolutions[i].height < 480)
			{
				skippedResolutions++;
			}
		}

		if (Application.platform != RuntimePlatform.WindowsEditor)
		{
			if (PlayerPrefs.HasKey("QualityLevel") && PlayerPrefs.HasKey("IsFullScreen") && PlayerPrefs.HasKey("ScreenResolution"))
			{
				QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityLevel"));
				Screen.SetResolution(Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width, Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height, PlayerPrefs.GetInt("IsFullScreen") == 1);
			}
		}
		gameQualityDropdown.template.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50 * QualitySettings.names.Length);
		gameQualityDropdown.ClearOptions();
		screenResolutionDropdown.template.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50 * Screen.resolutions.Length);
		screenResolutionDropdown.ClearOptions();

		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			gameQualityDropdown.options.Add(new Dropdown.OptionData() { text = QualitySettings.names[i] });

			if (QualitySettings.GetQualityLevel() != i) continue;

			gameQualityDropdown.value = i;
			gameQualityDropdown.captionText.text = QualitySettings.names[i];
		}

		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (Screen.resolutions[i].height < 480)
			{
				continue;
			}

			if (Screen.fullScreen == false)
			{
				if (Screen.resolutions[i].height == Screen.resolutions[Screen.resolutions.Length - 1].height)
				{
					continue;
				}
			}

			screenResolutionDropdown.options.Add(new Dropdown.OptionData() { text = Screen.resolutions[i].width + " X " + Screen.resolutions[i].height });

			if (Application.platform == RuntimePlatform.WindowsEditor) continue;
			if (Screen.resolutions[i].height !=
				Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height ||
				Screen.resolutions[i].width !=
				Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width) continue;

			screenResolutionDropdown.value = i;
			screenResolutionDropdown.captionText.text = Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width + " X " + Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height;
		}

		useFullscreenToggle.isOn = Screen.fullScreen != true;
		#endregion

		GetAudioSettings();
	}

	private void InputManager_OnDeviceAttached(InControl.InputDevice obj)
	{
		RedrawControllsMenu();
	}

	private void RedrawControllsMenu()
	{
		bindingButtons.Clear();

		for (int j = playerControllsHolder.childCount - 1; j >= 0; j--)
		{
			Transform obj = playerControllsHolder.GetChild(j);
			Destroy(obj.GetComponent<LayoutElement>());
			Destroy(obj.GetComponent<HorizontalLayoutGroup>());
			obj.SetParent(null);
			Destroy(obj.gameObject);
		}

		for (int i = 0; i < playerActions.Actions.Count; i++)
		{
			InControl.PlayerAction action = playerActions.Actions[i];

			if (action == playerActions.rotateUp || action == playerActions.rotateDown || action == playerActions.rotateLeft || action == playerActions.rotateRight)
			{
				continue;
			}

			ControllChanger changer = Instantiate(playerControllerChangerPrefab);

			changer.transform.SetParent(playerControllsHolder.transform);
			changer.GetComponent<HorizontalLayoutGroup>().padding.top = changer.GetComponent<HorizontalLayoutGroup>().padding.top * playerControllsHolder.childCount;
			changer.ControllName = action.Name;

			for (int j = 0; j < action.Bindings.Count; j++)
			{
				Dictionary<InControl.PlayerAction, InControl.BindingSource> acti =
					new Dictionary<InControl.PlayerAction, InControl.BindingSource> { { action, action.Bindings[j] } };

				Button button = Instantiate(changer.ControllBinding);
				bindingButtons.Add(button, acti);
				button.GetComponent<RebindButton>().action = action;
				button.GetComponent<RebindButton>().binding = action.Bindings[j];
				button.transform.SetParent(changer.transform);
				button.GetComponentInChildren<Text>().text = action.Bindings[j].Name;
			}
		}

		playerControllsHolder.sizeDelta = new Vector2(0, (playerControllerChangerPrefab.GetComponent<LayoutElement>().minHeight * playerControllsHolder.childCount) + (playerControllsHolder.GetComponent<VerticalLayoutGroup>().padding.top * 2));

		for (int i = 0; i < playerControllsHolder.childCount; i++)
		{
			playerControllsHolder.GetChild(i).transform.localPosition = Vector3.zero;
			playerControllsHolder.GetChild(i).transform.localRotation = Quaternion.Euler(Vector3.zero);
			playerControllsHolder.GetChild(i).transform.localScale = Vector3.one;
		}
	}

	public void ChangeBinding(KeyValuePair<InControl.PlayerAction, InControl.BindingSource> actionAndBinding)
	{
		foreach (InControl.PlayerAction action in playerActions.Actions)
		{
			action.StopListeningForBinding();
		}

		InControl.BindingListenOptions options = new InControl.BindingListenOptions();

		foreach (Dictionary<InControl.PlayerAction, InControl.BindingSource> i in bindingButtons.Values)
		{
			foreach (InControl.BindingSource value in i.Values)
			{
				if (value != actionAndBinding.Value)
				{
					continue;
				}

				options.IncludeModifiersAsFirstClassKeys = true;

				switch (value.BindingSourceType)
				{
					case InControl.BindingSourceType.DeviceBindingSource:
						options.IncludeControllers = true;
						options.IncludeKeys = false;
						options.IncludeMouseButtons = false;
						break;
					case InControl.BindingSourceType.KeyBindingSource:
						options.IncludeControllers = false;
						options.IncludeKeys = true;
						options.IncludeMouseButtons = false;
						break;
					default:
						options.IncludeControllers = false;
						options.IncludeKeys = false;
						options.IncludeMouseButtons = true;
						break;
				}

				actionAndBinding.Key.ListenOptions = options;

				actionAndBinding.Key.ListenForBindingReplacing(actionAndBinding.Value);
			}
		}
	}

	public void SaveBindings()
	{
		PlayerPrefs.SetString("Bindings", playerActions.Save());
	}

	public void ResetBindings()
	{
		playerActions.Reset();
		RedrawControllsMenu();
	}

	public override void UpdateMe()
	{
		foreach (var button in bindingButtons)
		{
			for (int i = 0; i < button.Value.Count; i++)
			{
				var action = button.Value.ElementAt(i);

				if (action.Key.IsListeningForBinding == false)
				{
					button.Key.GetComponentInChildren<Text>().text = action.Value.Name;
					continue;
				}

				if ((action.Key.ListenOptions.IncludeControllers == true &&
					action.Value.BindingSourceType == InControl.BindingSourceType.DeviceBindingSource) ||
					(action.Key.ListenOptions.IncludeKeys == true &&
					action.Value.BindingSourceType == InControl.BindingSourceType.KeyBindingSource) ||
					(action.Key.ListenOptions.IncludeMouseButtons == true &&
					action.Value.BindingSourceType == InControl.BindingSourceType.MouseBindingSource))
				{
					button.Key.GetComponentInChildren<Text>().text = "Listening";
				}
				else
				{
					button.Key.GetComponentInChildren<Text>().text = action.Value.Name;
				}

				action.Key.ListenOptions.OnBindingAdded = (act, binding) =>
				{
					RedrawControllsMenu();
				};
			}
		}
	}

	public void ResumeGame()
	{
		player.TogglePauseGame();
	}

	public void QuitGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		PhotonNetwork.Disconnect();
	}

	public void SaveAudioSettings()
	{
		GameManager.GetInstance().GetAudioManager().UpdateAudioVolumes(SFXVolumeSlider.normalizedValue, MusicVolumeSlider.normalizedValue);
	}

	public void GetAudioSettings()
	{
		SFXVolumeSlider.value = GameManager.GetInstance().GetAudioManager().SavedSFXVolume;
		MusicVolumeSlider.value = GameManager.GetInstance().GetAudioManager().SavedMusicVolume;
	}

	public void UpdateSFXVolumeTextLabel()
	{
		SFXVolumeText.text = SFXVolumeSlider.value.ToString();
	}

	public void UpdateMusicVolumeTextLabel()
	{
		MusicVolumeText.text = MusicVolumeSlider.value.ToString();
	}

	public void SaveVideoSettings()
	{
		bool isChanged = false;

		if (QualitySettings.GetQualityLevel() != gameQualityDropdown.value)
		{
			QualitySettings.SetQualityLevel(gameQualityDropdown.value);
			isChanged = true;
		}

		Screen.fullScreen = !useFullscreenToggle.isOn;

		if (Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].height != Screen.currentResolution.height || Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].width != Screen.currentResolution.width)
		{
			Screen.SetResolution(Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].width, Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].height, !useFullscreenToggle.isOn);
			isChanged = true;
		}

		if (isChanged == false) return;

		PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
		PlayerPrefs.SetInt("ScreenResolution", screenResolutionDropdown.value);
		PlayerPrefs.SetInt("IsFullScreen", useFullscreenToggle.isOn == true ? 0 : 1);
		PlayerPrefs.Save();

	}

	public void OnFullscreenToggle()
	{
		int savedValue = screenResolutionDropdown.value;

		if (useFullscreenToggle.isOn == true)
		{
			screenResolutionDropdown.ClearOptions();

			for (int i = 0; i < Screen.resolutions.Length; i++)
			{
				if (Screen.resolutions[i].height < 480)
				{
					continue;
				}

				if (Screen.resolutions[i].height == Screen.resolutions[Screen.resolutions.Length - 1].height)
				{
					continue;
				}

				screenResolutionDropdown.options.Add(new Dropdown.OptionData() { text = Screen.resolutions[i].width + " X " + Screen.resolutions[i].height });
			}

			if (screenResolutionDropdown.options.Count >= savedValue)
			{
				screenResolutionDropdown.value = savedValue;
				screenResolutionDropdown.captionText.text = Screen.resolutions[savedValue + skippedResolutions].width + " X " + Screen.resolutions[savedValue + skippedResolutions].height;
			}
			else
			{
				screenResolutionDropdown.value = screenResolutionDropdown.options.Count - 1;
				screenResolutionDropdown.captionText.text = screenResolutionDropdown.options[screenResolutionDropdown.options.Count - 1].text;
			}
		}
		else
		{
			screenResolutionDropdown.ClearOptions();

			for (int i = 0; i < Screen.resolutions.Length; i++)
			{
				if (Screen.resolutions[i].height < 480)
				{
					continue;
				}

				screenResolutionDropdown.options.Add(new Dropdown.OptionData() { text = Screen.resolutions[i].width + " X " + Screen.resolutions[i].height });
			}

			int temp = 0;
			for (int i = 0; i < savedValue; i++)
			{
				if (Screen.resolutions[skippedResolutions + i].height >= Screen.resolutions[Screen.resolutions.Length - 1].height)
				{
					temp++;
				}
			}

			if (screenResolutionDropdown.options[savedValue + temp] != null)
			{
				screenResolutionDropdown.value = savedValue + temp;
				screenResolutionDropdown.captionText.text = Screen.resolutions[savedValue + temp + skippedResolutions].width + " X " + Screen.resolutions[savedValue + temp + skippedResolutions].height;
			}
			else
			{
				screenResolutionDropdown.value = screenResolutionDropdown.options.Count;
				screenResolutionDropdown.captionText.text = Screen.resolutions[screenResolutionDropdown.options.Count + skippedResolutions].width + " X " + Screen.resolutions[screenResolutionDropdown.options.Count + skippedResolutions].height;
			}
		}
	}

	public void ResetPauseMenuManager()
	{
		if (mainMenu.gameObject.activeInHierarchy == true) return;

			mainMenu.gameObject.SetActive(true);
			optionsMenu.gameObject.SetActive(false);
			videoMenu.gameObject.SetActive(false);
			exitMenu.gameObject.SetActive(false);
			audioMenu.gameObject.SetActive(false);
			controlsMenu.gameObject.SetActive(false);
	}
}
