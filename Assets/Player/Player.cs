

using UnityEngine;
using UnityEngine.UI;

using NUNet;
using System;
using Unity.Collections;

public class Player : MonoBehaviour, IDamagable
{
	public PlayerNetData data;

	public Image healthBar;
	public Team team { get => data.team; set => data.team = value; }

	public float maxHealth = 100;

	public float currentHealth;

	public float moveSpeed = 10;

	public float rotationSpeed = 5;

	public float towerRotationSpeed = 7;

	public MeshRenderer[] renderers;

	public GameObject tower;

	public Weapon weapon;

	private Vector3 targetPosition;

	private Quaternion rotationToTarget;

	private Vector3 input;

	private Rigidbody rb;


	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		currentHealth = maxHealth;
		UpdateHealthBar();
	}

	private void FixedUpdate()
	{
		weapon.team = team;

		if (LobbyManager.isHost)
		{
			rb.velocity = input;

			if (input.magnitude > 0.1f)
			{
				LookAt(gameObject, input, rotationSpeed);
			}
		}



		if (!NUClient.connected || !data.isLocal)
			return;

		if (Input.GetMouseButton(0))
		{
			weapon.Attack();
		}

		input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized * moveSpeed;

		targetPosition = GetTowerTargetPosition();

		if (!LobbyManager.isHost)
		{
			string msg = string.Format("{0}|{1};{2}",
				(int)Message.PLAYER_INPUT,
				EncodePositionInput(),
				EncodeTargetPosition()
			);

			NUClient.SendUnreliable(new Packet(msg));
		}
	}

	private void LateUpdate()
	{
		Vector3 lookDirection = (targetPosition - tower.transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
		rotationToTarget = Quaternion.Lerp(rotationToTarget, lookRotation, towerRotationSpeed * Time.deltaTime);
		//"Ignora" a rotação do objeto pai e rotaciona em direção ao alvo.
		tower.transform.localRotation = rotationToTarget * Quaternion.Inverse(gameObject.transform.rotation);
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

	//public void DecodeVelocity(string msg)
	//{
	//	rb.velocity = NetUtility.DecodeVector(msg);
	//}

	public string EncodePositionInput()
	{
		return NetUtility.EncodeVector(input);
	}

	public void DecodePositionInput(string msg)
	{
		input = NetUtility.DecodeVector(msg);
	}

	public string EncodePosition()
	{
		return NetUtility.EncodeVector(transform.position);
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

	protected void LookAt(GameObject obj, Vector3 direction, float rotationSpeed)
	{
		direction.Normalize();
		//Debug.Log(direction);
		//Vector3 forward = transform.forward;
		//Vector3 right =   transform.right;



		//float angle = Vector3.SignedAngle(right, direction, Vector3.up); //Vector3.Dot(rightDir, direction);
		//float dotF = Vector3.Dot(forward, direction);
		//float dotR = Vector3.Dot(right, direction);
		//Debug.Log("dot: " + dotF + " dot2: " + dotR);

		//if (dotR > 0 && dotF > 0 || dotR < 0 && dotF < 0) 
		//if (direction.x > 0 && direction.z > 0 || direction.x < 0 && direction.z < 0) 
		//if(dotR > 0)
		{
			Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
			//Quaternion inverse = Quaternion.LookRotation(obj.transform.forward);
			//Debug.Log(obj.transform.rotation + "      " + inverse);
			Quaternion rotation = Quaternion.Slerp(obj.transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
			obj.transform.rotation = rotation;
		}
		//else if (dotR < 0 && dotF > 0 || dotR > 0 && dotF < 0)
		//else if(direction.x < 0 && direction.z > 0 || direction.x > 0 && direction.z < 0)
		//else if (dotR < 0)
		//{
		//	Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
		//	Quaternion inverse = Quaternion.LookRotation(-obj.transform.forward);
		//	Quaternion rotation = Quaternion.Slerp(inverse, lookRotation, 2 * Time.deltaTime);
		//	obj.transform.rotation = rotation;
		//}
	}
}
