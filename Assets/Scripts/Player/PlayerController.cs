using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //moving
    // Vector2[] lookAround = {Vector2.up, Vector2.right, Vector2.down, Vector2.left};
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public Vector2 lookDirection = new Vector2(0, -1);

    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetMovementInput();
    }

    private void GetMovementInput()
    {
        if (!Mathf.Approximately(playerMovement.movingRight, 0.0f))
        {
            lookDirection.Set(playerMovement.movingRight, 0);
            lookDirection.Normalize();
        }

        // animator.SetFloat("Look X", lookDirection.x);
        // animator.SetFloat("Look Y", lookDirection.y);
        // animator.SetFloat("Speed", move.magnitude);
    }

}
