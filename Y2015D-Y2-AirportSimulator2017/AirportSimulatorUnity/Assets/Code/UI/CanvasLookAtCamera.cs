using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasLookAtCamera : MonoBehaviour
{
    private Camera _camera;
    
	private void Awake() {
        _camera = FindObjectOfType<Camera>();
	}
	
	private void Update ()
    {
        Vector3 v = _camera.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(_camera.transform.position - v);
        transform.rotation = (_camera.transform.rotation);
    }
}
