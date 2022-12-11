using UnityAtoms.SceneMgmt;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public SceneField target;

    public void Load()
    {
        SceneManager.LoadScene(target);
    }
}