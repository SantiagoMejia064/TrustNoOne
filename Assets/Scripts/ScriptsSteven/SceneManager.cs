using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    public void IrInicio()
    {
        UnitySceneManager.LoadScene("inicio");
    }

    public void IrJuego()
    {
        UnitySceneManager.LoadScene("Juego");
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
