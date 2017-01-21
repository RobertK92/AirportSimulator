using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

    [SerializeField]
    private int divisions;
    [SerializeField]
    private Bucket[,] lattice;
    private float sizeOfLevel;
    private bool currentRowEmpty;
    public Vector3 gridOrigin;

    private List<Avoidance> allNeighbours;
    private Avoidance currentNearest;

    struct int2
    {
        public int x;
        public int y;
    }

    void Awake()
    {
        allNeighbours = new List<Avoidance>();
        //currentNearest = new GameObject();

        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
        {
            b.Encapsulate(r.bounds);
        }
        lattice = new Bucket[divisions, divisions];
        sizeOfLevel = Mathf.Max(b.size.x, b.size.z);
        gridOrigin = new Vector3(-sizeOfLevel / 2, 0, -sizeOfLevel / 2);

        for (int y = 0; y < divisions; y++)
        {
            for (int x = 0; x < divisions; x++)
            {
                lattice[x, y] = new Bucket(new Vector3(-sizeOfLevel / 2 + sizeOfLevel / divisions * x, 0, -sizeOfLevel / 2 + sizeOfLevel / divisions * y), sizeOfLevel / divisions, this, x, y);
            }
        }
        PopulateGrid();
        StartCoroutine("UpdateGridRow");
    }

    private void Update()
    {
        for (int y = 0; y < divisions; y++)
        {
            for (int x = 0; x < divisions; x++)
            {
                //lattice[y, x].DrawLines();
                //lattice[x, y].Update();   //updated in coroutine 
            }
        }
    }

    private IEnumerator UpdateGridRow()
    {
        int y = 0;
        while (true)
        {
            yield return null;

            if (y >= divisions)
                y = 0;

            currentRowEmpty = true;
            while (currentRowEmpty && y < divisions)
            {
                for (int x = 0; x < divisions; x++)
                {
                    lattice[y, x].Update();
                    lattice[y, x].DrawLines();
                    if (!lattice[y, x].empty)  
                        currentRowEmpty = false;  
                }
                y++;
            }
            
        }
    }

    public void GetNearestN(Avoidance agent, int n, List<Avoidance> neighbours)
    {
        int2 objIndex = GetObjectIndex(agent.gameObject);
        float closestDist = float.MaxValue;
        allNeighbours.Clear();

        //Get all agents from neighbour gridcells
        for (int y = -1; y < 1; y++)
        {
            for (int x = -1; x < 1; x++)
            {
                for (int i = 0; i < lattice[objIndex.x + x, objIndex.y + y].localAgents.Count; i++)
                {
                    if (lattice[objIndex.x + x, objIndex.y + y].localAgents[i] != agent)
                    {
                        allNeighbours.Add(lattice[objIndex.x + x, objIndex.y + y].localAgents[i]);
                    }
                }
            }
        }
        //Get closest agent from all neighbours N times and remove him from the list
        for (int i = 0; i < n; i++)
        {
            if (allNeighbours.Count == 0)
            {
                return;
            }
            closestDist = float.MaxValue;
            for (int k = 0; k < allNeighbours.Count; k++)
            {
                if ((allNeighbours[k].transform.position - agent.transform.position).magnitude < closestDist)
                {
                    closestDist = (allNeighbours[k].transform.position - agent.transform.position).magnitude;
                    currentNearest = allNeighbours[k];
                }
            }
            allNeighbours.Remove(currentNearest);

            //check if closest agent is already in the list of neighbours of the searching agent
            if (!neighbours.Contains(currentNearest))
            {
                neighbours.Add(currentNearest);
                //TODO: remove the farthest away neighbour if neighbours are more than N
                //TODO: figure out how the agent is going to let go of neighbours that are no longer relevant
                //RESOLVED: UpdateNeighbours function manages unnesesary neighbours
            }
        }
    }

    public void AddObject(Avoidance obj)
    {
        int2 objIndex = GetObjectIndex(obj.gameObject);
        lattice[objIndex.x, objIndex.y].localAgents.Add(obj);
    }

    public void RemoveObject(Avoidance obj)
    {
        int2 objIndex = GetObjectIndex(obj.gameObject);
        lattice[objIndex.x, objIndex.y].localAgents.Remove(obj);
    }

    int2 GetObjectIndex(GameObject obj)
    {
        int2 index = new int2();
        index.x = (int)((obj.transform.position - gridOrigin).x / (sizeOfLevel / divisions));
        index.y = (int)((obj.transform.position - gridOrigin).z / (sizeOfLevel / divisions));
        return index;
    }


    private void ClearGrid()
    {
        for (int y = 0; y < divisions; y++)
        {
            for (int x = 0; x < divisions; x++)
            {
                lattice[x, y].localAgents.Clear();
            }
        }
    }

    private void PopulateGrid()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Pawns"))
        {
            AddObject(obj.GetComponent<Avoidance>());
        }
    }
}
