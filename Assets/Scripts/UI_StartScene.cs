using UnityEngine;
using UnityEngine.SceneManagement;
public class UI_StartScene : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject instructionPanel;



    public void Play()
    {
        SceneManager.LoadScene("Play Scene");
    }

    public void ShowInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }
    }

    public void HideInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }
    }
     public void QuitGame()
    {
        Application.Quit();
    }



}
