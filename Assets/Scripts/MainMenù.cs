using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Questa funzione ascolta ogni istante se premi un tasto
    void Update()
    {
        // Se premi un tasto QUALSIASI (tastiera o mouse)...
        // MA NON se è il click sinistro (0) o destro (1) del mouse (per evitare conflitti)
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
        {
            PlayGame();
        }
    }

    public void PlayGame()
    {
        // Carica la scena successiva nella lista
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Il gioco si è chiuso!");
        Application.Quit();
    }
}