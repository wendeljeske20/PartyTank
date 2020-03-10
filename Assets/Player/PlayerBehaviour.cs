

using UnityEngine;

using NUNet;

public class PlayerBehaviour : MonoBehaviour
{

	public float moveSpeed = 10;

	public float rotationSpeed = 5;

	public bool isLocal;

	public GameObject tower;

	[HideInInspector]
	public Rigidbody rb;
	private void Awake()
	{
		tower = transform.Find("Base/Tower").gameObject;
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (!NUClient.connected || !isLocal)
			return;

		if (Input.GetKeyDown(KeyCode.Space))
		{
			// NUClient.SendReliable(new Packet("Jmp"));
		}

		//Stream Player Input
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * moveSpeed;
		//input *= Time.deltaTime;
		//GetComponent<Rigidbody>().velocity = input;

		string inpMsg = "Inp|";
		inpMsg += input.x.ToString("R") + ":" + input.y.ToString("R") + ":" + input.z.ToString("R");
		Packet inpPacket = new Packet(inpMsg);
		NUClient.SendUnreliable(inpPacket);

		//LookAt(tower, GetTowerLookDirection());
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
