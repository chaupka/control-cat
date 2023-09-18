using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class AlienInitialize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BehaviourTreeRunner>().enabled = true;
        GetComponent<NavMeshAgent>().enabled = true;
    }
}
