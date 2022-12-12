using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string target;

    public void Load()
    {
        SceneManager.LoadScene(target);
    }
}