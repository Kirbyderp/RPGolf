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
     * 5 = Shield
     * 6 = Glide
     * 7 = Curve Shot
     * 8 = Jump+
     * 9 = Do Over
     * 10 = Explosion
     * 11 = No Patience
     */

    public static readonly bool[] IS_GENERIC_KEY = new bool[] { false, false,
                                                                true, true, false,
                                                                false, true, false, false,
                                                                true, true, false };
    public static readonly string[] REWARD_NAMES = new string[] {"Chip Shot", "Jump",
                                                                 "Fireball", "Spike Ball", "Damage Up",
                                                                 "Shield", "Glide", "Spin Ball", "Jump+",
                                                                 "Do Over", "Explosion", "No Patience"};
    public static readonly string[] REWARD_USES = new string[] {"Before Shot", "During Shot",
                                                                "Before Shot", "During Shot", "Passive",
                                                                "Passive", "Before Shot", "Before Shot", "Passive",
                                                                "Before Shot", "During Shot", "Passive"};
    public static readonly string[] REWARD_DESCS = new string[] {"A new way to hit the ball!\n\n" +
                                                        /*0*/    "Press W to swap between putting and chip shotting.",
                                                        /*1*/    "Press space to have your ball hop into the air!\n\n" +
                                                                 "You can jump in midair.\n\n" +
                                                                 "You will have 2 jumps per hole.\n\n",
                                                        /*2*/    "Turn your ball into a fireball once per hole!\n\n" +
                                                                 "Fireballs last for 2 seconds, don't lose speed, and deal 4 times " +
                                                                 "more damage.\n\n",
                                                        /*3*/    "You can become a spikeball during your shot!\n\n" +
                                                                 "While spiky, your ball does 4 times more damage, " +
                                                                 "but loses speed much more quickly.\n\n",
                                                        /*4*/    "Your ball permanently does 50% more damage to enemies.",
                                                        /*5*/    "The shield prevents deadly objects from hurting your ball.\n\n" +
                                                                 "The shield will not save you if you go out of bounds.\n\n",
                                                        /*6*/    "Once per hole, make your ball able to glide!\n\n" +
                                                                 "During a glide shot, the ball will descend much more " +
                                                                 "slowly if it is in the air.\n\n",
                                                        /*7*/    "A new way to hit the ball!\n\n" +
                                                                 "Press S to apply a spin to your ball, making it curve " +
                                                                 "either left or right.",
                                                        /*8*/    "Your jumps become stronger, and you get 2 more jumps every hole.",
                                                        /*9*/    "Once per hole, undo the last shot you took!\n\n" +
                                                                 "You will not get ability uses back.\n\n" +
                                                                 "Enemies hit and experience gained will not be reset.\n\n",
                                                        /*10*/   "Once per hole, create an explosion, damaging all nearby " +
                                                                 "enemies!\n\n",
                                                        /*11*/   "You no longer have to wait for your ball to stop before " +
                                                                 "making another shot.\n\n" +
                                                                 "You must still wait for your ball to stop to activate " +
                                                                 "before shot abilities.",};
    public static readonly Sprite[] REWARD_IMAGES = new Sprite[] { /* Use Resources.Load([FilePath])*/ };

    public static readonly int[] LEVEL_EXP_THRESHOLDS = new int[] { 0, 50, 100, 200, 400, 500, 500, 500, 500, 500, 500, 500, 500,
                                                                    500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 };

    private static List<PlayerInfo> players = new List<PlayerInfo>();

    private bool[] hasAbilities;
    private int totalExp, levelExp, level, golfBallLevel;

    private int jumpsPerLevel, shieldsPerLevel;
    private float damageBonus, jumpStrength;

    private bool waitingForLevelUp;
    private List<int> strokesPerHole;

    public PlayerInfo()
    {
        hasAbilities = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        totalExp = 0;
        levelExp = 0;
        level = 1;
        golfBallLevel = 1;
        damageBonus = 1;
        jumpsPerLevel = 2;
        jumpStrength = 7;
        shieldsPerLevel = 0;
        waitingForLevelUp = false;
        strokesPerHole = new List<int>();

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

    public int GetGolfBallLevel()
    {
        return golfBallLevel;
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

    public float GetJumpStrength()
    {
        return jumpStrength;
    }

    public int GetJumpsPerLevel()
    {
        return jumpsPerLevel;
    }

    public int GetShieldsPerLevel()
    {
        return shieldsPerLevel;
    }

    public bool HasAbility(int index)
    {
        return hasAbilities[index];
    }

    public bool WaitingForLevelUp()
    {
        return waitingForLevelUp;
    }

    public void SetWaitingForLevelUp(bool isWaiting)
    {
        waitingForLevelUp = isWaiting;
    }

    //Returns true if the exp gained caused the player to level up, false otherwise
    public bool GainEXP(int expIn)
    {
        totalExp += expIn;
        levelExp += expIn;
        if (levelExp >= LEVEL_EXP_THRESHOLDS[level])
        {
            while (levelExp >= LEVEL_EXP_THRESHOLDS[level])
            {
                Debug.Log(levelExp);
                Debug.Log(level);
                LevelUp();
            }
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
            damageBonus *= 1.5f;
            return;
        }
        if (abilityCode == 5)
        {
            shieldsPerLevel++;
            return;
        }
        if (abilityCode == 8)
        {
            jumpsPerLevel += 2;
            jumpStrength += 2;
            return;
        }
        hasAbilities[abilityCode] = true;
    }

    public void AddGolfBallLevel()
    {
        golfBallLevel++;
    }

    public void RecordStrokesForHole(int numStrokes)
    {
        strokesPerHole.Add(numStrokes);
    }
}
