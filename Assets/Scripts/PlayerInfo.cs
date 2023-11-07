using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    /* ABILITY CODES
     * 1 = Chip Shot
     * 2 = Jump
     */
    
    public static readonly int[] LEVEL_EXP_THRESHOLDS = new int[] { 0, int.MaxValue };

    private static List<PlayerInfo> players = new List<PlayerInfo>();

    private List<int> abilities;
    private int totalExp, levelExp, level;


    public PlayerInfo()
    {
        abilities = new List<int>();
        totalExp = 0;
        levelExp = 0;
        level = 1;

        players.Add(this);
    }

    public static List<PlayerInfo> GetPlayers()
    {
        return players;
    }

    public void GainEXP(int expIn)
    {
        totalExp += expIn;
        levelExp += expIn;
        if (levelExp >= LEVEL_EXP_THRESHOLDS[level])
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        levelExp -= LEVEL_EXP_THRESHOLDS[level];
        level++;
        switch(level)
        {
            case 2:
                break;
            default:
                break;
        }
    }


}
