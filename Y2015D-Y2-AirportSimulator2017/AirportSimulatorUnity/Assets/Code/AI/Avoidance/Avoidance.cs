using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;

public class Avoidance : MonoBehaviour
{
    public enum CollisionType
    {
        FACE_TO_FACE,
        FACE_TO_BACK,
        SIDE_TO_SIDE
    }

    public struct PosAndRot
    {
        public Vector3 position;
        public Vector3 forward;
        public Quaternion rotation;
    }

    public struct FutureCollision
    {
        public CollisionType type;
        public PosAndRot impactTransform;
        public Avoidance otherAvoidance;
        public float impactTime;
    }
    public List<Avoidance> nearestAgents = new List<Avoidance>();
    public PosAndRot[] futureTransforms;

    [SerializeField]
    private int maxNearestAgents = 5;

    public bool drawPath = false;

    public UnityEngine.AI.NavMeshAgent agent;
    public QueueMember queueMember;

    private Avoidance currentFarthestAway;
    private FutureCollision futureCol;
    private CollisionType futureCollision;
    private float timeStep = 0.2f;
    private float initSpeed;
    private float steeringSpeed = 0.2f;
    private Vector3 actionPosition;
    private Vector3 steeringVelocity;
    public bool alteringPath;
    public bool waiting;
    public bool futureImpact;

    void Awake()
    {
        queueMember = GetComponent<QueueMember>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        futureTransforms = new PosAndRot[10];
        initSpeed = agent.speed;
    }

    void LateUpdate()
    {
        //UpdateNeighbours();   //called from the gridcell

        if (alteringPath)
        {
            agent.velocity = steeringVelocity;
        }

        //TODO: Avoid other agents
        //"Every agent I know, knows i hate agents"
    }

    public void UpdateNeighbours()
    {
        if (nearestAgents.Count > maxNearestAgents)
        {
            //Remove farthest away agents from list
            int iterations = nearestAgents.Count - maxNearestAgents;
            float maxDist = 0;
            for (int i = 0; i < iterations; i++)
            {
                foreach (Avoidance obj in nearestAgents)
                {
                    if (obj == null)
                        continue;
                    if ((obj.transform.position - transform.position).magnitude > maxDist)
                    {
                        maxDist = (obj.transform.position - transform.position).magnitude;
                        currentFarthestAway = obj;
                    }
                }
                nearestAgents.Remove(currentFarthestAway);
            }
        }
    }

    public bool PredictCollisionsCheap()
    {
        futureImpact = false;
        for (int i = 0; i < 10; i++)
        {
            foreach (Avoidance neighbour in nearestAgents)
            {
                if (Vector3.Distance(futureTransforms[i].position, neighbour.futureTransforms[i].position) < agent.radius * 2)
                {
                    //define collision type
                    float dot = Vector3.Dot(futureTransforms[i].forward, neighbour.futureTransforms[i].forward);
                    if (dot < -0.9f)
                        futureCol.type = CollisionType.FACE_TO_FACE;
                    else if (dot > 0.9f)
                        futureCol.type = CollisionType.FACE_TO_BACK;
                    else
                        futureCol.type = CollisionType.SIDE_TO_SIDE;

                    futureCol.impactTransform = futureTransforms[i];
                    futureCol.impactTime = i * timeStep;
                    futureCol.otherAvoidance = neighbour;
                    futureImpact = true;

                    //early exit on first impact
                    return futureImpact;
                }
            }
        }

        alteringPath = false;
        waiting = false;
        return futureImpact;
    }

    public bool PredictCollisions(float time)
    {
        PosAndRot myTransformAtTime;
        PosAndRot otherTransformAtTime;

        futureImpact = false;

        for (float t = 0; t < time; t += timeStep)
        {
            myTransformAtTime = GetTransformAtDistanceAlongPath(agent.path, agent.speed * t);
            foreach (Avoidance neighbour in nearestAgents)
            {
                otherTransformAtTime = neighbour.GetTransformAtDistanceAlongPath(neighbour.agent.path, neighbour.agent.speed * t);
                if (Vector3.Distance(myTransformAtTime.position, otherTransformAtTime.position) < agent.radius * 2)
                {
                    //define collision type
                    float dot = Vector3.Dot(myTransformAtTime.forward, otherTransformAtTime.forward);
                    if (dot < -0.9f)
                        futureCol.type = CollisionType.FACE_TO_FACE;
                    else if (dot > 0.9f)
                        futureCol.type = CollisionType.FACE_TO_BACK;
                    else
                        futureCol.type = CollisionType.SIDE_TO_SIDE;

                    futureCol.impactTransform = myTransformAtTime;
                    futureCol.impactTime = t;
                    futureCol.otherAvoidance = neighbour;
                    futureImpact = true;

                    //early exit on first impact
                    return futureImpact;
                }
            }
        }
        alteringPath = false;
        waiting = false;
        return futureImpact;
    }

