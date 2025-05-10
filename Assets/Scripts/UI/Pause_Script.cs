using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);

        // Get image component
        Image image = pauseMenuUI.GetComponent<Image>();
        image.CrossFadeAlpha(0f, 0f, true);
        image.CrossFadeAlpha(0.85f, 0.2f, true);

        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void Quit()
    {
        pauseMenuUI.SetActive(false);
        Debug.Log("Quitting to Main Menu...");

        // Going to Main menu
        LevelManager.Instance.LoadScene(Scene.MenuScene, Transition.CrossFade);

        Time.timeScale = 1f;
        GameIsPaused = false;
    }
}
