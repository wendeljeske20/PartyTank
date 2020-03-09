

using UnityEngine;

using NUNet;

public class PlayerBehaviour : MonoBehaviour
{

	public float moveSpeed = 30;

	public float rotationSpeed = 5;

	public Transform top;
	private void Start()
	{
		top = transform.Find("Base/Top");
	}

	private void Update()
	{
		if (!NUClient.connected)
			return;

		if (Input.GetKeyDown(KeyCode.Space))
		{
			NUClient.SendReliable(new Packet("Jmp"));
		}

		//Stream Player Input
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		//input *= (Time.deltaTime * moveSpeed);

		string inpMsg = "Inp|";
		inpMsg += input.x.ToString("R") + ":" + input.y.ToString("R") + ":" + input.z.ToString("R");
		Packet inpPacket = new Packet(inpMsg);
		NUClient.SendUnreliable(inpPacket);

		LookAtMouse();
	}

	private void LookAtMouse()
	{
		Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, new Vector3(0, top.transform.position.y, 0));
		float rayLenght;
		if (groundPlane.Raycast(cameraRay, out rayLenght))
		{
			Vector3 pointToLook = new Vector3(cameraRay.GetPoint(rayLenght).x, cameraRay.GetPoint(rayLenght).y, cameraRay.GetPoint(rayLenght).z - 0.5f);

			Vector3 direction = (pointToLook - top.transform.position).normalized;

			LookAt(direction);

		}
	}

	protected void LookAt(Vector3 direction)
	{
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
		top.transform.rotation = Quaternion.Slerp(top.transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
	}
}