    public void UpdateFutureTransforms(float time)
    {
        int i = 0;
        for (float t = 0; t < time; t += timeStep)
        {
            futureTransforms[i] = GetTransformAtDistanceAlongPath(agent.path, agent.speed * t);
            i++;
        }
    }

    public void ResolveCollisions()
    {
        agent.Resume();
        if (futureImpact)
        {

            Debug.DrawLine(transform.position, futureCol.impactTransform.position, Color.red);
            switch (futureCol.type)
            {
                case CollisionType.FACE_TO_BACK:
                    ResolveTrivial();
                    break;
                case CollisionType.FACE_TO_FACE:
                    ResolveWithSteering();
                    break;
                case CollisionType.SIDE_TO_SIDE:
                    if (!ResolveTrivial())
                        ResolveWithSteering();
                    break;
            }
            //if (ResolveTrivial())
            //{
            //    agent.Resume();
            //}
            ////TODO: If speed adjustment doesn't resolve, try path adjustment
            //else
            //{
            //    if (!alteringPath)
            //    {
            //        ResolveWithSteering();
            //        agent.speed = initSpeed;
            //        //ResolveNontrivial();
            //    }
            //}
        }
        else
        {
            //TODO: Decide when to reset speed to the default one
            if (!waiting)
                agent.speed = initSpeed;

        }
    }

    private void ResolveWithSteering()
    {
        bool resolved = false;
        UnityEngine.AI.NavMeshHit hit;
        Vector3 targetPos = transform.position + transform.right * agent.radius * 2;

        if (!futureCol.otherAvoidance.alteringPath)
        {
            if (!agent.Raycast(targetPos, out hit))
            {
                agent.velocity += transform.right * steeringSpeed;
                agent.velocity = agent.velocity.normalized * agent.speed;
                steeringVelocity = agent.velocity;
                Debug.DrawLine(transform.position, targetPos, Color.green);
                alteringPath = true;
            }
            else
            {
                targetPos = transform.position + transform.right * agent.radius * -2;
                if (!agent.Raycast(targetPos, out hit))
                {
                }
                Debug.DrawLine(transform.position, targetPos, Color.green);
                agent.velocity += transform.right * -steeringSpeed;
                agent.velocity = agent.velocity.normalized * agent.speed;
                steeringVelocity = agent.velocity;
                alteringPath = true;

            }
        }
    }

    private void ResolveNontrivial()
    {
        bool resolved = false;


        if (!alteringPath)
        {
            //save last Destination
            actionPosition = agent.destination;
        }
        else
        {
            return;
        }
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMeshPath newPath = new UnityEngine.AI.NavMeshPath();

        //get pos at impact time
        //try left of it 
        Vector3 newDestination = futureCol.impactTransform.position + transform.right * agent.radius * 5;
        if (UnityEngine.AI.NavMesh.SamplePosition(newDestination, out hit, 0, UnityEngine.AI.NavMesh.AllAreas))
        {
            UnityEngine.AI.NavMesh.CalculatePath(transform.position, newDestination, UnityEngine.AI.NavMesh.AllAreas, newPath);
            agent.SetPath(newPath);
        }
        //if no col detected
        if (!CheckCollisionAtTime(futureCol.impactTime))
        {
            //assign new destination
            //flip a flag for altering the path
            agent.SetDestination(newDestination);
            alteringPath = true;
        }
        else
        {
            //do the same for a pos to the right of the impactPos
            newDestination = futureCol.impactTransform.position + transform.right * agent.radius * -5;
            if (UnityEngine.AI.NavMesh.SamplePosition(newDestination, out hit, 0, UnityEngine.AI.NavMesh.AllAreas))
            {
                UnityEngine.AI.NavMesh.CalculatePath(transform.position, newDestination, UnityEngine.AI.NavMesh.AllAreas, newPath);
                agent.SetPath(newPath);
            }
            if (!CheckCollisionAtTime(futureCol.impactTime))
            {
                agent.SetDestination(newDestination);
                alteringPath = true;
            }
            else
            {
                resolved = false;
            }
        }
        //apparently in unity 5 assigning a new path to an agent could cause the agent to stop
        agent.Resume();

    }

    private bool ResolveTrivial()
    {
        bool resolved = false;

        for (float i = 0.5f; i <= 1.5f; i += 1f)
        {
            agent.speed = initSpeed * i;
            if (!CheckCollisionAtTime(futureCol.impactTime))
            {
                resolved = true;
                break;
            }
        }
        if (!resolved)
        {
            agent.speed = 0.1f * initSpeed;
            //agent.speed = initSpeed;
            if (!CheckCollisionAtTime(futureCol.impactTime))
            {
                //agent.Stop();
                waiting = true;
                resolved = true;
            }
            else
            {
                agent.speed = initSpeed;
            }
        }

        return resolved;
    }

