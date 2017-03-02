using UnityEngine;

public class HealthpackPickup : OverridableMonoBehaviour
{
	[SerializeField]
	private int amountToHeal = 0;
	[SerializeField]
	private float rotationSpeed = 0;
	[SerializeField]
	private float trailMovementSpeed = 0;
	[SerializeField]
	private TrailRenderer trail = null;
	[SerializeField]
	private Vector3 trailEndPosition = Vector3.zero;

	private Vector3 trailStartPosition;
	private float currentMovementSpeed;
	private float currentMovementPercentage;

	private void Start()
	{
		trailStartPosition = trail.transform.localPosition;
	}

	public override void UpdateMe()
	{
		transform.Rotate(transform.up * rotationSpeed * Time.deltaTime);
		trail.transform.localPosition = Vector3.Lerp(trailStartPosition, trailStartPosition + trailEndPosition, currentMovementPercentage);

		if (!(currentMovementSpeed < trailMovementSpeed)) return;

		currentMovementSpeed += Time.deltaTime;
		currentMovementPercentage = currentMovementSpeed / trailMovementSpeed;
	}

	private void OnTriggerEnter(Collider other)
	{
		Player player = other.GetComponent<Player>();
		if (player == null) return;

		if (player.CurrentHealth == player.StartingHealth) return;

		player.AddHealth(amountToHeal);

		UpdateManager.RemoveSpecificItemAndDestroyIt(this);
	}
}
