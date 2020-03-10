

using UnityEngine;

using NUNet;

public class PlayerBehaviour : MonoBehaviour
{

	public float moveSpeed = 10;

	public float rotationSpeed = 5;

	public bool isLocal;

	public GameObject tower;

	bool onFocus;

	private Rigidbody rb;
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
		//if (onFocus)
		{
			string msg = "Inp|" + EncodeInput() + ";" + EncodeInputTowerRotation();
			NUClient.SendUnreliable(new Packet(msg));
		}


		//LookAt(tower, GetTowerLookDirection());
	}

	private void OnApplicationFocus(bool focus)
	{
		onFocus = focus;
	}

	public void DecodeVelocity(string msg)
	{
		rb.velocity = NetUtility.DecodeVector(msg);
	}

	private string EncodeInput()
	{
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * moveSpeed;
		return NetUtility.EncodeVector(input);
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

	public string EncodeTowerRotation()
	{
		return NetUtility.EncodeQuaternion(tower.transform.rotation);
	}

	public string EncodeInputTowerRotation()
	{
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(GetTowerLookDirection().x, 0, GetTowerLookDirection().z));

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
