using UnityEngine;

public enum GameState
{
    Reinforce,
    Action,
    MigrationCheck,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public int CurrentPlayerId { get; private set; } = 0; // 0 for Player 1, 1 for Player 2
    
    // Simple selection state
    private Tile selectedTile;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.Reinforce);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        // 3D Raycasting
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null)
            {
                OnTileClicked(clickedTile);
            }
        }
    }

    private void OnTileClicked(Tile clickedTile)
    {
        if (CurrentState == GameState.Action)
        {
            // If no tile is selected, try to select one of ours
            if (selectedTile == null)
            {
                if (clickedTile.OwnerPlayerId == CurrentPlayerId && clickedTile.CubeCount > 1)
                {
                    selectedTile = clickedTile;
                    Debug.Log($"Selected tile with {selectedTile.CubeCount} cubes.");
                }
            }
            // If we have a tile selected, try to attack an enemy tile
            else
            {
                if (clickedTile.OwnerPlayerId != CurrentPlayerId)
                {
                    // Basic adjacency check can be added here using HexGridManager.Instance.GetNeighbors()
                    int attackingDice = Mathf.Min(3, selectedTile.CubeCount - 1);
                    if (attackingDice > 0)
                    {
                        CombatManager.Instance.ResolveCombat(selectedTile, clickedTile, attackingDice);
                    }
                }
                
                // Deselect after action
                selectedTile = null;
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game State changed to: {CurrentState}");
        
        switch (CurrentState)
        {
            case GameState.Reinforce:
                ChangeState(GameState.Action);
                break;
            case GameState.Action:
                break;
            case GameState.MigrationCheck:
                EndTurn();
                break;
            case GameState.GameOver:
                break;
        }
    }

    public void EndTurn()
    {
        CurrentPlayerId = (CurrentPlayerId + 1) % 2; // Toggle between 0 and 1
        ChangeState(GameState.Reinforce);
    }

    public void OnEndTurnButtonClicked()
    {
        if (CurrentState == GameState.Action)
        {
            ChangeState(GameState.MigrationCheck);
        }
    }
}
