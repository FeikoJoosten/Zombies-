using UnityEngine;

public class HealthpackPickup : OverridableMonoBehaviour
{
	[SerializeField]
	private int amountToHeal;
	[SerializeField]
	private float rotationSpeed;
	[SerializeField]
	private float trailMovementSpeed;
	[SerializeField]
	private TrailRenderer trail;
	[SerializeField]
	private Vector3 trailEndPosition;

	private Vector3 trailStartPosition;
	private float currentMovementSpeed;
	private float currentMovementPercentage;

	void Start()
	{
		trailStartPosition = trail.transform.localPosition;
	}

	public override void UpdateMe()
	{
		transform.Rotate(transform.up * rotationSpeed * Time.deltaTime);
		trail.transform.localPosition = Vector3.Lerp(trailStartPosition, trailStartPosition + trailEndPosition, currentMovementPercentage);

		if (currentMovementSpeed < trailMovementSpeed)
		{
			currentMovementSpeed += Time.deltaTime;
			currentMovementPercentage = currentMovementSpeed / trailMovementSpeed;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Player player = other.GetComponent<Player>();
		if (player != null)
		{
			if (player.CurrentHealth == player.StartingHealth)
			{
				return;
			}
			else
			{
				player.AddHealth(amountToHeal);

				UpdateManager.RemoveSpecificItemAndDestroyIt(this);
			}
		}
	}
}
