using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] AudioSource menuAudio;
    [SerializeField] AudioClip startSound;

    public void PlayButtonPressed()
    {
        menuAudio.PlayOneShot(startSound);
        StartCoroutine(LoadGame());
    }

    public void QuitButtonPressed()
    {
        StartCoroutine(QuitGame());
    }

    IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(0.1f);
        mainMenuPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        LeanTween.moveLocalY(mainMenuPanel, 1000f, 1f).setEaseOutQuad();
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("EkkamScene");
    }

    IEnumerator QuitGame()
    {
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
