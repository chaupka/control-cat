using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;

namespace TheKiwiCoder
{
    // The context is a shared object every node has access to.
    // Commonly used components and subsytems should be stored here
    // It will be somewhat specfic to your game exactly what to add here.
    // Feel free to extend this class
    public class Context
    {
        public GameObject alien;
        public Transform transform;
        public SpriteRenderer renderer;
        public Animator animator;
        public Rigidbody physics;
        public NavMeshAgent agent;
        public NavMeshSurface navMeshSurface;
        public SphereCollider sphereCollider;
        public BoxCollider boxCollider;
        public CapsuleCollider capsuleCollider;
        public Transform headTransform;
        public GameObject head;

        public static Context CreateFromGameObject(GameObject gameObject)
        {
            // Fetch all commonly used components
            Context context = new Context { alien = gameObject, transform = gameObject.transform };
            context.renderer = context.transform.GetComponent<SpriteRenderer>();
            context.animator = gameObject.GetComponent<Animator>();
            context.physics = gameObject.GetComponent<Rigidbody>();
            context.agent = gameObject.GetComponent<NavMeshAgent>();
            context.navMeshSurface = (NavMeshSurface)context.agent.navMeshOwner;
            context.agent.updateRotation = false;
            context.agent.updateUpAxis = false;
            context.sphereCollider = gameObject.GetComponent<SphereCollider>();
            context.boxCollider = gameObject.GetComponent<BoxCollider>();
            context.capsuleCollider = gameObject.GetComponent<CapsuleCollider>();

            // Head
            context.headTransform = GetHead(gameObject.transform);
            context.head = context.headTransform.gameObject;

            return context;
        }

        public static Transform GetHead(Transform transform)
        {
            return transform.Find("Head");
        }
    }
}
