using UnityEngine;
using System.Collections;

public class GameManager : OverridableMonoBehaviour
{
	[SerializeField]
	private AIManager AIManagerPrefab;
	[SerializeField]
	private NetworkManager networkManagerPrefab;
	[SerializeField]
	private AudioManager audioManagerPrefab;
	[SerializeField]
	private Texture[] allPlayerSkins;

	private static GameManager instance;
	private AIManager aIManager;
	private NetworkManager networkManager;
	private AudioManager audioManager;
	private bool inGame;
	private GameTypes currentGameType;

	public bool InGame
	{
		get { return inGame; }
		set { inGame = value; }
	}
	public Texture[] AllPlayerSkins
	{
		get { return allPlayerSkins; }
	}
	public GameTypes CurrentGameType
	{
		get { return currentGameType; }
		set { currentGameType = value; }
	}

	public static GameManager GetInstance()
	{
		return instance;
	}
	
	protected override void Awake()
	{
		base.Awake();

		GetAudioManager().UpdateAudioVolumes(PlayerPrefs.GetFloat("SFXVolume"), PlayerPrefs.GetFloat("MusicVolume"));
	}

	void Start()
	{
		GameManager[] gameManagers = FindObjectsOfType<GameManager>();

		if(gameManagers.Length > 1)
		{
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);

		if (instance == null)
		{
			instance = this;
		}
	}

	public override void UpdateMe()
	{
		if (PhotonNetwork.offlineMode == false)
		{
			if (inGame == true && PhotonNetwork.isMasterClient == true)
			{
				if (GetAIManager().AllRemainingZombies.Count == 0)
				{
					aIManager.NextWave();
				}
			} 
		}
		else
		{
			if (inGame == true)
			{
				if (GetAIManager().AllRemainingZombies.Count == 0)
				{
					aIManager.NextWave();
				}
			}
		}
	}

	public AIManager GetAIManager()
	{
		if (aIManager == null)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (PhotonNetwork.isMasterClient == true)
				{
					aIManager = PhotonNetwork.InstantiateSceneObject(AIManagerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero), 0, null).GetComponent<AIManager>();
				}
				else
				{
					if(FindObjectOfType<AIManager>() != null)
					{
						aIManager = FindObjectOfType<AIManager>();
					}
				}
			}
			else
			{
				aIManager = Instantiate(AIManagerPrefab);
			}
		}

		return aIManager;
	}

	public NetworkManager GetNetworkManager()
	{
		if (networkManager == null)
		{
			networkManager = Instantiate(networkManagerPrefab);
			DontDestroyOnLoad(networkManager);
		}

		return networkManager;
	}

	public AudioManager GetAudioManager()
	{
		if (audioManager == null)
		{
			if(PhotonNetwork.inRoom == false)
			{
				audioManager = Instantiate(audioManagerPrefab);
			}
			else
			{
				if(PhotonNetwork.offlineMode == false)
				{
					if(PhotonNetwork.isMasterClient == false)
					{
						audioManager = FindObjectOfType<AudioManager>();
					}
					else
					{
						audioManager = PhotonNetwork.InstantiateSceneObject(audioManagerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero), 0, null).GetComponent<AudioManager>();
					}
				}
			}
		}

		return audioManager;
	}

	void OnApplicationQuit()
	{
		PlayerPrefs.SetFloat("SFXVolume", GetAudioManager().SavedSFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", GetAudioManager().SavedMusicVolume);
		PlayerPrefs.Save();
	}
}
