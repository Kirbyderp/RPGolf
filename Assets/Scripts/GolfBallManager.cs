using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GolfBallManager : MonoBehaviour
{
    //Leveling system vars
    private PlayerInfo player;
    private GameObject expBar;
    private TMPro.TextMeshProUGUI expText;
    private int playerLevel;
    private GameObject levelUpUIBackground;
    private LevelUpRewardUI[] levelUpRewardUIs;
    private bool inLevelUpScreen = false, waitingForLevelUpAnim = false;
    private int levelUpAnimCount = 0;

    //Ability use vars
    private int nextAbilityKey = 1;
    private KeyCode[] abilityKeys = new KeyCode[12];
    private int timesJumped = 0;
    private GameObject jumpNumUI;
    private TMPro.TextMeshProUGUI jumpNumText;
    private bool usedSpikeBall = false, inSpikeBallEndAnim = false;
    private GameObject spikesModel;
    private bool usingFireball = false, usedFireball = false;
    private bool usingGlide = false, usedGlide = false;
    private bool usedDoOver = false;
    private bool usedExplosion = false;
    private float fireballTimer = 0, fireballDuration = 2;
    private int shieldsUsed = 0;
    private GameObject shieldVisual;
    private GameObject leftCurveLoc, rightCurveLoc;
    private GameObject puttIcon, chipIcon, curveIcon;

    //Hitting the ball vars
    private GameObject camRotator;
    private Vector3 mouseStarterPos, mousePos;
    private bool planningShot = false, mouseOverLevelUp = false;
    private GameObject powerBar, triRotator, tri, triRotatorCopy, hitDir2D, hitDir2DUI;
    private float barPercentage = 0;
    private int curShotType = 0; //0 = putt, 1 = chip, 2 = curve left, 3 = curve right
    private SirPuttAnimController sirPuttAnim;
    private bool ballInAnim = false;
    private Vector3 lastHitPos, curHitPos;
    private GameObject[] allEnemies;
    private bool[] canRotateAtDegree = new bool[12];
    private GameObject sirPuttsAlot;
    private bool inBackupPosition = false;

    //Ball physics vars
    private Rigidbody golfBallRb;
    private float power = 50, bounciness = .8f, friction = 2.2f;
    private Vector3 friction1Over30;
    private Vector3 curVel = new Vector3(0, 0, 0), lastFrameVel = new Vector3(0, 0, 0);
    private bool applyFriction = false;
    private bool ballHasStopped = true, waitingForStopCheck = false;
    private float ballShotTime = 0, frictionTimeMod = 1;
    private Vector3 defaultGrav = new Vector3(0, -9.81f, 0);
    private bool hasJustSwung = false;
    private bool waitingForDamageAnim = false;

    //Curse vars
    private bool hasMegaBounce = false;
    private int airBallTurns = 0, tinyTurns = 0, icyTurns = 0;

    //Other
    private bool lastShotOB = false;
    private int numStrokes = 0;
    private int holeEndAnimCount = 0;
    private bool inEndScreen = false, waitingOnEndAnim = false;
    private GameObject holeEndUIBackground;
    private string sceneToGoTo;
    private TMPro.TextMeshProUGUI ehStrokeText, ehParText, ehRewardText;

    //KeyCodes
    KeyCode chipShotKey = KeyCode.W;
    KeyCode curveShotKey = KeyCode.S;
    KeyCode jumpKey = KeyCode.Space;

    //Debug Vars
    public bool isDebugMode;
    //public GameObject castEndObj, test;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerInfo.GetPlayers().Count == 0)
        {
            player = new PlayerInfo();
            playerLevel = 1;
        }
        else
        {
            player = PlayerInfo.GetPlayers()[0];
            playerLevel = player.GetGolfBallLevel();
        }
        camRotator = GameObject.Find("Camera Rotator");
        powerBar = GameObject.Find("Power Bar");
        expBar = GameObject.Find("Exp Bar");
        expBar.GetComponent<Image>().color = Color.green;
        expText = GameObject.Find("Exp Text").GetComponent<TMPro.TextMeshProUGUI>();
        if (player.WaitingForLevelUp())
        {
            LevelUpNotif();
        }
        else
        {
            UpdateExpBar();
        }
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
        hitDir2D = GameObject.Find("2D Hit Dir Rotator");
        hitDir2DUI = GameObject.Find("Ball Hit Direction UI");
        hitDir2DUI.SetActive(false);
        tri.SetActive(false);
        golfBallRb = GetComponent<Rigidbody>();
        shieldVisual = GameObject.Find("Golf Ball Shield Buff");
        if (shieldsUsed >= player.GetShieldsPerLevel())
        {
            shieldVisual.SetActive(false);
        }
        jumpNumText = GameObject.Find("Jump Number UI Text").GetComponent<TMPro.TextMeshProUGUI>();
        jumpNumUI = GameObject.Find("Jump Number UI");
        if (player.HasAbility(1))
        {
            jumpNumText.text = player.GetJumpsPerLevel() + "";
        }
        else
        {
            jumpNumUI.SetActive(false);
        }
        puttIcon = GameObject.Find("Putt Shot Icon");
        chipIcon = GameObject.Find("Chip Shot Icon");
        curveIcon = GameObject.Find("Curve Shot Icon");
        puttIcon.SetActive(false);
        chipIcon.SetActive(false);
        curveIcon.SetActive(false);

        for (int deg30 = 0; deg30 < 12; deg30++)
        {
            /*Instantiate(castEndObj,
                        triRotator.transform.position + 3 * new Vector3(-Mathf.Cos(deg30 * Mathf.PI / 6), 0, Mathf.Sin(deg30 * Mathf.PI / 6)),
                        Quaternion.Euler(Vector3.zero),
                        test.transform);*/
            canRotateAtDegree[deg30] = !Physics.Linecast(triRotator.transform.position,
                                                         triRotator.transform.position + 3 * new Vector3(-Mathf.Cos(deg30 * Mathf.PI / 6), 0, Mathf.Sin(deg30 * Mathf.PI / 6)));
        }

        sirPuttsAlot = GameObject.Find("Sir Puttsalot");
        sirPuttAnim = GameObject.Find("Sir Puttsalot").GetComponent<SirPuttAnimController>();

        spikesModel = GameObject.Find("Golf Ball Spikes");
        leftCurveLoc = GameObject.Find("Left Curve Loc");
        rightCurveLoc = GameObject.Find("Right Curve Loc");

        curHitPos = transform.position;

        holeEndUIBackground = GameObject.Find("Hole End Background");
        ehStrokeText = GameObject.Find("End Hole Stroke Text").GetComponent<TMPro.TextMeshProUGUI>();
        ehParText = GameObject.Find("End Hole Par Text").GetComponent<TMPro.TextMeshProUGUI>();
        ehRewardText = GameObject.Find("End Hole EXP Reward Text").GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //test.transform.rotation = Quaternion.Euler(Vector3.zero);


        if (!inEndScreen)
        {
            //Keeps track of the golf ball's velocity for collisions
            if (!hasJustSwung)
            {
                lastFrameVel = curVel;
            }
            else
            {
                if (golfBallRb.velocity.magnitude != 0)
                {
                    hasJustSwung = false;
                }
            }
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
                    friction1Over30 = golfBallRb.velocity.normalized * friction * (usedSpikeBall ? 2 : 1) * (usingFireball ? 0 : 1) *
                                           (icyTurns > 0 ? .5f : 1) * (airBallTurns > 0 ? .5f : 1) *
                                           frictionTimeMod;
                    golfBallRb.velocity -= golfBallRb.velocity.normalized * friction * (usedSpikeBall ? 2 : 1) * (usingFireball ? 0 : 1) *
                                           (icyTurns > 0 ? .5f : 1) * (airBallTurns > 0 ? .5f : 1) *
                                           frictionTimeMod * Time.deltaTime;
                }
                else if (!ballHasStopped && !waitingForStopCheck && !waitingForDamageAnim)
                {
                    //Debug.Log(golfBallRb.velocity.magnitude);
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
                    //Debug.Log("Deactivated Spike Ball");
                    StartCoroutine(SpikeBallEndAnim());
                    usedSpikeBall = false;
                }

                //Allow changing shot type if player has ability(s)
                if (player.HasAbility(0) && Input.GetKeyDown(chipShotKey) && ballHasStopped)
                {
                    if (curShotType == 1)
                    {
                        curShotType = 0;
                        puttIcon.SetActive(true);
                        chipIcon.SetActive(false);
                        curveIcon.SetActive(false);
                        Debug.Log("Current Shot Type: Putt");
                    }
                    else
                    {
                        curShotType = 1;
                        puttIcon.SetActive(false);
                        chipIcon.SetActive(true);
                        curveIcon.SetActive(false);
                        Debug.Log("Current Shot Type: Chip");
                    }
                }

                if (player.HasAbility(7) && Input.GetKeyDown(curveShotKey) && ballHasStopped)
                {
                    if (curShotType == 2)
                    {
                        curShotType = 3;
                        puttIcon.SetActive(false);
                        chipIcon.SetActive(false);
                        curveIcon.SetActive(true);
                        Debug.Log("Current Shot Type: Curve Right");
                    }
                    else if (curShotType == 3)
                    {
                        curShotType = 0;
                        puttIcon.SetActive(true);
                        chipIcon.SetActive(false);
                        curveIcon.SetActive(false);
                        Debug.Log("Current Shot Type: Putt");
                    }
                    else
                    {
                        curShotType = 2;
                        puttIcon.SetActive(false);
                        chipIcon.SetActive(false);
                        curveIcon.SetActive(true);
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
                        float curRotation = triRotator.transform.rotation.eulerAngles.y;
                        if (!inBackupPosition)
                        {
                            if (canRotateAtDegree[(int)((curRotation) % 360 / 30)] && canRotateAtDegree[(int)((curRotation + 30) % 360 / 30)])
                            {
                                sirPuttsAlot.transform.localScale = new Vector3(1, 1, 1);
                                triRotatorCopy.transform.rotation = triRotator.transform.rotation;
                            }
                            else if (canRotateAtDegree[(int)((curRotation + 180) % 360 / 30)] && canRotateAtDegree[(int)((curRotation + 210) % 360 / 30)])
                            {
                                sirPuttsAlot.transform.localScale = new Vector3(1, 1, -1);
                                triRotatorCopy.transform.rotation = Quaternion.Euler(triRotator.transform.rotation.eulerAngles.x,
                                                                                     triRotator.transform.rotation.eulerAngles.y + 180,
                                                                                     triRotator.transform.rotation.eulerAngles.z);
                                inBackupPosition = true;
                            }
                        }
                        else
                        {
                            if (canRotateAtDegree[(int)((curRotation + 180) % 360 / 30)] && canRotateAtDegree[(int)((curRotation + 210) % 360 / 30)])
                            {
                                sirPuttsAlot.transform.localScale = new Vector3(1, 1, -1);
                                triRotatorCopy.transform.rotation = Quaternion.Euler(triRotator.transform.rotation.eulerAngles.x,
                                                                                     triRotator.transform.rotation.eulerAngles.y + 180,
                                                                                     triRotator.transform.rotation.eulerAngles.z);
                            }
                            else if (canRotateAtDegree[(int)((curRotation) % 360 / 30)] && canRotateAtDegree[(int)((curRotation + 30) % 360 / 30)])
                            {
                                sirPuttsAlot.transform.localScale = new Vector3(1, 1, 1);
                                triRotatorCopy.transform.rotation = triRotator.transform.rotation;
                                inBackupPosition = false;
                            }
                        }
                    }
                    hitDir2D.transform.rotation = Quaternion.Euler(new Vector3(0, 0,
                                                                   camRotator.transform.rotation.eulerAngles.y -
                                                                   triRotator.transform.rotation.eulerAngles.y));
                    if (mouseDif.magnitude < 250)
                    {
                        powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(mouseDif.magnitude * 150 / 250,
                                                                                       mouseDif.magnitude * 700 / 250);
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
                    if (curShotType == 0)
                    {
                        puttIcon.SetActive(true);
                    }
                    else if (curShotType == 1)
                    {
                        chipIcon.SetActive(true);
                    }
                    else
                    {
                        curveIcon.SetActive(true);
                    }
                    mouseStarterPos = Input.mousePosition;
                    transform.rotation = camRotator.transform.rotation;
                    hitDir2DUI.SetActive(true);
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
                    jumpNumText.text = (int.Parse(jumpNumText.text) - 1) + "";
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
                    //Debug.Log("Activated Spike Ball");
                    StartCoroutine(SpikeBallStartAnim());
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
                                    player.SetWaitingForLevelUp(true);
                                    LevelUpNotif();
                                }
                                else if (!player.WaitingForLevelUp())
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
        frictionTimeMod = 1;
        curHitPos = transform.position;
        waitingForDamageAnim = false;
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
            Vector3 forceToAdd = power * barPercentage * (power * (tri.transform.position -
                                triRotator.transform.position).normalized + new Vector3(0, 7, 0)).normalized;
            golfBallRb.AddForce(forceToAdd, ForceMode.Impulse);
            lastFrameVel = forceToAdd;
        }
        else
        {
            Vector3 forceToAdd = power * barPercentage * (tri.transform.position - triRotator.transform.position).normalized;
            golfBallRb.AddForce(forceToAdd, ForceMode.Impulse);
            lastFrameVel = forceToAdd;
        }

        hasJustSwung = true;
        tri.SetActive(false);
        powerBar.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
        barPercentage = 0;
        powerBar.GetComponent<Image>().color = GetBarColor();

        puttIcon.SetActive(false);
        chipIcon.SetActive(false);
        curveIcon.SetActive(false);
        hitDir2DUI.SetActive(false);

        numStrokes++;
        lastShotOB = false;

        if (isDebugMode)
        {
            if (player.GainEXP(50))
            {
                player.SetWaitingForLevelUp(true);
                LevelUpNotif();
            }
            else if (!player.WaitingForLevelUp())
            {
                UpdateExpBar();
            }
        }
    }

    IEnumerator SpikeBallStartAnim()
    {
        for (int frameNum = 0; frameNum <= 20; frameNum++)
        {
            spikesModel.transform.localScale = new Vector3(spikesModel.transform.localScale.x +.03f,
                                                           spikesModel.transform.localScale.y + .03f,
                                                           spikesModel.transform.localScale.z + .03f);
            yield return new WaitForSeconds(1 / 60f);
        }
        if (!inSpikeBallEndAnim)
        {
            spikesModel.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    IEnumerator SpikeBallEndAnim()
    {
        inSpikeBallEndAnim = true;
        for (int frameNum = 0; frameNum <= 40; frameNum++)
        {
            spikesModel.transform.localScale = new Vector3(spikesModel.transform.localScale.x - .015f,
                                                           spikesModel.transform.localScale.y - .015f,
                                                           spikesModel.transform.localScale.z - .015f);
            yield return new WaitForSeconds(1 / 60f);
        }
        spikesModel.transform.localScale = new Vector3(.4f, .4f, .4f);
        inSpikeBallEndAnim = false;
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
        if ((golfBallRb.velocity.magnitude < friction * (usedSpikeBall ? 2 : 1) *
                                            (icyTurns > 0 ? .5f : 1) * (airBallTurns > 0 ? .5f : 1) * 
                                            frictionTimeMod * Time.deltaTime) && !waitingForDamageAnim)
        {
            ballInAnim = true;
            for (int deg30 = 0; deg30 < 12; deg30++)
            {
                canRotateAtDegree[deg30] = !Physics.Linecast(triRotator.transform.position,
                                                             triRotator.transform.position + 3 * new Vector3(-Mathf.Cos(deg30 * Mathf.PI / 6), 0, Mathf.Sin(deg30 * Mathf.PI / 6)));
            }
            golfBallRb.velocity = Vector3.zero;
            golfBallRb.constraints = RigidbodyConstraints.FreezeAll;
            ballHasStopped = true;
            waitingForStopCheck = false;
            ballShotTime = 0;
            frictionTimeMod = 1;
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
                transform.position = new Vector3(transform.position.x, transform.position.y + .125f, transform.position.z);
                transform.localScale = new Vector3(.5f, .5f, .5f);
            }
            icyTurns--;
            if ((curHitPos - lastHitPos).magnitude < .5f)
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

    public bool CanRotateAtDegree30(int index)
    {
        return canRotateAtDegree[index];
    }

    public void SetBallInAnim(bool input)
    {
        ballInAnim = input;
    }

    public void ResetSirPuttsalot()
    {
        inBackupPosition = false;
        sirPuttsAlot.transform.localScale = new Vector3(1, 1, 1);
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
                    player.SetWaitingForLevelUp(true);
                    LevelUpNotif();
                }
                else if (!player.WaitingForLevelUp())
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
                if ((golfBallRb.velocity * collision.gameObject.GetComponent<Enemy>().bounciness).magnitude > friction1Over30.magnitude)
                {
                    Debug.Log("Used Bounciness");
                    golfBallRb.velocity *= collision.gameObject.GetComponent<Enemy>().bounciness;
                }
                else
                {
                    golfBallRb.velocity *= friction1Over30.magnitude / golfBallRb.velocity.magnitude;
                }
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
                if (shieldsUsed >= player.GetShieldsPerLevel())
                {
                    shieldVisual.SetActive(false);
                }
            }
            else
            { // ...reset its position to the last hit position if they have no shield.
                ResetBallPosition(lastHitPos);
            }
        }  
        
        if (collision.gameObject.CompareTag("Delayed Damage"))
        {
            waitingForDamageAnim = true;
            golfBallRb.constraints = RigidbodyConstraints.FreezeAll;
            StopCoroutine(BallStopCheck());
            StartCoroutine(DelayedDamage());
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

    IEnumerator DelayedDamage()
    {
        yield return new WaitForSeconds(1);
        ResetBallPosition(lastHitPos);
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
        if (player.WaitingForLevelUp() && !inLevelUpScreen && golfBallRb.velocity == new Vector3(0, 0, 0))
        {
            inLevelUpScreen = true;
            waitingForLevelUpAnim = true;
            playerLevel++;
            player.AddGolfBallLevel();
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
                    SelectRandomRewards();
                    break;
            }
            InvokeRepeating("LevelUpUIEnterAnim", 0, 1 / 60f);
            if (playerLevel == player.GetLevel())
            {
                player.SetWaitingForLevelUp(false);
                UpdateExpBar();
            }
        }
    }

    private void SelectRandomRewards()
    {
        levelUpRewardUIs[0].gameObject.SetActive(false);
        levelUpRewardUIs[1].gameObject.SetActive(false);
        int[] nextAbilities = new int[3];
        for (int count = 0; count < 3; count++)
        {
            int abilityNum;
            do
            {
                abilityNum = (int)Random.Range(0, 11.999f);
            }
            while (player.HasAbility(abilityNum) || (count > 0 ? nextAbilities[0] == abilityNum : false) || (count > 1 ? nextAbilities[1] == abilityNum : false));
            nextAbilities[count] = abilityNum;
            levelUpRewardUIs[count + 2].gameObject.SetActive(true);
            levelUpRewardUIs[count + 2].SetLevelUpUI(abilityNum);
            if (abilityNum == 2 || abilityNum == 3 || abilityNum == 6 || abilityNum == 9 || abilityNum == 10)
            {
                levelUpRewardUIs[count + 2].AddDesc("Press " + nextAbilityKey + " to activate.");
            }
            if (abilityNum == 5)
            {
                levelUpRewardUIs[count + 2].AddDesc("You will have " + (player.GetShieldsPerLevel() + 1) + " shield uses per hole.");
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
        if (abilityCode == 1)
        {
            jumpNumUI.SetActive(true);
            jumpNumText.text = "2";
        }
        if (abilityCode == 5)
        {
            shieldVisual.SetActive(true);
        }
        if (abilityCode == 8)
        {
            jumpNumText.text = (int.Parse(jumpNumText.text) + 2) + "";
        }
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

    public void FinishHole(string sceneIn, int parIn)
    {
        sceneToGoTo = sceneIn;
        golfBallRb.constraints = RigidbodyConstraints.FreezeAll;
        ehStrokeText.text = numStrokes + "";
        ehParText.text = parIn + "";
        if (!inLevelUpScreen)
        {
            inEndScreen = true;
            waitingOnEndAnim = true;
            if (numStrokes == 1)
            {
                if (player.GainEXP(400))
                {
                    player.SetWaitingForLevelUp(true);
                    LevelUpNotif();
                }
                else
                {
                    UpdateExpBar();
                }
                ehRewardText.text = 400 + "";
            }
            else if (numStrokes <= parIn - 4)
            {
                if (player.GainEXP(200))
                {
                    player.SetWaitingForLevelUp(true);
                    LevelUpNotif();
                }
                else
                {
                    UpdateExpBar();
                }
                ehRewardText.text = 200 + "";
            }
            else if (numStrokes <= parIn)
            {
                if (player.GainEXP(50 + 50 * (parIn - numStrokes)))
                {
                    player.SetWaitingForLevelUp(true);
                    LevelUpNotif();
                }
                else
                {
                    UpdateExpBar();
                }
                ehRewardText.text = (50 + 50 * (parIn - numStrokes)) + "";
            }
            InvokeRepeating("HoleEndUIEnterAnim", 0, 1 / 60f);
        }
        else
        {
            StartCoroutine(BufferHoleEndUI(parIn));
        }
    }

    IEnumerator BufferHoleEndUI(int parIn)
    {
        bool waiting = true;
        while (waiting)
        {
            yield return new WaitForSeconds(1 / 60f);
            if (!inLevelUpScreen)
            {
                waiting = false;
                inEndScreen = true;
                waitingOnEndAnim = true;
                if (numStrokes == 1)
                {
                    if (player.GainEXP(400))
                    {
                        player.SetWaitingForLevelUp(true);
                        LevelUpNotif();
                    }
                    else
                    {
                        UpdateExpBar();
                    }
                    ehRewardText.text = 400 + "";
                }
                else if (numStrokes <= parIn - 4)
                {
                    if (player.GainEXP(200))
                    {
                        player.SetWaitingForLevelUp(true);
                        LevelUpNotif();
                    }
                    else
                    {
                        UpdateExpBar();
                    }
                    ehRewardText.text = 200 + "";
                }
                else if (numStrokes <= parIn)
                {
                    if (player.GainEXP(50 + 50 * (parIn - numStrokes)))
                    {
                        player.SetWaitingForLevelUp(true);
                        LevelUpNotif();
                    }
                    else
                    {
                        UpdateExpBar();
                    }
                    ehRewardText.text = (50 + 50 * (parIn - numStrokes)) + "";
                }
                if (player.WaitingForLevelUp())
                {
                    LevelUpNotif();
                }
                else
                {
                    UpdateExpBar();
                }
                InvokeRepeating("HoleEndUIEnterAnim", 0, 1 / 60f);
            }
        }
    }

    private void HoleEndUIEnterAnim()
    {
        holeEndAnimCount++;
        holeEndUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(0, 1920 * (1 - Mathf.Sin(Mathf.PI
                                                                                      * holeEndAnimCount / 120)), 0);

        if (holeEndAnimCount == 60)
        {
            CancelInvoke();
            waitingOnEndAnim = false;
            holeEndUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            holeEndAnimCount = 0;
        }
    }

    public void LoadNextScene()
    {
        if (inEndScreen && !waitingOnEndAnim)
        {
            SceneManager.LoadScene(sceneToGoTo, LoadSceneMode.Single);
        }
    }
}