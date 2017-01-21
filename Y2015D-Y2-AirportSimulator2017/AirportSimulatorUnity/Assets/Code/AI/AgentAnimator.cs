using UnityEngine;
using System.Collections;

public class AgentAnimator : MonoBehaviour
{
    public Animator animator;
    public UnityEngine.AI.NavMeshAgent navAgent;

    private void Update()
    {
        
        //animator.SetFloat("Speed_f", Mathf.Clamp(navAgent.velocity.magnitude / 10.0f, 0.0f, 1.0f));
    }
}
