using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Weapon : MonoBehaviour
{
	public Team team;

	[BoxGroup("References")]
	public GameObject muzzle;

	public Projectile projectilePrefab;

	protected AudioSource audioSource;

	public float attackInterval;

	public float projectileSpeed;

	public int damage;

	private float nextAttackInterval;

	protected virtual void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}
	protected virtual void Update()
	{
		nextAttackInterval += Time.deltaTime;
	}
	public virtual void Attack()
	{

		if (nextAttackInterval >= attackInterval)
			Shoot();
	}

	public virtual void Shoot()
	{
		Projectile projectile = Instantiate(projectilePrefab, muzzle.transform.position, transform.rotation);

		projectile.team = team;
		projectile.moveSpeed = projectileSpeed;
		projectile.damage = damage;

		projectile.rb.velocity = transform.forward * projectileSpeed;

		//AudioManager.PlaySFX(audioSource, Sound.PLASMA);

		nextAttackInterval = 0;
	}
}
