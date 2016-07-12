using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Healthbar : MonoBehaviour
{
	[SerializeField]
	private Image healthBar;
	[SerializeField]
	private Color almostDeadHealthColor;
	[SerializeField]
	private Color startingColor;
	[SerializeField]
	private float almostDeadPercentage;

	public void UpdateHealthBar(float currentHealth)
	{
		healthBar.fillAmount = 1 * currentHealth * 0.01F;

		if (healthBar.fillAmount <= almostDeadPercentage)
		{
			healthBar.color = almostDeadHealthColor;
		}
		else
		{
			healthBar.color = startingColor;
		}
	}
}
