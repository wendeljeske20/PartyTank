using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class Projectile : MonoBehaviour
{
	[BoxGroup("Properties")]
	public Team team;

	[BoxGroup("Properties")]
	public float moveSpeed;

	[BoxGroup("Properties")]
	public int damage;

	[HideInInspector]
	public Rigidbody rb;

	//[BoxGroup("Properties")]
	//public TrajectoryType trajectoryType;

	public Action OnHit;

	public bool canDestroy;

	Vector3 spawnPosition;

	public IDamagable hittedDamagable;

	private float timer;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	protected virtual void Start()
	{
		spawnPosition = transform.position;
	}

	protected virtual void Update()
	{
		//float distance = (transform.position - spawnPosition).sqrMagnitude;

		//if (distance >= attackRange * attackRange)
		//{
		//	ToDestroy();
		//}
		timer += Time.deltaTime;

		//if (timer > 0.05f)
		canDestroy = true;
	}
	public void ToDestroy()
	{
		Destroy(gameObject);
		OnHit?.Invoke();

	}

	protected void OnTriggerEnter(Collider other)
	{
		if (!canDestroy)
			return;

		IDamagable damagable = other.GetComponent<IDamagable>();

		if (damagable == null || hittedDamagable == damagable || team == damagable.team)
			return;

		damagable.TakeDamage(damage);
		hittedDamagable = damagable;
		ToDestroy();
	}
}


