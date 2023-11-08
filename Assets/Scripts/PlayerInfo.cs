using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    /* ABILITY CODES
     * 0 = Chip Shot
     * 1 = Jump
     * 2 =
     * 3 =
     * 4 =
     * 5 =
     * 6 =
     * 7 =
     * 8 =
     * 9 =
     * 10 =
     */
    
    public static readonly int[] LEVEL_EXP_THRESHOLDS = new int[] { 0, 100, 150, 200, 250, 100000 };

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

    public int GetLevel()
    {
        return level;
    }

    //Returns how much exp the player has gained this level
    public int GetLevelExp()
    {
        return levelExp;
    }

    //Returns true if the exp gained caused the player to level up, false otherwise
    public bool GainEXP(int expIn)
    {
        totalExp += expIn;
        levelExp += expIn;
        if (levelExp >= LEVEL_EXP_THRESHOLDS[level])
        {
            LevelUp();
            return true;
        }
        return false;
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
