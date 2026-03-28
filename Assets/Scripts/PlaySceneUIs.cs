using UnityEngine;
using UnityEngine.SceneManagement;
public class PlaySceneUIs : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject instructionPanel;

    private void Start()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        Time.timeScale = 1f; // ensure game starts running
    }

    public void ShowInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
            Time.timeScale = 0f; // PAUSE GAME
        }
    }

    public void HideInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
            Time.timeScale = 1f; // RESUME GAME
        }
    }



    public void GoToMainMenu()
    {
        if (instructionPanel != null)
        {
            SceneManager.LoadScene("StartScene");
           
            Time.timeScale = 1f; // RESUME GAME
        }
    }


}

