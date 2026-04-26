using UnityEngine;
using System.Collections.Generic;

public class CombatResult
{
    public int[] AttackerRolls;
    public int[] DefenderRolls;
    public int   AttackerLosses;
    public int   DefenderLosses;
    public bool  Conquered;
    public int   AttackingDiceUsed;
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

    // attackingDiceCount: 1..3 (capped to attacker.CubeCount-1).
    // defendingDiceCount: 1..2 (capped to defender.CubeCount).
    public bool ResolveCombat(Tile attackerTile, Tile defenderTile,
                              int attackingDiceCount, int defendingDiceCount)
    {
        if (attackerTile.CubeCount <= attackingDiceCount)
        {
            Debug.LogWarning("Not enough cubes to attack. Must leave at least 1 cube behind.");
            return false;
        }

        attackingDiceCount = Mathf.Clamp(attackingDiceCount, 1, Mathf.Min(3, attackerTile.CubeCount - 1));
        defendingDiceCount = Mathf.Clamp(defendingDiceCount, 1, Mathf.Min(2, defenderTile.CubeCount));

        List<int> allAttackRolls = RollDice(attackingDiceCount);
        List<int> attackRolls    = allAttackRolls;
        List<int> defendRolls    = RollDice(defendingDiceCount);

        int attackerLosses = 0;
        int defenderLosses = 0;

        int comparisons = Mathf.Min(attackRolls.Count, defendRolls.Count);
        for (int i = 0; i < comparisons; i++)
        {
            // Reverse pairing: attacker's highest die is paired with defender's LOWEST,
            // attacker's next with defender's next-lowest, etc. A single huge attacker
            // roll can't sweep both pairs — defender's high die still gets to compete
            // against the attacker's lower one. Defender wins ties.
            int atkDie = attackRolls[i];
            int defDie = defendRolls[comparisons - 1 - i];
            if (atkDie > defDie)
                defenderLosses++;
            else
                attackerLosses++;
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
            AttackerRolls     = allAttackRolls.ToArray(),
            DefenderRolls     = defendRolls.ToArray(),
            AttackerLosses    = attackerLosses,
            DefenderLosses    = defenderLosses,
            Conquered         = conquered,
            AttackingDiceUsed = attackingDiceCount,
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
