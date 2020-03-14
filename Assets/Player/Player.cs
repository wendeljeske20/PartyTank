

using UnityEngine;
using UnityEngine.UI;

using NUNet;
using System.Globalization;
using System;

public class Player : MonoBehaviour, IDamagable
{
	public PlayerNetData data;

	public Image healthBar;
	public Team team { get; set; }

	public float maxHealth = 100;

	public float currentHealth;

	public float moveSpeed = 10;

	public float rotationSpeed = 5;

	public GameObject tower;

	public Weapon weapon;

	private Vector3 targetPosition;

	private Rigidbody rb;
	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		currentHealth = maxHealth;
		UpdateHealthBar();
	}

	private void Update()
	{
		weapon.team = team;



		if (!NUClient.connected || !data.isLocal)
			return;

		if (Input.GetMouseButton(0))
		{
			weapon.Attack();
		}


	}
	private void FixedUpdate()
	{
		Vector3 lookDirection = (targetPosition - tower.transform.position).normalized;
		LookAt(tower, lookDirection);

		if (!NUClient.connected || !data.isLocal)
			return;

		targetPosition = GetTowerTargetPosition();

		string msg = string.Format("{0}|{1};{2}",
			(int)Message.PLAYER_INPUT,
			EncodePositionInput(),
			EncodeTargetPosition()
		);

		NUClient.SendUnreliable(new Packet(msg));


	}

	public void TakeDamage(int damage)
	{
		currentHealth -= damage;

		if (currentHealth < 0)
		{
			//SendDestroy();
			//gameObject.SetActive(false);
		}

		UpdateHealthBar();
	}

	public void SendTakeDamage(int damage)
	{
		TakeDamage(damage);

		string sendMsg = (int)Message.PLAYER_TAKE_DAMAGE + "|" + data.guid.ToString() + ";" + damage;

		NUServer.SendReliable(new Packet(sendMsg, NUServer.GetConnectedClients()));

	}

	private void UpdateHealthBar()
	{
		healthBar.fillAmount = currentHealth / maxHealth;
	}

	public void DecodeVelocity(string msg)
	{
		rb.velocity = NetUtility.DecodeVector(msg);
	}

	public string EncodePosition()
	{
		return NetUtility.EncodeVector(transform.position);
	}

	private string EncodePositionInput()
	{
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * moveSpeed;
		return NetUtility.EncodeVector(input);
	}

	public void DecodePosition(string msg)
	{
		transform.position = NetUtility.DecodeVector(msg);
	}

	public string EncodeRotation()
	{
		return NetUtility.EncodeQuaternion(transform.rotation);
	}

	public void DecodeRotation(string msg)
	{
		transform.rotation = NetUtility.DecodeQuaternion(msg);
	}

	public string EncodeTowerRotation()
	{
		return NetUtility.EncodeQuaternion(tower.transform.rotation);
	}

	public string EncodeTargetPosition()
	{
		return NetUtility.EncodeVector(targetPosition);
	}

	public void DecodeTargetPosition(string msg)
	{
		targetPosition = NetUtility.DecodeVector(msg);
	}

	private Vector3 GetTowerTargetPosition()
	{
		Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, new Vector3(0, tower.transform.position.y, 0));
		float rayLenght;
		Vector3 pointToLook = Vector3.one;
		if (groundPlane.Raycast(cameraRay, out rayLenght))
		{
			pointToLook = new Vector3(cameraRay.GetPoint(rayLenght).x, cameraRay.GetPoint(rayLenght).y, cameraRay.GetPoint(rayLenght).z - 0.5f);
		}
		return pointToLook;
	}

	private Vector3 GetTowerLookDirection()
	{
		Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, new Vector3(0, tower.transform.position.y, 0));
		float rayLenght;
		Vector3 direction = Vector3.right;
		if (groundPlane.Raycast(cameraRay, out rayLenght))
		{
			Vector3 pointToLook = new Vector3(cameraRay.GetPoint(rayLenght).x, cameraRay.GetPoint(rayLenght).y, cameraRay.GetPoint(rayLenght).z - 0.5f);

			direction = (pointToLook - tower.transform.position).normalized;
		}
		return direction;
	}

	protected void LookAt(GameObject obj, Vector3 direction)
	{
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
		obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
	}
}
