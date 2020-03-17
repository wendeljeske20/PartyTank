using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using NUNet;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
	[BoxGroup("Properties")]
	public Team team;

	[BoxGroup("Properties")]
	public float moveSpeed = 5;

	[BoxGroup("Properties")]
	public int damage = 10;

	public float duration = 10;

	[HideInInspector]
	public Rigidbody rb;

	//[BoxGroup("Properties")]
	//public TrajectoryType trajectoryType;

	public Action OnHit;

	public bool canDestroy;

	Vector3 spawnPosition;

	public IDamagable hittedDamagable;

	public int id;

	private Vector3 lastVelocity;

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
		if (!NUClient.connected || !LobbyManager.isHost)
			return;

		duration -= Time.deltaTime;
		if (duration <= 0)
		{
			SendDestroy();
		}

		//if (timer > 0.05f)
		canDestroy = true;
	}

	private void FixedUpdate()
	{
		if (!NUClient.connected || !LobbyManager.isHost)
			return;

		lastVelocity = rb.velocity;
	}

	public void ToDestroy()
	{
		Destroy(gameObject);
		OnHit?.Invoke();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!LobbyManager.isHost || !canDestroy)
			return;

		IDamagable damagable = other.gameObject.GetComponent<IDamagable>();

		if (damagable == null || hittedDamagable == damagable)// || team == damagable.team)
			return;

		damagable.SendTakeDamage(damage);
		hittedDamagable = damagable;
		SendDestroy();
	}

	private void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			rb.velocity = Vector3.Reflect(lastVelocity, contact.normal);
		}
	}
	public void SendDestroy()
	{
		NetworkAppManager.projectiles.Remove(id);
		ToDestroy();

		string sendMsg = (int)Message.DESTROY_PROJECTILE + "|" + id;

		NUServer.SendReliable(new Packet(sendMsg, NUServer.GetConnectedClients()));
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


