using UnityEngine;
using System.Collections;

public class WalkForwardAndBackward : MonoBehaviour {

    [SerializeField]
    private float distance;

    private Vector3 finalPos;
    private Vector3 startingPos; 
    private UnityEngine.AI.NavMeshAgent agent;
     

	// Use this for initialization
	void Awake () {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        startingPos = transform.position;
        finalPos = transform.position + transform.forward * distance;
        agent.SetDestination(finalPos);
	}
	
	// Update is called once per frame
	void Update () {

        if ((transform.position - startingPos).magnitude < 2)  
        {
            agent.SetDestination(finalPos);
        }
        if ((transform.position - finalPos).magnitude < 2)
        {
            agent.SetDestination(startingPos);
        }
	}
}
