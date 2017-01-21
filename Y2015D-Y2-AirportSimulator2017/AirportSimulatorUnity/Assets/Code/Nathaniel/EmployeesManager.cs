using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class EmployeesManager
{
	public int numOfEmployees = 40;
	public GameObject employeePrefab;
	public GameObject staffRoom;
	public float staffRoomRadius = 10.0f;

	public bool hasAvailable { get { return employees.Count > 0; } }

	private List<Employee> employees;

	public void Init()
	{
		Rand rand = new Rand();
		Vector3 pos = staffRoom.transform.position;
		Vector3 posOffset = Vector3.zero;
		employees = new List<Employee>();
		GameObject empGroup = new GameObject();
		empGroup.name = "Employees";

		for(int i = 0; i < numOfEmployees; ++i)
		{
			float angle = 2.0f * Mathf.PI * rand.NextUniform();
			float dist = Mathf.Clamp(1.5f * staffRoomRadius * rand.NextNormalNormal(), 0.0f, staffRoomRadius);
			posOffset.x = dist * Mathf.Cos(angle);
			posOffset.z = dist * Mathf.Sin(angle);

			GameObject go = GameObject.Instantiate(employeePrefab, pos + posOffset, Quaternion.AngleAxis(180.0f / Mathf.PI * angle, Vector3.up)) as GameObject;
			employees.Add(go.GetComponent<Employee>());
			go.GetComponent<Employee>().Init(pos + posOffset, this);
			go.transform.parent = empGroup.transform;
		}
	}

	public bool SendEmployee(LocationImpl loc, int flightNum)
	{
		if(!loc.needsStaff)
		{
			loc.Open(flightNum);
			return true;
		}

		if(employees.Count > 0)
		{
			employees[0].GoToJob(loc, flightNum);
			employees.RemoveAt(0);
			return true;
		}

		return false;
	}

	public void Return(Employee emp)
	{
		employees.Add(emp);
	}
}