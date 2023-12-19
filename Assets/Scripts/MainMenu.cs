using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private bool onTitle = true, onPage1 = true;
    private GameObject menuDarkener, mainMenuUI, howToPlayUI, creditsUI;
    private GameObject cutsceneUICanvas, menuCanvas, mainCamera;
    private GameObject curPage;
    
    // Start is called before the first frame update
    void Start()
    {
        menuDarkener = GameObject.Find("Menu Darkener");
        mainMenuUI = GameObject.Find("Main Menu UI");
        howToPlayUI = GameObject.Find("How To Play UI");
        creditsUI = GameObject.Find("Credits UI");

        howToPlayUI.SetActive(false);
        creditsUI.SetActive(false);
        menuDarkener.SetActive(false);

        menuCanvas = GameObject.Find("Menu Canvas");
        cutsceneUICanvas = GameObject.Find("Cutscene UI Canvas");
        cutsceneUICanvas.SetActive(false);
        mainCamera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (onTitle && Input.GetMouseButtonDown(0))
        {
            onTitle = false;
            menuDarkener.SetActive(true);
        }
    }

    public void PlayGame()
    {
        menuCanvas.SetActive(false);
        cutsceneUICanvas.SetActive(true);
        Instantiate(Resources.Load<GameObject>("Objects/Square"));
        mainCamera.transform.position = new Vector3(1000, 1000, 1000);
        mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        curPage = (GameObject)(Instantiate(Resources.Load("Narration Elements/Pg_1_Final_No_Anim")));
    }

    public void ShowHowToPlay()
    {
        mainMenuUI.SetActive(false);
        howToPlayUI.SetActive(true);
    }

    public void ShowCredits()
    {
        mainMenuUI.SetActive(false);
        creditsUI.SetActive(true);
    }

    public void ReturnToMain()
    {
        howToPlayUI.SetActive(false);
        creditsUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void AdvanceCutscene()
    {
        if (onPage1)
        {
            Destroy(curPage);
            Instantiate(Resources.Load("Narration Elements/Pg_1_Final"));
            onPage1 = false;
        }
        else
        {
            SceneManager.LoadScene("Hole1_Cliff", LoadSceneMode.Single);
        }
    }
}
