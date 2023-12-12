using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHP;
    public int expReward;
    public float bounciness;
    public bool isGolem;

    private int curHP;

    
    // Start is called before the first frame update
    void Start()
    {
        curHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Returns true if the damage taken is enough to kill the enemy, false otherwise
    public bool TakeDamage(float velIn)
    {
        //Debug.Log(velIn);
        if (isGolem && velIn > 30 && curHP > maxHP * .3f)
        {
            curHP = (int)(curHP + 30 - velIn);
        }
        else if (!isGolem || curHP < maxHP * .3f)
        {
            curHP = (int)(curHP - velIn);
        }
        
        
        if (curHP <= 0)
        {
            return true;
        }
        return false;
    }

}
