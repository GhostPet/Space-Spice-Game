using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private GameInput gameInput;
	[SerializeField] private LayerMask itemLayerMask;

	private bool isWalking;
	private Vector3 lastInteractDirection;

	private void Update() {
		HandleMovement();
		HandleInteractions();
	}

	private void HandleInteractions() {

		Vector2 inputVector = gameInput.GetMovementVectorNormalized();
		Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

		float interactDistance = 2f;

		if (moveDirection != Vector3.zero) {
			lastInteractDirection = moveDirection;
		}

		if (Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit, interactDistance, itemLayerMask)) {
			// Fýrýn mý?
			if (raycastHit.transform.TryGetComponent(out Furnace furnace)) {
				if (gameInput.IsInteractPressed()) {
				furnace.Interact();
				}
			}
		}
	}

	private void HandleMovement() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();
		Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

		// Collision Detection
		float moveDistance = moveSpeed * Time.deltaTime;
		float playerRadius = .4f;
		float playerHeight = 2f;
		float playerBottomOffset = 1f;
		Vector3 playerBottom = transform.position + Vector3.up * playerBottomOffset;
		Vector3 playerTop = transform.position + (playerHeight - playerBottomOffset) * Vector3.up;


		bool canMove = !Physics.CapsuleCast(playerBottom, playerTop, playerRadius, moveDirection, moveDistance);
		if (canMove) {
			transform.position += moveDirection * moveDistance;
			isWalking = moveDirection != Vector3.zero;
		} else {
			// Try X movement only
			Vector3 moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
			canMove = !Physics.CapsuleCast(playerBottom, playerTop, playerRadius, moveDirectionX, moveDistance);
			if (canMove) {
				transform.position += moveDirectionX * moveDistance;
				isWalking = moveDirectionX != Vector3.zero;
			} else {
				// Try Z movement only
				Vector3 moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
				canMove = !Physics.CapsuleCast(playerBottom, playerTop, playerRadius, moveDirectionZ, moveDistance);
				if (canMove) {
					transform.position += moveDirectionZ * moveDistance;
					isWalking = moveDirectionZ != Vector3.zero;
				} else {
					isWalking = false;
				}
			}
		}

		// Looking at Move Direction
		float rotateSpeed = 10f;
		transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
	}

	public bool IsWalking() {
		return isWalking;
	}
}
