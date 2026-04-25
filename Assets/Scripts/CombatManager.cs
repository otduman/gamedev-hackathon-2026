using UnityEngine;
using System.Collections.Generic;

public class CombatResult
{
    public int[] AttackerRolls;   // sorted descending (may include bonus die)
    public int[] DefenderRolls;   // sorted descending
    public int   AttackerLosses;
    public int   DefenderLosses;
    public bool  Conquered;
    public bool  ForestBonus;     // true when attacker rolled from forest
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public CombatResult LastResult { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Resolves Risk-style combat. attackerForestBonus grants one extra die (best N used).
    public bool ResolveCombat(Tile attackerTile, Tile defenderTile, int attackingDiceCount, bool attackerForestBonus = false)
    {
        if (attackerTile.CubeCount <= attackingDiceCount)
        {
            Debug.LogWarning("Not enough cubes to attack. Must leave at least 1 cube behind.");
            return false;
        }

        int defendingDiceCount = Mathf.Min(2, defenderTile.CubeCount);

        // Forest bonus: roll one extra die, discard the lowest (since list is sorted desc, just trim)
        int diceToRoll = attackerForestBonus ? attackingDiceCount + 1 : attackingDiceCount;
        List<int> allAttackRolls = RollDice(diceToRoll);
        // Use only the top attackingDiceCount dice for comparisons
        List<int> attackRolls = allAttackRolls.GetRange(0, Mathf.Min(attackingDiceCount, allAttackRolls.Count));
        List<int> defendRolls = RollDice(defendingDiceCount);

        int attackerLosses = 0;
        int defenderLosses = 0;

        int comparisons = Mathf.Min(attackRolls.Count, defendRolls.Count);
        for (int i = 0; i < comparisons; i++)
        {
            if (attackRolls[i] > defendRolls[i])
                defenderLosses++;
            else
                attackerLosses++; // defender wins ties
        }

        attackerTile.RemoveCubes(attackerLosses);
        defenderTile.RemoveCubes(defenderLosses);

        bool conquered = false;
        if (defenderTile.CubeCount == 0)
        {
            // Move surviving attacking cubes forward; ownership handled by GameManager (contested mechanic)
            int survivingAttackingCubes = attackingDiceCount - attackerLosses;
            attackerTile.RemoveCubes(survivingAttackingCubes);
            defenderTile.AddCubes(survivingAttackingCubes);
            conquered = true;
        }

        LastResult = new CombatResult
        {
            AttackerRolls  = allAttackRolls.ToArray(),
            DefenderRolls  = defendRolls.ToArray(),
            AttackerLosses = attackerLosses,
            DefenderLosses = defenderLosses,
            Conquered      = conquered,
            ForestBonus    = attackerForestBonus,
        };

        return conquered;
    }

    private List<int> RollDice(int count)
    {
        List<int> rolls = new List<int>();
        for (int i = 0; i < count; i++)
            rolls.Add(Random.Range(1, 7));
        rolls.Sort((a, b) => b.CompareTo(a));
        return rolls;
    }
}
