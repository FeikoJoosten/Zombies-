using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
	[SerializeField]
	private Image healthBar = null;
	[SerializeField]
	private Color almostDeadHealthColor = Color.red;
	[SerializeField]
	private Color startingColor = Color.green;
	[SerializeField]
	private float almostDeadPercentage = 0;

	public void UpdateHealthBar(float currentHealth)
	{
		healthBar.fillAmount = 1 * currentHealth * 0.01F;

		healthBar.color = healthBar.fillAmount <= almostDeadPercentage ? almostDeadHealthColor : startingColor;
	}
}
