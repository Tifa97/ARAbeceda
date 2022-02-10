using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartScanningLetters()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void StartScanningWords()
    {
        SceneManager.LoadScene("WordsScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
