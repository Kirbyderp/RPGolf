using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    /* ABILITY CODES
     * 0 = Chip Shot
     * 1 = Jump
     * 2 = Fireball
     * 3 = Spike Ball
     * 4 = Damage Up
     * 5 =
     * 6 =
     * 7 =
     * 8 =
     * 9 =
     * 10 =
     */

    public static readonly bool[] IS_GENERIC_KEY = new bool[] { false, false, true, true, false };
    public static readonly string[] REWARD_NAMES = new string[] {"Chip Shot", "Jump",
                                                                 "Fireball", "Spike Ball", "Damage Up"};
    public static readonly string[] REWARD_USES = new string[] {"Before Shot", "During Shot",
                                                                "Before Shot", "During Shot", "Passive"};
    public static readonly string[] REWARD_DESCS = new string[] {"A new way to hit the ball!\n\n" +
                                                        /*0*/    "Press the up arrow to swap between putting and chip shotting.",
                                                        /*1*/    "Press space to have your ball hop into the air!\n\n" +
                                                                 "You can jump in midair.\n\n" +
                                                                 "You will have 2 jumps per hole.\n\n",
                                                        /*2*/    "Turn your ball into a fireball once per hole!\n\n" +
                                                                 "Fireballs last for 2 seconds, don't lose speed, and deal XX% " +
                                                                 "more damage.\n\n",
                                                        /*3*/    "You can become a spikeball during your shot!\n\n" +
                                                                 "While spiky, your ball does XX% more damage, " +
                                                                 "but loses speed much more quickly.\n\n",
                                                        /*4*/    "Your ball does XX% more damage to enemies."};
    public static readonly Sprite[] REWARD_IMAGES = new Sprite[] { /* Use Resources.Load([FilePath])*/ };

    public static readonly int[] LEVEL_EXP_THRESHOLDS = new int[] { 0, 100, 150, 200, 250, 100000 };

    private static List<PlayerInfo> players = new List<PlayerInfo>();

    private bool[] hasAbilities;
    private int totalExp, levelExp, level;

    private float damageBonus;

    public PlayerInfo()
    {
        hasAbilities = new bool[11] { false, false, false, false, false, false, false, false, false, false, false };
        totalExp = 0;
        levelExp = 0;
        level = 1;
        damageBonus = 1;

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

    public float GetDamageBonus()
    {
        return damageBonus;
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
    }

    public void GainAbility(int abilityCode)
    {
        if (abilityCode == 4)
        {
            //damageBonus *=
            return;
        }
        hasAbilities[abilityCode] = true;
    }
}
