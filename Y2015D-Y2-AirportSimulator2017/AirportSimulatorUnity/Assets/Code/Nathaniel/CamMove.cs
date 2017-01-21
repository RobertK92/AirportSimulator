using UnityEngine;
using System.Collections;

public class CamMove : MonoBehaviour
{
	public float moveSpeed = 100.0f;
	public float mouseSpeed = 10.0f;

	private Vector3 oldMousePos;

	void Start()
	{
		oldMousePos = Input.mousePosition;
	}

	void Update()
	{
		Vector3 r = transform.right;
		Vector3 f = transform.forward;
		Vector3 u = transform.up;

		transform.position += f * moveSpeed * Time.unscaledDeltaTime *
								  ((Input.GetKey(KeyCode.W) ? 1.0f : 0.0f) +
								  (Input.GetKey(KeyCode.S) ? -1.0f : 0.0f));
		transform.position += r * moveSpeed * Time.unscaledDeltaTime *
								  ((Input.GetKey(KeyCode.D) ? 1.0f : 0.0f) +
								  (Input.GetKey(KeyCode.A) ? -1.0f : 0.0f));
		transform.position += u * moveSpeed * Time.unscaledDeltaTime *
								  ((Input.GetKey(KeyCode.Q) ? 1.0f : 0.0f) +
								  (Input.GetKey(KeyCode.Z) ? -1.0f : 0.0f));

		if(Input.GetMouseButton(0))
		{
			Vector3 deltaMouse = oldMousePos - Input.mousePosition;

			if(deltaMouse.sqrMagnitude < 50.0f * mouseSpeed * mouseSpeed)
			{
				float dx = -deltaMouse.x;
				float dy = deltaMouse.y;
				transform.Rotate(Vector3.up, dx * mouseSpeed * Time.unscaledDeltaTime, Space.World);
				transform.Rotate(r, dy * mouseSpeed * Time.unscaledDeltaTime, Space.World);

				Quaternion rot = transform.rotation;
				Vector3 e = rot.eulerAngles;
				e.z = 0.0f;
				transform.rotation = Quaternion.Euler(e);
			}
		}
		oldMousePos = Input.mousePosition;
	}
}
