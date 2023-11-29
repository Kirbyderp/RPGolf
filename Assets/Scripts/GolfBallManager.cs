using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GolfBallManager : MonoBehaviour
{
    //Leveling system vars
    private PlayerInfo player;
    private GameObject expBar;
    private TMPro.TextMeshProUGUI expText;
    private int playerLevel = 1;
    private bool waitingForLevelUp = false;
    private GameObject levelUpUIBackground;
    private LevelUpRewardUI[] levelUpRewardUIs;
    private bool inLevelUpScreen = false, waitingForLevelUpAnim = false;
    private int levelUpAnimCount = 0;

    //Ability use vars
    private int nextAbilityKey = 1;
    private KeyCode[] abilityKeys = new KeyCode[12];
    private int timesJumped = 0;
    private bool usedSpikeBall = false;
    private bool usingFireball = false, usedFireball = false;
    private bool usingGlide = false, usedGlide = false;
    private bool usedDoOver = false;
    private bool usedExplosion = false;
    private float fireballTimer = 0, fireballDuration = 2;
    private int shieldsUsed = 0;
    private GameObject leftCurveLoc, rightCurveLoc;


    //Hitting the ball vars
    private GameObject camRotator;
    private Vector3 mouseStarterPos, mousePos;
    private bool planningShot = false, mouseOverLevelUp = false;
    private GameObject powerBar, triRotator, tri, triRotatorCopy;
    private float barPercentage = 0;
    private int curShotType = 0; //0 = putt, 1 = chip, 2 = curve left, 3 = curve right
    private SirPuttAnimController sirPuttAnim;
    private bool ballInAnim = false;
    private Vector3 lastHitPos, curHitPos;
    private GameObject[] allEnemies;

    //Ball physics vars
    private Rigidbody golfBallRb;
    private float power = 50, bounciness = .8f, friction = 2.2f;
    private Vector3 curVel = new Vector3(0, 0, 0), lastFrameVel = new Vector3(0, 0, 0);
    private bool applyFriction = false;
    private bool ballHasStopped = true, waitingForStopCheck = false;
    private float ballShotTime = 0, frictionTimeMod = 1;
    private Vector3 defaultGrav = new Vector3(0, -9.81f, 0);

    //Curse vars
    private bool hasMegaBounce = false;
    private int airBallTurns = 0, tinyTurns = 0, icyTurns = 0;

    //Other
    bool lastShotOB = false;
    int numStrokes = 0;

    //KeyCodes
    KeyCode chipShotKey = KeyCode.W;
    KeyCode curveShotKey = KeyCode.S;
    KeyCode jumpKey = KeyCode.Space;

    //Debug Vars
    public bool isDebugMode = false;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerInfo.GetPlayers().Count == 0)
        {
            player = new PlayerInfo();
        }
        else
        {
            player = PlayerInfo.GetPlayers()[0];
        }
        camRotator = GameObject.Find("Camera Rotator");
        powerBar = GameObject.Find("Power Bar");
        expBar = GameObject.Find("Exp Bar");
        expBar.GetComponent<Image>().color = Color.green;
        expText = GameObject.Find("Exp Text").GetComponent<TMPro.TextMeshProUGUI>();
        UpdateExpBar();
        levelUpUIBackground = GameObject.Find("Level Up Background");
        levelUpRewardUIs = new LevelUpRewardUI[5];
        levelUpRewardUIs[0] = GameObject.Find("Level Up 21").GetComponent<LevelUpRewardUI>();
        levelUpRewardUIs[1] = GameObject.Find("Level Up 22").GetComponent<LevelUpRewardUI>();
        levelUpRewardUIs[2] = GameObject.Find("Level Up 31").GetComponent<LevelUpRewardUI>();
        levelUpRewardUIs[3] = GameObject.Find("Level Up 32").GetComponent<LevelUpRewardUI>();
        levelUpRewardUIs[4] = GameObject.Find("Level Up 33").GetComponent<LevelUpRewardUI>();

        powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
        powerBar.GetComponent<Image>().color = GetBarColor();
        triRotator = GameObject.Find("Tri Rotator");
        tri = GameObject.Find("Tri");
        triRotatorCopy = GameObject.Find("Tri Rotator Copy");
        tri.SetActive(false);
        golfBallRb = GetComponent<Rigidbody>();

        sirPuttAnim = GameObject.Find("Sir Puttsalot").GetComponent<SirPuttAnimController>();

        leftCurveLoc = GameObject.Find("Left Curve Loc");
        rightCurveLoc = GameObject.Find("Right Curve Loc");

        curHitPos = transform.position;
        Debug.Log(GameObject.Find("Main Camera").transform.position);
    }

    // Update is called once per frame
    void Update()
    {        
        //Keeps track of the golf ball's velocity for collisions
        lastFrameVel = curVel;
        curVel = golfBallRb.velocity;

        if (!ballHasStopped && !ballInAnim)
        {
            ballShotTime += Time.deltaTime;
            if (ballShotTime >= 10)
            {
                frictionTimeMod = Mathf.Pow(2, ballShotTime / 5 - 1);
            }
        }

        //Friction
        if (applyFriction && !ballInAnim)
        {
            if (golfBallRb.velocity.magnitude > friction * (usedSpikeBall ? 2 : 1) * (usingFireball ? 0 : 1) *
                                                (icyTurns > 0 ? .5f : 1) * (airBallTurns > 0 ? .5f : 1) *
                                                frictionTimeMod * Time.deltaTime)
            {
                golfBallRb.velocity -= golfBallRb.velocity.normalized * friction * (usedSpikeBall ? 2 : 1)
                                       * (usingFireball ? 0 : 1) * Time.deltaTime;
            }
            else if (!ballHasStopped && !waitingForStopCheck)
            {
                golfBallRb.velocity = Vector3.zero;
                StartCoroutine(BallStopCheck());
                waitingForStopCheck = true;
            }
        }

        //Can't fire a shot if the golf ball is moving, if player is leveling up, or if in animation
        if ((ballHasStopped || player.HasAbility(11)) && !inLevelUpScreen && !ballInAnim)
        {
            if (usedSpikeBall && ballHasStopped)
            {
                Debug.Log("Deactivated Spike Ball");
                usedSpikeBall = false;
            }

            //Allow changing shot type if player has ability(s)
            if (player.HasAbility(0) && Input.GetKeyDown(chipShotKey) && ballHasStopped)
            {
                if (curShotType == 1)
                {
                    curShotType = 0;
                    Debug.Log("Current Shot Type: Putt");
                }
                else
                {
                    curShotType = 1;
                    Debug.Log("Current Shot Type: Chip");
                }
            }

            if (player.HasAbility(7) && Input.GetKeyDown(curveShotKey) && ballHasStopped)
            {
                if (curShotType == 2)
                {
                    curShotType = 3;
                    Debug.Log("Current Shot Type: Curve Right");
                }
                else if (curShotType == 3)
                {
                    curShotType = 0;
                    Debug.Log("Current Shot Type: Putt");
                }
                else
                {
                    curShotType = 2;
                    Debug.Log("Current Shot Type: Curve Left");
                }
            }

            //Allow fireball activation if player has ability
            if (player.HasAbility(2) && Input.GetKeyDown(abilityKeys[2]) && !usedFireball && ballHasStopped)
            {
                if (usingFireball)
                {
                    Debug.Log("Cancelled Fireball");
                    usingFireball = false;
                }
                else
                {
                    Debug.Log("Using Fireball");
                    usingFireball = true;
                }
            }

            //Allow glide activation if player has ability
            if (player.HasAbility(6) && Input.GetKeyDown(abilityKeys[6]) && !usedGlide && ballHasStopped)
            {
                if (usingGlide)
                {
                    Debug.Log("Cancelled Glide");
                    usingGlide = false;
                }
                else
                {
                    Debug.Log("Using Glide");
                    usingGlide = true;
                }
            }

            //Allow do over if player has ability
            if (player.HasAbility(9) && Input.GetKeyDown(abilityKeys[9]) && !usedDoOver && ballHasStopped)
            {
                numStrokes--;
                if (lastShotOB)
                {
                    numStrokes--;
                }
                ResetBallPosition(lastHitPos);
                triRotatorCopy.transform.position = triRotator.transform.position;
                usedDoOver = true;
            }

            //If the player releases lmb, fire shot
            if (Input.GetMouseButtonUp(0) && planningShot)
            {
                planningShot = false;
                lastHitPos = curHitPos;
                if (ballHasStopped)
                {
                    ballHasStopped = false;
                    ballInAnim = true;
                    sirPuttAnim.Swing();
                    StartCoroutine(WaitForBallSwing());
                }
                else
                {
                    golfBallRb.velocity = Vector3.zero;
                    Swing();
                }
            }

            if (planningShot)
            {
                mousePos = Input.mousePosition;
                transform.rotation = camRotator.transform.rotation;
                Vector3 mouseDif = new Vector3(mousePos.x - mouseStarterPos.x, 0, mousePos.y - mouseStarterPos.y);
                if (mouseDif.magnitude == 0)
                {
                    return;
                }
                tri.SetActive(true);
                Vector3 triDif = tri.transform.position - triRotator.transform.position;
                triRotator.transform.rotation *= Quaternion.FromToRotation(triDif, -mouseDif);
                triRotator.transform.rotation *= transform.rotation;
                if (triRotator.transform.rotation.eulerAngles.x != 0 || triRotator.transform.rotation.eulerAngles.z != 0)
                {
                    triRotator.transform.rotation = Quaternion.Euler(0, triRotator.transform.rotation.eulerAngles.y, 0);
                }
                if (ballHasStopped)
                {
                    triRotatorCopy.transform.rotation = triRotator.transform.rotation;
                }
                if (mouseDif.magnitude < 250)
                {
                    powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, mouseDif.magnitude * 700 / 250);
                    barPercentage = mouseDif.magnitude / 250;
                }
                else
                {
                    powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 700);
                    barPercentage = 1;
                }
                powerBar.GetComponent<Image>().color = GetBarColor();
            }

            //If the player clicks down lmb, begin planning shot
            if (Input.GetMouseButtonDown(0) && !mouseOverLevelUp)
            {
                planningShot = true;
                mouseStarterPos = Input.mousePosition;
                transform.rotation = camRotator.transform.rotation; 
            }
        }
        else if (golfBallRb.velocity.magnitude > 0) //During Shot Ability Activations
        {
            //Jump
            if (player.HasAbility(1) && Input.GetKeyDown(jumpKey) && timesJumped < player.GetJumpsPerLevel())
            {
                Debug.Log("Jumped");
                golfBallRb.AddForce(player.GetJumpStrength() * Vector3.up, ForceMode.Impulse);
                timesJumped++;
                if (timesJumped >= player.GetJumpsPerLevel())
                {
                    Debug.Log("Now out of jumps");
                }
            }

            //Fireball
            if (usingFireball)
            {
                fireballTimer += Time.deltaTime;
                if (fireballTimer >= fireballDuration)
                {
                    Debug.Log("Fireball Duration Over");
                    usingFireball = false;
                    usedFireball = true;
                }
            }

            //Spike Ball
            if (player.HasAbility(3) && Input.GetKeyDown(abilityKeys[3]) && !usedSpikeBall)
            {
                Debug.Log("Activated Spike Ball");
                usedSpikeBall = true;
            }

            //Gliding
            if (usingGlide)
            {
                if (golfBallRb.velocity.y < -1)
                {
                    Physics.gravity = Vector3.zero;
                    golfBallRb.velocity = new Vector3(golfBallRb.velocity.x, -1, golfBallRb.velocity.z);
                }
                else if (golfBallRb.velocity.y > -.9f)
                {
                    Physics.gravity = defaultGrav / (airBallTurns > 0 ? 2 : 1);
                }
            }

            //Ball Curving
            if (curShotType == 2 && !ballHasStopped)
            {
                Debug.Log("Curving Left");
                Debug.Log(golfBallRb.velocity);
                golfBallRb.velocity = Vector3.RotateTowards(golfBallRb.velocity,
                                                            leftCurveLoc.transform.position - transform.position,
                                                            Mathf.PI / 4 * Time.deltaTime, 0);
                Debug.Log(golfBallRb.velocity);
            }
            else if (curShotType == 3 && !ballHasStopped)
            {
                Debug.Log("Curving Right");
                Debug.Log(golfBallRb.velocity);
                golfBallRb.velocity = Vector3.RotateTowards(golfBallRb.velocity,
                                                            rightCurveLoc.transform.position - transform.position,
                                                            Mathf.PI / 4 * Time.deltaTime, 0);
                Debug.Log(golfBallRb.velocity);
            }

            //Explosion
            if (player.HasAbility(10) && Input.GetKeyDown(abilityKeys[10]) && !usedExplosion)
            {
                Debug.Log("Exploded");
                allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in allEnemies)
                {
                    if ((enemy.transform.position - transform.position).magnitude < 1.5f / (tinyTurns > 0 ? 2 : 1))
                    {
                        if (enemy.GetComponent<Enemy>().TakeDamage(125 * player.GetDamageBonus() * (tinyTurns > 0 ? .5f : 1)))
                        {
                            if (player.GainEXP(enemy.GetComponent<Enemy>().expReward))
                            {
                                waitingForLevelUp = true;
                                LevelUpNotif();
                            }
                            else if (!waitingForLevelUp)
                            {
                                UpdateExpBar();
                            }
                            Destroy(enemy);
                        }
                    }
                }
            }
        }
    }

    private Color GetBarColor()
    {
        return Color.red * Mathf.Sqrt(barPercentage) + Color.green * Mathf.Sqrt(1 - barPercentage);
    }

    public void MouseIsOverLevelUp()
    {
        mouseOverLevelUp = true;
    }

    public void MouseNotOverLevelUp()
    {
        mouseOverLevelUp = false;
    }

    public void OutOfBounds()
    {
        lastShotOB = true;
        numStrokes++;
        airBallTurns--;
        if (airBallTurns <= 0)
        {
            Physics.gravity = defaultGrav;
        }
        tinyTurns--;
        icyTurns--;
        ResetBallPosition(lastHitPos);
    }

    public void ResetBallPosition(Vector3 posIn)
    {
        transform.position = posIn;
        golfBallRb.velocity = Vector3.zero;
        golfBallRb.constraints = RigidbodyConstraints.FreezeAll;
        ballHasStopped = true;
        waitingForStopCheck = false;
        ballShotTime = 0;
        curHitPos = transform.position;
        if (usingGlide)
        {
            usedGlide = true;
            usingGlide = false;
            Physics.gravity = defaultGrav / (airBallTurns > 0 ? 2 : 1);
        }
    }

    IEnumerator WaitForBallSwing()
    {
        yield return new WaitForSeconds(55 / 60f);
        Swing();
        ballInAnim = false;
    }
    
    private void Swing()
    {
        golfBallRb.constraints = RigidbodyConstraints.None;
        if (curShotType == 1)
        {
            golfBallRb.AddForce(power * barPercentage * (power * (tri.transform.position -
                                triRotator.transform.position).normalized + new Vector3(0, 7, 0)).normalized,
                                ForceMode.Impulse);
        }
        else
        {
            golfBallRb.AddForce(power * barPercentage * (tri.transform.position - triRotator.transform.position).normalized,
                                ForceMode.Impulse);
        }

        tri.SetActive(false);
        powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
        barPercentage = 0;
        powerBar.GetComponent<Image>().color = GetBarColor();

        numStrokes++;
        lastShotOB = false;

        if (isDebugMode)
        {
            Debug.Log(isDebugMode);
            if (player.GainEXP(50))
            {
                waitingForLevelUp = true;
                LevelUpNotif();
            }
            else if (!waitingForLevelUp)
            {
                UpdateExpBar();
            }
        }
    }

    public void FreeFireball()
    {
        fireballTimer = 0;
        if (!usingFireball)
        {
            usingFireball = true;
        }
    }

    public void GainMegaBounce()
    {
        hasMegaBounce = true;
    }

    public void GainAirBall()
    {
        if (airBallTurns <= 0)
        {
            Physics.gravity /= 2;
        }
        airBallTurns = 2;
    }

    public void GainTinyBall()
    {
        if (tinyTurns <= 0)
        {
            transform.localScale = new Vector3(.25f, .25f, .25f);
        }
        tinyTurns = 2;
    }

    public void GainIcePhysics()
    {
        icyTurns = 2;
    }

    IEnumerator BallStopCheck()
    {
        yield return new WaitForSeconds(1);
        if (golfBallRb.velocity.magnitude < friction * (usedSpikeBall ? 2 : 1) *
                                            (icyTurns > 0 ? .5f : 1) * (airBallTurns > 0 ? .5f : 1) * 
                                            frictionTimeMod * Time.deltaTime)
        {
            ballInAnim = true;
            golfBallRb.velocity = Vector3.zero;
            golfBallRb.constraints = RigidbodyConstraints.FreezeAll;
            ballHasStopped = true;
            waitingForStopCheck = false;
            ballShotTime = 0;
            curHitPos = transform.position;
            if (usingGlide)
            {
                usedGlide = true;
                usingGlide = false;
                Physics.gravity = defaultGrav / (airBallTurns > 0 ? 2 : 1);
            }
            airBallTurns--;
            if (airBallTurns == 0)
            {
                Physics.gravity = defaultGrav;
            }
            tinyTurns--;
            if (tinyTurns == 0)
            {
                transform.localScale = new Vector3(.5f, .5f, .5f);
            }
            icyTurns--;
            if (curHitPos == lastHitPos)
            {
                ballInAnim = false;
            }
            else
            {
                sirPuttAnim.Jump();
            }
        }
        else
        {
            waitingForStopCheck = false;
        }
    }

    public void SetBallInAnim(bool input)
    {
        ballInAnim = input;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Ball is touching an object; apply friction
        applyFriction = true;

        //If the ball collides with a bouncy surface, bounce off of it
        if (collision.gameObject.CompareTag("Bouncy"))
        {
            Physics.Raycast(transform.position, collision.GetContact(0).point - transform.position, out RaycastHit hitInfo);
            golfBallRb.velocity = lastFrameVel;
            golfBallRb.velocity -= 2 * Vector3.Dot(golfBallRb.velocity, hitInfo.normal) * hitInfo.normal;
            golfBallRb.velocity *= usingFireball ? 1 : bounciness;
            if (hasMegaBounce)
            {
                golfBallRb.velocity *= 4;
                hasMegaBounce = false;
            }
            curVel = golfBallRb.velocity;
        }
        
        //If the ball collides with an enemy...
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //...and kills it, gain the exp reward, destroy the enemy, and continue onwards
            if (collision.gameObject.GetComponent<Enemy>().TakeDamage(lastFrameVel.magnitude * player.GetDamageBonus() *
                (usedSpikeBall ? 4 : 1) * (usingFireball ? 4 : 1) * (tinyTurns > 0 ? .5f : 1)))
            {
                if (player.GainEXP(collision.gameObject.GetComponent<Enemy>().expReward))
                {
                    waitingForLevelUp = true;
                    LevelUpNotif();
                }
                else if (!waitingForLevelUp)
                {
                    UpdateExpBar();
                }
                Destroy(collision.gameObject);
                golfBallRb.velocity = lastFrameVel;
                curVel = golfBallRb.velocity;
            }
            //...and doesn't kill it, bounce off the enemy
            else
            {
                Physics.Raycast(transform.position, collision.transform.position - transform.position, out RaycastHit hitInfo);
                golfBallRb.velocity = lastFrameVel;
                golfBallRb.velocity -= 2 * Vector3.Dot(golfBallRb.velocity, hitInfo.normal) * hitInfo.normal;
                golfBallRb.velocity *= collision.gameObject.GetComponent<Enemy>().bounciness;
                if (hasMegaBounce)
                {
                    golfBallRb.velocity *= 4;
                    hasMegaBounce = false;
                }
                curVel = golfBallRb.velocity;
            }
        }

        //If the ball collides with a damaging object...
        if (collision.gameObject.CompareTag("Damage"))
        {
            // ...if they have a shield to use, bounce off object
            if (shieldsUsed < player.GetShieldsPerLevel())
            {
                shieldsUsed++;
                Physics.Raycast(transform.position, collision.GetContact(0).point - transform.position, out RaycastHit hitInfo);
                golfBallRb.velocity = lastFrameVel;
                golfBallRb.velocity -= 2 * Vector3.Dot(golfBallRb.velocity, hitInfo.normal) * hitInfo.normal;
                golfBallRb.velocity *= usingFireball ? 1 : bounciness;
                if (hasMegaBounce)
                {
                    golfBallRb.velocity *= 4;
                    hasMegaBounce = false;
                }
                curVel = golfBallRb.velocity;
            }
            else
            { // ...reset its position to the last hit position if they have no shield.
                ResetBallPosition(lastHitPos);
            }
        }    
    }

    private void OnCollisionStay(Collision collision)
    {
        /* Ensures that, when the ball is touching multiple objects and
         * stops touching only one, friction is still being applied.
         */
        applyFriction = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        //Ball is no longer touching object, stop applying friction
        applyFriction = false;
    }

    public void UpdateExpBar()
    {
        expBar.GetComponent<RectTransform>().sizeDelta = new Vector2(400f * player.GetLevelExp() /
                                                PlayerInfo.LEVEL_EXP_THRESHOLDS[player.GetLevel()], 120);
        expText.text = player.GetLevelExp() + " / " + PlayerInfo.LEVEL_EXP_THRESHOLDS[player.GetLevel()];
    }

    public void LevelUpNotif()
    {
        expBar.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 120);
        expText.text = "Click Here to Level Up!";
    }

    public void LevelUpBonus()
    {
        if (waitingForLevelUp && !inLevelUpScreen && golfBallRb.velocity == new Vector3(0, 0, 0))
        {
            inLevelUpScreen = true;
            waitingForLevelUpAnim = true;
            playerLevel++;
            switch (playerLevel)
            {
                case 2:
                    levelUpRewardUIs[0].gameObject.SetActive(true);
                    levelUpRewardUIs[0].SetLevelUpUI(0);
                    levelUpRewardUIs[1].gameObject.SetActive(true);
                    levelUpRewardUIs[1].SetLevelUpUI(1);
                    levelUpRewardUIs[2].gameObject.SetActive(false);
                    levelUpRewardUIs[3].gameObject.SetActive(false);
                    levelUpRewardUIs[4].gameObject.SetActive(false);
                    break;
                case 3:
                    levelUpRewardUIs[0].gameObject.SetActive(false);
                    levelUpRewardUIs[1].gameObject.SetActive(false);
                    levelUpRewardUIs[2].gameObject.SetActive(true);
                    levelUpRewardUIs[2].SetLevelUpUI(2);
                    levelUpRewardUIs[2].AddDesc("Press " + nextAbilityKey + " to activate.");
                    levelUpRewardUIs[3].gameObject.SetActive(true);
                    levelUpRewardUIs[3].SetLevelUpUI(3);
                    levelUpRewardUIs[3].AddDesc("Press " + nextAbilityKey + " to activate.");
                    levelUpRewardUIs[4].gameObject.SetActive(true);
                    levelUpRewardUIs[4].SetLevelUpUI(4);
                    break;
                case 4:
                    levelUpRewardUIs[0].gameObject.SetActive(false);
                    levelUpRewardUIs[1].gameObject.SetActive(false);
                    levelUpRewardUIs[2].gameObject.SetActive(true);
                    levelUpRewardUIs[2].SetLevelUpUI(5);
                    levelUpRewardUIs[2].AddDesc("You will have " + (player.GetShieldsPerLevel() + 1) + " shield uses per hole.");
                    levelUpRewardUIs[3].gameObject.SetActive(true);
                    levelUpRewardUIs[3].SetLevelUpUI(6);
                    levelUpRewardUIs[3].AddDesc("Press " + nextAbilityKey + " to activate.");
                    levelUpRewardUIs[4].gameObject.SetActive(true);
                    if (player.HasAbility(0))
                    {
                        levelUpRewardUIs[4].SetLevelUpUI(7);
                    }
                    else
                    {
                        levelUpRewardUIs[4].SetLevelUpUI(8);
                    }
                    break;
                case 5:
                    levelUpRewardUIs[0].gameObject.SetActive(false);
                    levelUpRewardUIs[1].gameObject.SetActive(false);
                    levelUpRewardUIs[2].gameObject.SetActive(true);
                    levelUpRewardUIs[2].SetLevelUpUI(9);
                    levelUpRewardUIs[2].AddDesc("Press " + nextAbilityKey + " to activate.");
                    levelUpRewardUIs[3].gameObject.SetActive(true);
                    levelUpRewardUIs[3].SetLevelUpUI(10);
                    levelUpRewardUIs[3].AddDesc("Press " + nextAbilityKey + " to activate.");
                    levelUpRewardUIs[4].gameObject.SetActive(true);
                    levelUpRewardUIs[4].SetLevelUpUI(11);
                    break;
                default:
                    levelUpRewardUIs[0].gameObject.SetActive(false);
                    levelUpRewardUIs[1].gameObject.SetActive(false);
                    levelUpRewardUIs[2].gameObject.SetActive(true);
                    levelUpRewardUIs[3].gameObject.SetActive(true);
                    levelUpRewardUIs[4].gameObject.SetActive(true);
                    break;
            }
            InvokeRepeating("LevelUpUIEnterAnim", 0, 1 / 60f);
            if (playerLevel == player.GetLevel())
            {
                waitingForLevelUp = false;
                UpdateExpBar();
            }
        }
    }

    public bool IsWaitingForLevelUpAnim()
    {
        return waitingForLevelUpAnim;
    }

    private void LevelUpUIEnterAnim()
    {
        levelUpAnimCount++;
        levelUpUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(0, 1920 * (1 - Mathf.Sin(Mathf.PI
                                                                                      * levelUpAnimCount / 120)), 0);
        if (levelUpAnimCount == 60)
        {
            CancelInvoke();
            levelUpUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            waitingForLevelUpAnim = false;
        }
    }

    public void SelectReward(int abilityCode)
    {
        waitingForLevelUpAnim = true;
        player.GainAbility(abilityCode);
        if (PlayerInfo.IS_GENERIC_KEY[abilityCode])
        {
            abilityKeys[abilityCode] = GetNextKey();
        }
        InvokeRepeating("LevelUpUIExitAnim", 0, 1 / 60f);
    }

    private KeyCode GetNextKey()
    {
        switch (nextAbilityKey)
        {
            case 1:
                return KeyCode.Alpha1;
            case 2:
                return KeyCode.Alpha2;
            case 3:
                return KeyCode.Alpha3;
            case 4:
                return KeyCode.Alpha4;
            case 5:
                return KeyCode.Alpha5;
            case 6:
                return KeyCode.Alpha6;
            case 7:
                return KeyCode.Alpha7;
            case 8:
                return KeyCode.Alpha8;
            case 9:
                return KeyCode.Alpha9;
            case 10:
                return KeyCode.Alpha0;
        }
        throw new System.Exception("Too many generic-key triggerable abilities assigned to player");
    }

    private void LevelUpUIExitAnim()
    {
        levelUpAnimCount--;
        levelUpUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(0, 1920 * (1 - Mathf.Sin(Mathf.PI
                                                                                      * levelUpAnimCount / 120)), 0);
        if (levelUpAnimCount == 0)
        {
            CancelInvoke();
            levelUpUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(0, 1920, 0);
            waitingForLevelUpAnim = false;
            inLevelUpScreen = false;
        }
    }
}