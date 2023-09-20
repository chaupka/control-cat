using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

public class InputController : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Start()
    {
        playerMovement = GameObject
            .FindGameObjectWithTag(LayerTag.playerTag)
            .GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCancelled;
        playerInput.Player.Jump.performed += OnJumpPerformed;
        playerInput.Player.Jump.canceled += OnJumpCanceled;
        playerInput.Player.Look.performed += OnLookPerformed;
        playerInput.Player.Look.canceled += OnLookCancelled;
        playerInput.Player.Dash.performed += OnDashPerformed;
        playerInput.Player.Interact.performed += OnInteractPerformed;
        playerInput.Player.Copy.performed += OnCopyPerformed;
        playerInput.Player.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        playerInput.Disable();
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCancelled;
        playerInput.Player.Jump.performed -= OnJumpPerformed;
        playerInput.Player.Jump.canceled -= OnJumpCanceled;
        playerInput.Player.Look.performed -= OnLookPerformed;
        playerInput.Player.Look.canceled -= OnLookCancelled;
        playerInput.Player.Dash.performed -= OnDashPerformed;
        playerInput.Player.Interact.performed -= OnInteractPerformed;
        playerInput.Player.Copy.performed -= OnCopyPerformed;
        playerInput.Player.Pause.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        GameStateController.instance.TogglePauseGame();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        playerMovement.movingRight = context.ReadValue<Vector2>().x;
    }

    private void OnMoveCancelled(InputAction.CallbackContext context)
    {
        playerMovement.movingRight = 0;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        playerMovement.OnLookingUpOrDownPerformed(context.ReadValue<Vector2>().y);
    }

    private void OnLookCancelled(InputAction.CallbackContext context)
    {
        playerMovement.OnLookingUpOrDownCancelled();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        playerMovement.OnJumpPerformed();
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        playerMovement.OnJumpCancelled();
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        playerMovement.OnDashStarted();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        playerMovement.OnInteractPerformed();
    }

    private void OnCopyPerformed(InputAction.CallbackContext context)
    {
        playerMovement.OnCopyPerformed();
    }
}
