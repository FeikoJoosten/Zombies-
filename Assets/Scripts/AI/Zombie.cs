using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class Zombie : OverridableMonoBehaviour
{
	[SerializeField]
	private UnityEngine.AI.NavMeshAgent agent = null;
	[SerializeField]
	private Rigidbody rig = null;
	[SerializeField]
	private Rigidbody[] ragdollRigidBodys = null;
	[SerializeField]
	private float startingHealth = 100;
	[SerializeField]
	private float damage = 0;
	[SerializeField]
	private float healthMultiplier = 0;
	[SerializeField]
	private Image healthBar = null;
	[SerializeField]
	private Animator ani = null;
	[SerializeField]
	private Color almostDeadHealthColor = Color.red;
	[SerializeField]
	private float almostDeadPercentage = 0;
	[SerializeField]
	private float attackWaitTime = 0;
	[SerializeField]
	private float despawnAfterDeathTime = 5;
	[SerializeField]
	private float minimumWaitTimeBeforeGrunt = 0;
	[SerializeField]
	private float maximumWaitTimeBeforeGrunt = 0;
	[SerializeField]
	private Collider[] triggers = null;
	[SerializeField]
	private AudioSource audioSource = null;
	[SerializeField]
	private AudioClip gruntSound = null;
	[SerializeField]
	private AudioClip attackSound = null;

	private float currentHealth;
	private float currentAttackWaitTime;
	private int walkingHashID;
	private int attackHashID;
	private int lastPlayerIDThatDidDamage;
	private bool finishedSpawning;
	private bool isAttacking;
	private bool isDead;

	public UnityEngine.AI.NavMeshAgent Agent
	{
		get { return agent; }
	}
	public bool IsAttacking
	{
		get { return isAttacking; }
	}
	public bool FinishedSpawning
	{
		get { return finishedSpawning; }
	}
	public bool IsDead
	{
		get { return isDead; }
	}
	public float Damage
	{
		get { return damage; }
	}
	public float StartingHealth
	{
		get { return startingHealth; }
		set { startingHealth = value; }
	}
	public float HealthMultiplier
	{
		get { return healthMultiplier; }
	}

	protected override void Awake()
	{
		base.Awake();

		GameManager.GetInstance()
			.GetAIManager()
			.AllRemainingZombies.Add(PhotonNetwork.offlineMode != false ? gameObject.GetInstanceID() : photonView.viewID, this);
	}

	private void Start()
	{
		for (int i = 0; i < ragdollRigidBodys.Length; i++)
		{
			ragdollRigidBodys[i].isKinematic = true;
		}

		if (PhotonNetwork.offlineMode == true)
		{
			GameManager.GetInstance().GetAudioManager().AddSFXAudioSource( audioSource);
			StartCoroutine(GruntTimer());
		}
		else
		{
			GameManager.GetInstance().GetAudioManager().AddSFXAudioSource(audioSource);

			if (PhotonNetwork.isMasterClient == true)
			{
				StartCoroutine(GruntTimer());
			}
		}

		currentHealth = startingHealth;
		walkingHashID = Animator.StringToHash("Walking");
		attackHashID = Animator.StringToHash("ShouldAttack");
		StartCoroutine(SpawnTimer());
		healthBar.transform.parent.gameObject.SetActive(false);
	}

	public override void UpdateMe()
	{
		if (finishedSpawning == true)
		{
			if (isDead == true)
			{
				despawnAfterDeathTime -= Time.deltaTime;

				if (despawnAfterDeathTime <= 0)
				{
					GameManager.GetInstance().GetAIManager().RemoveZombie(this);
				}
				return;
			}
		}

		if (Vector3.Distance(transform.position, agent.destination) < agent.stoppingDistance)
		{
			if (finishedSpawning == false)
			{
				return;
			}

			ani.SetBool(walkingHashID, false);

			if (PhotonNetwork.isMasterClient == true)
			{
				Vector3 targetRotation = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
				transform.LookAt(targetRotation);
			}

			if (currentAttackWaitTime <= 0)
			{
				StartCoroutine(Attack());
			}
			else
			{
				currentAttackWaitTime -= Time.deltaTime;
				ani.SetBool(attackHashID, false);
			}
		}
		else
		{
			ani.SetBool(walkingHashID, true);
			ani.SetBool(attackHashID, false);
			currentAttackWaitTime = 0.25F;
		}
	}

	[PunRPC]
	private void RemoveHealth(float healthToRemove)
	{
		if (currentHealth - healthToRemove > 0)
		{
			currentHealth -= healthToRemove;

			if (healthBar.transform.parent.gameObject.activeInHierarchy == false)
			{
				healthBar.transform.parent.gameObject.SetActive(true);
			}

			healthBar.fillAmount = currentHealth / startingHealth;

			if (currentHealth / startingHealth < almostDeadPercentage)
			{
				healthBar.color = almostDeadHealthColor;
			}
		}
		else
		{
			Die();
		}
	}

	private void Die()
	{
		ani.SetBool(attackHashID, false);
		ani.SetBool(walkingHashID, false);
		ani.enabled = false;
		healthBar.transform.parent.gameObject.SetActive(false);
		agent.enabled = false;
		rig.isKinematic = true;
		StopCoroutine(GruntTimer());

		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].enabled = false;
		}
		for (int i = 0; i < ragdollRigidBodys.Length; i++)
		{
			ragdollRigidBodys[i].isKinematic = false;
		}

		if (isDead == false)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (PhotonNetwork.isMasterClient == true)
				{
					GameManager.GetInstance().GetNetworkManager().MasterClient.AddKillCount(lastPlayerIDThatDidDamage);
				}
			}
			else
			{
				GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.First().Value.AddKillCount(lastPlayerIDThatDidDamage);
			}
		}

		isDead = true;

		switch(PhotonNetwork.offlineMode)
		{
			case true:
				GameManager.GetInstance().GetAIManager().AllRemainingZombies.Remove(gameObject.GetInstanceID());
				break;
			case false:
				GameManager.GetInstance().GetAIManager().AllRemainingZombies.Remove(photonView.viewID);
				break;
		}
	}

	private IEnumerator SpawnTimer()
	{
		yield return new WaitForSeconds(2.3333333F);
		agent.enabled = true;
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].enabled = true;
		}
		finishedSpawning = true;
	}

	private IEnumerator Attack()
	{
		ani.SetBool(attackHashID, true);

		GameManager.GetInstance().GetAudioManager().PlaySFXSound(audioSource, attackSound);

		if (isAttacking == false)
		{
			isAttacking = true;
		}

		yield return new WaitForSeconds(0.25F);
		currentAttackWaitTime = attackWaitTime;
	}

	private IEnumerator GruntTimer()
	{
		while (finishedSpawning == false)
		{
			yield return null;
		}

		while (isDead == true)
		{
			StopCoroutine(GruntTimer());
			yield break;
		}

		while (isAttacking == true)
		{
			yield return null;
		}

		float waitTime = Random.Range(minimumWaitTimeBeforeGrunt, maximumWaitTimeBeforeGrunt);

		yield return new WaitForSeconds(waitTime);

		switch (isDead)
		{
			case false:
				if (PhotonNetwork.offlineMode == false)
				{
					GameManager.GetInstance().GetAIManager().photonView.RPC("CreatePositionPointOnMinimap", PhotonTargets.All, transform.position);
					photonView.RPC("Grunt", PhotonTargets.All);
				}
				else
				{
					Grunt();
				}

				StartCoroutine(GruntTimer());
				break;
		}
	}

	[PunRPC]
	private void Grunt()
	{
		GameManager.GetInstance().GetAIManager().CreatePositionPointOnMinimap(transform.position);
		GameManager.GetInstance().GetAudioManager().PlaySFXSound(audioSource, gruntSound);
	}

	public void StopAttacking()
	{
		StopCoroutine(Attack());
		isAttacking = false;
		ani.SetBool(attackHashID, false);
		currentAttackWaitTime = attackWaitTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		Bullet bullet = other.GetComponent<Bullet>();
		Grenade grenade = other.GetComponent<Grenade>();

		if (bullet != null)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				photonView.RPC("RemoveHealth", PhotonTargets.Others, bullet.Damage);
			}
			lastPlayerIDThatDidDamage = bullet.OwnerID;
			RemoveHealth(bullet.Damage);
		}

		if (grenade != null)
		{
			if (grenade.IsExploded == true)
			{
				if (PhotonNetwork.offlineMode == false)
				{
					photonView.RPC("RemoveHealth", PhotonTargets.Others, grenade.Damage);
				}
				lastPlayerIDThatDidDamage = grenade.OwnerID;
				RemoveHealth(grenade.Damage);
			}
		}
	}
}
