using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
	public event EventHandler OnInteractAction;
	private InputSystem_Actions inputSystem_Actions;

	private void Awake() {
		inputSystem_Actions = new InputSystem_Actions();
		inputSystem_Actions.Player.Enable();

		inputSystem_Actions.Player.Interact.performed += Interact_performed;
	}

	private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		OnInteractAction?.Invoke(this, EventArgs.Empty);
	}

	public Vector2 GetMovementVectorNormalized() {
		Vector2 inputVector = inputSystem_Actions.Player.Move.ReadValue<Vector2>();
		inputVector = inputVector.normalized;
		return inputVector;

	}
}
