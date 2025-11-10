using UnityEngine;

public class GameInput : MonoBehaviour
{
	private InputSystem_Actions inputSystem_Actions;

	private void Awake() {
		inputSystem_Actions = new InputSystem_Actions();
		inputSystem_Actions.Player.Enable();
	}

	public Vector2 GetMovementVectorNormalized() {
		Vector2 inputVector = inputSystem_Actions.Player.Move.ReadValue<Vector2>();

		inputVector = inputVector.normalized;

		return inputVector;

	}

	public bool IsInteractPressed() {
		return inputSystem_Actions.Player.Interact.triggered;
	}
}
