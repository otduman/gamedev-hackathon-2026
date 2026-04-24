using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Resolves Risk-style combat between two tiles.
    /// Returns true if the attacker successfully conquered the tile.
    /// </summary>
    public bool ResolveCombat(Tile attackerTile, Tile defenderTile, int attackingDiceCount)
    {
        // Must leave at least 1 cube behind
        if (attackerTile.CubeCount <= attackingDiceCount)
        {
            Debug.LogWarning("Not enough cubes to attack. Must leave at least 1 cube behind.");
            return false;
        }

        int defendingDiceCount = Mathf.Min(2, defenderTile.CubeCount);
        
        List<int> attackRolls = RollDice(attackingDiceCount);
        List<int> defendRolls = RollDice(defendingDiceCount);
        
        int attackerLosses = 0;
        int defenderLosses = 0;

        // Compare highest dice
        int comparisons = Mathf.Min(attackRolls.Count, defendRolls.Count);
        for (int i = 0; i < comparisons; i++)
        {
            if (attackRolls[i] > defendRolls[i])
            {
                defenderLosses++;
            }
            else
            {
                // Defender wins ties in Risk
                attackerLosses++;
            }
        }

        // Apply losses
        attackerTile.RemoveCubes(attackerLosses);
        defenderTile.RemoveCubes(defenderLosses);

        Debug.Log($"Combat Result: Attacker lost {attackerLosses}, Defender lost {defenderLosses}");

        // Check for conquest
        if (defenderTile.CubeCount == 0)
        {
            Debug.Log("Tile conquered!");
            defenderTile.OwnerPlayerId = attackerTile.OwnerPlayerId;
            
            // Move minimum cubes over (the amount of dice rolled minus losses)
            int survivingAttackingCubes = attackingDiceCount - attackerLosses;
            attackerTile.RemoveCubes(survivingAttackingCubes);
            defenderTile.AddCubes(survivingAttackingCubes);

            return true;
        }

        return false;
    }

    private List<int> RollDice(int count)
    {
        List<int> rolls = new List<int>();
        for (int i = 0; i < count; i++)
        {
            rolls.Add(Random.Range(1, 7)); // D6
        }
        rolls.Sort((a, b) => b.CompareTo(a)); // Sort descending (highest first)
        return rolls;
    }
}