    private bool CheckCollisionAtTime(float t)
    {
        //less expensive function for double checking if future impact is resolved

        PosAndRot myTransformAtTime;
        PosAndRot otherTransformAtTime;

        futureImpact = false;

        myTransformAtTime = GetTransformAtDistanceAlongPath(agent.path, agent.speed * t);
        foreach (Avoidance neighbour in nearestAgents)
        {
            otherTransformAtTime = neighbour.GetTransformAtDistanceAlongPath(neighbour.agent.path, neighbour.agent.speed * t);

            if (Vector3.Distance(myTransformAtTime.position, otherTransformAtTime.position) < agent.radius * 2)
            {
                //define collision type
                float dot = Vector3.Dot(myTransformAtTime.forward, otherTransformAtTime.forward);
                if (dot < -0.9f)
                    futureCol.type = CollisionType.FACE_TO_FACE;
                else if (dot > 0.9f)
                    futureCol.type = CollisionType.FACE_TO_BACK;
                else
                    futureCol.type = CollisionType.SIDE_TO_SIDE;

                futureCol.impactTransform = myTransformAtTime;
                futureCol.impactTime = t;
                futureCol.otherAvoidance = neighbour;
                futureImpact = true;

                //early exit on first impact
                return futureImpact;
            }
        }

        return futureImpact;
    }

    public Vector3 GetPosAtDistanceAlongPath(UnityEngine.AI.NavMeshPath path, float distance)
    {
        Vector3 predictedPos = new Vector3();
        //TODO: make some crazy calculations

        float pathLenght = GetPathLength(path);
        bool agentBetweenPoints = false;
        int i = 1;

        if (path.corners.Length < 2)
        {
            return predictedPos;
        }

        if (distance > pathLenght)
        {
            predictedPos = agent.pathEndPosition;
            return predictedPos;
        }
        while (!agentBetweenPoints)
        {
            if (distance < GetPathLengthAtPoint(path, i))
            {
                agentBetweenPoints = true;
                break;
            }
            distance -= Vector3.Distance(path.corners[i - 1], path.corners[i]);
            i++;
        }
        predictedPos = path.corners[i - 1] + Vector3.Normalize(path.corners[i] - path.corners[i - 1]) * distance;

        return predictedPos;
    }

    public PosAndRot GetTransformAtDistanceAlongPath(UnityEngine.AI.NavMeshPath path, float distance)
    {
        PosAndRot predictedTransform = new PosAndRot();
        //TODO: make some crazy calculations

        float remainingDistance = distance;
        //float pathLenght = GetPathLength(path);
        bool agentBetweenPoints = false;
        int i = 1;

        if (path.corners.Length < 2)
        {
            predictedTransform.position = transform.position;
            predictedTransform.rotation = transform.rotation;
            predictedTransform.forward = transform.forward;
            return predictedTransform;
        }

        if (distance > GetPathLengthAtPoint(path, path.corners.Length - 1))
        {
            predictedTransform.position = agent.pathEndPosition;
            predictedTransform.rotation = Quaternion.LookRotation((path.corners[path.corners.Length - 1] - path.corners[path.corners.Length - 2]).normalized, Vector3.up);
            predictedTransform.forward = (path.corners[path.corners.Length - 1] - path.corners[path.corners.Length - 2]).normalized;
            return predictedTransform;
        }
        while (!agentBetweenPoints)
        {

            float dist = GetPathLengthAtPoint(path, i);
            if (distance <= dist)
            {
                agentBetweenPoints = true;
                break;
            }

            if (i >= path.corners.Length)
            {
                int k = 1;
            }

            remainingDistance -= Vector3.Distance(path.corners[i - 1], path.corners[i]);

            i++;

        }

        predictedTransform.position = path.corners[i - 1] + Vector3.Normalize(path.corners[i] - path.corners[i - 1]) * remainingDistance;
        predictedTransform.rotation = Quaternion.LookRotation((path.corners[i] - path.corners[i - 1]).normalized, Vector3.up);
        predictedTransform.forward = (path.corners[i] - path.corners[i - 1]).normalized;
        return predictedTransform;
    }

    private float GetPathLength(UnityEngine.AI.NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return 0;

        Vector3 previousCorner = path.corners[0];
        float lengthSoFar = 0.0F;
        int i = 1;
        if (drawPath)
        {
            while (i < path.corners.Length)
            {

                Debug.DrawLine(path.corners[i - 1], path.corners[i]);

                i++;
            }
        }
        while (i < path.corners.Length)
        {
            Vector3 currentCorner = path.corners[i];
            if (drawPath)
            {
                Debug.DrawLine(path.corners[i - 1], path.corners[i]);
            }
            lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
            i++;
        }

        //lengthSoFar = agent.remainingDistance;
        return lengthSoFar;
    }

    private float GetPathLengthAtPoint(UnityEngine.AI.NavMeshPath path, int point)
    {
        if (path.corners.Length < 2)
            return 0;

        Vector3 previousCorner = path.corners[0];
        float lengthSoFar = 0.0F;
        int i = 1;
        if (point < path.corners.Length)
        {
            while (i <= point)
            {
                Vector3 currentCorner = path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
        }
        else
        {
            lengthSoFar = agent.remainingDistance;
        }

        return lengthSoFar;
    }
}
