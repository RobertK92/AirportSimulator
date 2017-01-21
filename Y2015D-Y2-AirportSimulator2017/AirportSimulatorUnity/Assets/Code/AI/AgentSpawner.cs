using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentSpawner : MonoBehaviour 
{
    public GameObject agentPrefab;
    public float spawnInterval = 3.0f;
    public uint maxAgents = 5;

    public bool IsSpawning { get; set; }

    private void Start()
    {
        IsSpawning = true;
        StartCoroutine("SpawnRoutine");
    }
    
    private IEnumerator SpawnRoutine()
    {
        float timer = 0.0f;
        while(IsSpawning)
        {
            yield return null;
            if (timer >= spawnInterval)
            {
                GameObject agent = (GameObject)Instantiate(agentPrefab, transform.position, transform.rotation);
                FindObjectOfType<Grid>().AddObject(agent.GetComponent<Avoidance>());
                timer = 0.0f;
                if (FindObjectsOfType<AI.GOAPv3.GoapAgent>().Length >= maxAgents-1)
                    IsSpawning = false;
            }
            timer += Time.deltaTime;
        }
    }	

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        Gizmos.DrawSphere(transform.position, 2.0f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 6.0f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }
}
