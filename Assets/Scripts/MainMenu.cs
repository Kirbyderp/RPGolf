using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private bool onTitle = true;
    private GameObject menuDarkener, mainMenuUI, howToPlayUI, creditsUI;
    
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
    }

    // Update is called once per frame
    void Update()
    {
        if (onTitle && Input.GetMouseButtonDown(0))
        {
            onTitle = false;
            menuDarkener.SetActive(true);
        }
    }

    public void PlayGame()
    {

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
}
