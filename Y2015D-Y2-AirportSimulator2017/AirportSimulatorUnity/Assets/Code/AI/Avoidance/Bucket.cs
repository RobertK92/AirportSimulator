using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;

public class Bucket
{
    public List<Avoidance> localAgents = new List<Avoidance>();
    public List<Avoidance> movedAgents = new List<Avoidance>();

    public Vector3 position;
    public float size;
    public bool empty;
    private Grid lattice;
    private int posInGridX;
    private int posInGridY;

    public Bucket(Vector3 pos, float scale, Grid parent, int xIndex, int yIndex)
    {
        position = pos;
        size = scale;
        posInGridX = xIndex;
        posInGridY = yIndex;
        lattice = parent;
    }
    
	public void  Update ()
	{
	    empty = false;

	    for (int i = 0; i < localAgents.Count; i++)
	    {
            if (localAgents[i] == null)
                continue;
            lattice.GetNearestN(localAgents[i], 5, localAgents[i].nearestAgents);
            localAgents[i].UpdateNeighbours();
            localAgents[i].UpdateFutureTransforms(2.0f);
        }
        for (int i = 0; i < localAgents.Count; i++)
        {
            if (localAgents[i] == null)
                continue;
            //update the neighbours of all the agents in the gridecell
            if (localAgents[i].queueMember.State != QueueMember.QueueState.InQueue)
            {
                localAgents[i].PredictCollisionsCheap();
                localAgents[i].ResolveCollisions();     
            }
            
            if (AgentIsOutsideBucket(localAgents[i]))
            {
                //move object to neighbour bucket
                movedAgents.Add(localAgents[i]);
            }
        }
        foreach (Avoidance obj in movedAgents)
        {
            if (obj == null)
                continue;
            lattice.AddObject(obj);
        }
        for (int i = 0; i < movedAgents.Count; i++)
        {
            if (localAgents[i] == null)
                continue;
            localAgents.Remove(movedAgents[i]);
        }
        movedAgents.Clear();

        if (localAgents.Count == 0)
        {
            empty = true;
        }
    }

    private bool AgentIsOutsideBucket(Avoidance agent)
    {
        if (agent.transform.position.x < position.x ||
            agent.transform.position.x > position.x + size ||
            agent.transform.position.z < position.z ||
            agent.transform.position.z > position.z + size)
        {
            return true;
        }

        return false;  
    }

    public void DrawLines()
    {
        Color color = Color.green;
        if (localAgents.Count > 0)
        {
            color = Color.yellow;
        }
        if (localAgents.Count > 5)
        {
            color = Color.red;
        }
        Debug.DrawLine(position, new Vector3(position.x + size, position.y, position.z),color);
        Debug.DrawLine(position, new Vector3(position.x, position.y, position.z + size), color);
        Debug.DrawLine(new Vector3(position.x + size, position.y, position.z), new Vector3(position.x + size, position.y, position.z + size), color);
        Debug.DrawLine(new Vector3(position.x, position.y, position.z + size), new Vector3(position.x + size, position.y, position.z + size), color);
    }
}
