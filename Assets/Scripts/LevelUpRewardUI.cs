using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpRewardUI : MonoBehaviour
{
    public int ID;
    private int abilityCode;
    private TMPro.TextMeshProUGUI rewardName, use, desc;
    private Image rewardImage;
    private GolfBallManager golfBallManager;

    // Start is called before the first frame update
    void Start()
    {
        rewardName = GameObject.Find("Level Up " + ID + " Name").GetComponent<TMPro.TextMeshProUGUI>();
        use = GameObject.Find("Level Up " + ID + " Use").GetComponent<TMPro.TextMeshProUGUI>();
        desc = GameObject.Find("Level Up " + ID + " Desc").GetComponent<TMPro.TextMeshProUGUI>();
        rewardImage = GameObject.Find("Level Up " + ID + " Background").GetComponent<Image>();
        golfBallManager = GameObject.Find("Golf Ball").GetComponent<GolfBallManager>();
        abilityCode = -1;
    }

    public void SetLevelUpUI(int abilityCodeIn)
    {
        if (abilityCodeIn == -1)
        {
            rewardName.text = "Name";
            use.text = "Use When";
            desc.text = "Here is a description about what this level up reward does.";
            return;
        }
        rewardName.text = PlayerInfo.REWARD_NAMES[abilityCodeIn];
        use.text = PlayerInfo.REWARD_USES[abilityCodeIn];
        desc.text = PlayerInfo.REWARD_DESCS[abilityCodeIn];
        //rewardImage.sprite = PlayerInfo.REWARD_IMAGES[rewardID];
        abilityCode = abilityCodeIn;
    }

    public void SetRewardName(string nameIn)
    {
        rewardName.text = nameIn;
    }

    public void SetUse(string useIn)
    {
        use.text = useIn;
    }

    public void SetDesc(string descIn)
    {
        desc.text = descIn;
    }

    public void AddDesc(string descIn)
    {
        desc.text += descIn;
    }

    public void SetImage(Sprite spriteIn)
    {
        rewardImage.sprite = spriteIn;
    }

    public void SelectThis()
    {
        if (!golfBallManager.IsWaitingForLevelUpAnim())
        {
            golfBallManager.SelectReward(abilityCode);
        }
    }
}