using UnityEngine;

public class Spectator : OverridableMonoBehaviour
{
	[SerializeField]
	private float movementSpeed = 0;
	[SerializeField]
	private float sprintSpeed = 0;
	[SerializeField]
	private float rotationSpeed = 0;
	[SerializeField]
	private RectTransform endScreen = null;

	private Camera playerCamera;
	private Rigidbody rig;
	private HighscoreList highScoreList;

	public float MovementSpeed
	{
		get { return movementSpeed; }
	}
	public float SprintSpeed
	{
		get { return sprintSpeed; }
	}
	public float RotationSpeed
	{
		get { return rotationSpeed; }
	}
	
	public Camera PlayerCamera
	{
		get { return playerCamera; }
		set { playerCamera = value;	}
	}
	public Rigidbody Rig
	{
		get { return rig; }
	}
	public HighscoreList HighScoreList
	{
		get { return highScoreList; }
		set { highScoreList = value; }
	}

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
	}

	public override void UpdateMe()
	{
		if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count == 0)
		{
			OpenEndscreen();
		}
	}

	public void ToggleHighscoreMenu()
	{
		highScoreList.UIParent.SetActive(!highScoreList.UIParent.activeInHierarchy);
	}

	public void OpenEndscreen()
	{
		Cursor.lockState = CursorLockMode.None;

		RectTransform endScrn = Instantiate(endScreen);
		endScrn.transform.SetParent(playerCamera.transform);
		endScrn.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => LeaveGame());

		highScoreList.UIParent.SetActive(true);
		endScreen.gameObject.SetActive(true);
	}

	public void PauseGame()
	{
		if (Time.timeScale > 0)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public void LeaveGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}
}
