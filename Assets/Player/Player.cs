

using UnityEngine;

using NUNet;
using System.Globalization;

public class Player : MonoBehaviour, IDamagable
{
	public Team team { get; set; }

	public float maxHealth = 100;

	public float currentHealth;

	public float moveSpeed = 10;

	public float rotationSpeed = 5;

	public bool isLocal;

	public GameObject tower;

	public Weapon weapon;

	private Rigidbody rb;
	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		currentHealth = maxHealth;
	}

	private void Update()
	{
		team = isLocal ? Team.PLAYER : Team.ENEMY;
		weapon.team = team;

		if (!NUClient.connected || !isLocal)
			return;

		if (Input.GetMouseButton(0))
		{
			weapon.Attack();
		}


	}
	private void FixedUpdate()
	{
		if (!NUClient.connected || !isLocal)
			return;

		string msg = string.Format("Inp|{0};{1}",
			EncodePositionInput(),
			EncodeTowerRotationInput()
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

	public string EncodeTowerRotationInput()
	{
		Vector3 direction = GetTowerLookDirection();
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
		Quaternion rotation = Quaternion.Slerp(tower.transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

		return NetUtility.EncodeQuaternion(rotation);
	}

	public void DecodeTowerRotation(string msg)
	{
		tower.transform.rotation = NetUtility.DecodeQuaternion(msg);
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
