using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private GameInput gameInput;

	[System.Serializable]
	private struct InteractionSettings
	{
		[Tooltip("Which layers are considered interactable (items)")]
		public LayerMask itemLayerMask;

		[Tooltip("Which layers are considered floor (floors)")]
		public LayerMask floorLayerMask;

		[Tooltip("Max distance for interaction raycast / spherecast")]
		public float distance;

		[Tooltip("Height offset from the player's position to cast interactions from")]
		public float originHeight;

		[Tooltip("Radius for spherecast to make targeting more forgiving")]
		public float radius;

		[Tooltip("Within this distance interaction allowed regardless of look angle")]
		public float closeDistance;

		[Tooltip("Angle (degrees) from look direction within which items will be highlighted")]
		public float highlightAngle;
	}

	[System.Serializable]
	private struct HighlightSettings
	{
		[Tooltip("Enable visual highlight on nearby items")]
		public bool enable;

		[Tooltip("Color used for highlighting items")]
		public Color color;
	}

	[SerializeField] private InteractionSettings interaction = new InteractionSettings { itemLayerMask = ~0, distance = 2f, originHeight = 1f, radius = 0.2f, closeDistance = 0.8f, highlightAngle = 60f };
	[SerializeField] private HighlightSettings highlight = new HighlightSettings { enable = true, color = new Color(1f, 0.9f, 0.3f, 1f) };

	private GameObject currentHighlighted;
	private GameObject currentSelected;

	private bool isWalking;
	private Vector3 lastInteractDirection;

	// Enum: GameState [TileEdit, ItemEdit, ItemInteract]
	private enum GameState
	{
		TileEdit,
		ItemEdit,
		ItemInteract
	}
	private GameState gameState;

	private void Start()
	{
		gameState = GameState.ItemInteract;
		lastInteractDirection = transform.forward;
		gameInput.OnInteractAction += GameInput_OnInteractAction;
	}

	private void GameInput_OnInteractAction(object sender, System.EventArgs e)
	{
		// Selected item interact
		if (currentSelected != null) {
			if (gameState == GameState.TileEdit) {
				// Tile edit interaction (not implemented)
			} else if (gameState == GameState.ItemEdit) {
				// Item move interaction
				if (currentSelected.TryGetComponent(out Item item)) {
					item.Move();
				}
			} else if (gameState == GameState.ItemInteract) {
				// Item interaction
				if (currentSelected.TryGetComponent(out Item item)) {
					item.Interact();
				}
			}
		} else {
			// No item selected Change game state
			if (gameState == GameState.TileEdit) {
				gameState = GameState.ItemEdit;
				Debug.Log("Switched to Item Edit Mode");
			} else if (gameState == GameState.ItemEdit) {
				gameState = GameState.ItemInteract;
				Debug.Log("Switched to Item Interact Mode");
			} else if (gameState == GameState.ItemInteract) {
				gameState = GameState.TileEdit;
				Debug.Log("Switched to Tile Edit Mode");
			}

			// After changing game state, remove any highlights
			if (currentHighlighted != null) {
				if (currentHighlighted.TryGetComponent(out Item previousItem)) {
					previousItem.RemoveHighlight();
				} else if (currentHighlighted.TryGetComponent(out Floor previousFloor)) {
					previousFloor.RemoveHighlight();
				}
				currentHighlighted = null;
			}
		}
	}

	private void Update()
	{
		HandleMovement();
		HandleHighlight();
	}

	private void HandleHighlight()
	{
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();
		Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
		if (moveDirection.sqrMagnitude > 0.0001f)
		{
			lastInteractDirection = moveDirection.normalized;
		}
		Vector3 origin = transform.position + Vector3.up * interaction.originHeight;
		Vector3 dir = lastInteractDirection.sqrMagnitude > 0.0001f ? lastInteractDirection.normalized : transform.forward;

		// If GameState is ItemInteract or ItemEdit, use SphereCast to find item
		if (gameState == GameState.ItemInteract || gameState == GameState.ItemEdit)
		{
			if (Physics.SphereCast(origin, interaction.radius, dir, out RaycastHit hit, interaction.distance, interaction.itemLayerMask))
			{
				var previousSelected = currentSelected;
				var previousHighlighted = currentHighlighted;
				currentSelected = hit.collider.gameObject;
				currentHighlighted = null;
				// If highlighting enabled, highlight
				if (currentSelected.TryGetComponent(out Item item))
				{
					if (item.IsHighlightable() && highlight.enable)
					{
						item.Highlight(highlight.color);
					}
					currentHighlighted = currentSelected;
				}
				// Remove previous highlight if different
				if (previousHighlighted != null && previousHighlighted != currentHighlighted)
				{
					if (previousHighlighted.TryGetComponent(out Item previousItem))
					{
						previousItem.RemoveHighlight();
					}
				}
			}
			else
			{
				// No item hit, remove selection/highlight
				if (currentHighlighted != null)
				{
					if (currentHighlighted.TryGetComponent(out Item previousItem))
					{
						previousItem.RemoveHighlight();
					}
					currentHighlighted = null;
				}
				currentSelected = null;
			}
		}
		// Else If GameState is TileEdit, use SphereCast to find tile
		else if (gameState == GameState.TileEdit)
		{
			// Change origin to be slightly lower to better hit floor
			var floorOrigin = transform.position + Vector3.up * (interaction.originHeight - 1f);
			if (Physics.SphereCast(floorOrigin, interaction.radius, dir, out RaycastHit hit, interaction.distance, interaction.floorLayerMask))
			{
				var previousSelected = currentSelected;
				var previousHighlighted = currentHighlighted;
				currentSelected = hit.collider.gameObject;
				currentHighlighted = null;
				// If highlighting enabled, highlight
				if (currentSelected.TryGetComponent(out Floor floor))
				{
					if (floor.IsHighlightable() && highlight.enable)
					{
						floor.Highlight(highlight.color);
					}
					currentHighlighted = currentSelected;
				}

			} else
			{
				// No floor hit, remove selection/highlight
				if (currentHighlighted != null) {
					if (currentHighlighted.TryGetComponent(out Floor previousFloor)) {
						previousFloor.RemoveHighlight();
					}
					currentHighlighted = null;
				}
				currentSelected = null;
				currentSelected = null;
			}
		}
		else
		{
			// Create exception for unhandled game state
			throw new System.Exception("Unhandled game state: " + gameState);
		}
	}

	private void HandleMovement()
	{
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
		if (canMove)
		{
			transform.position += moveDirection * moveDistance;
			isWalking = moveDirection != Vector3.zero;
		}
		else
		{
			// Try X movement only
			Vector3 moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
			canMove = !Physics.CapsuleCast(playerBottom, playerTop, playerRadius, moveDirectionX, moveDistance);
			if (canMove)
			{
				transform.position += moveDirectionX * moveDistance;
				isWalking = moveDirectionX != Vector3.zero;
			}
			else
			{
				// Try Z movement only
				Vector3 moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
				canMove = !Physics.CapsuleCast(playerBottom, playerTop, playerRadius, moveDirectionZ, moveDistance);
				if (canMove)
				{
					transform.position += moveDirectionZ * moveDistance;
					isWalking = moveDirectionZ != Vector3.zero;
				}
				else
				{
					isWalking = false;
				}
			}
		}

		// Looking at Move Direction
		float rotateSpeed = 10f;
		transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
	}

	private void OnDrawGizmosSelected()
	{
		// Draw interaction visualization
		Vector3 origin = transform.position + Vector3.up * interaction.originHeight;
		Vector3 dir = lastInteractDirection.sqrMagnitude > 0.0001f ? lastInteractDirection.normalized : transform.forward;

		// SphereCast small radius at origin
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(origin, interaction.radius);

		// Max reach point
		Vector3 reachPoint = origin + dir * interaction.distance;
		Gizmos.DrawWireSphere(reachPoint, interaction.radius);
		Gizmos.DrawLine(origin, reachPoint);

		// Draw highlight cone
		Gizmos.color = Color.yellow;
		int steps = 20;
		float halfAngle = interaction.highlightAngle * 0.5f;
		for (int i = 0; i <= steps; i++)
		{
			float a = Mathf.Lerp(-halfAngle, halfAngle, i / (float)steps);
			Quaternion rot = Quaternion.AngleAxis(a, Vector3.up);
			Vector3 rDir = rot * dir;
			Gizmos.DrawLine(origin, origin + rDir * interaction.distance);
		}

		// Draw close interact sphere
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(origin + dir * Mathf.Min(interaction.closeDistance, interaction.distance), interaction.closeDistance);
	}

	public bool IsWalking()
	{
		return isWalking;
	}
}
