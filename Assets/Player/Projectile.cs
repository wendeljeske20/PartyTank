using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using NUNet;

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

	public int id;

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

	private void OnCollisionEnter(Collision collision)
	{
		if (!LobbyManager.isHost || !canDestroy)
			return;

		IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();

		if (damagable == null || hittedDamagable == damagable)// || team == damagable.team)
			return;

		damagable.TakeDamage(damage);
		hittedDamagable = damagable;
		//ToDestroy();
		SendDestroy();
	}

	public void SendDestroy()
	{
		string msg = "DestroyProjectile|" + id;
		NUClient.SendReliable(new Packet(msg));
	}

	public string EncodePosition()
	{
		return NetUtility.EncodeVector(transform.position);
	}

	public void DecodePosition(string msg)
	{
		transform.position = NetUtility.DecodeVector(msg);
	}
}


