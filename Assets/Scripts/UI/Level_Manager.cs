using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public GameObject transitionsContainer;

    private SceneTransition[] transitions;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void LoadScene(Scene scene, Transition transitionName)
    {
        StartCoroutine(LoadSceneAsync(scene, transitionName));
    }

    private IEnumerator LoadSceneAsync(Scene sceneName, Transition transitionName)
    {
        SceneTransition transition = transitions.First(t => t.name == transitionName.ToString());

        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName.ToString());
        scene.allowSceneActivation = false;
        
        yield return transition.AnimateTransitionIn();

        while(scene.progress < 0.9f)
        {
            yield return null;
        }

        scene.allowSceneActivation = true;
        
        yield return transition.AnimateTransitionOut();
    }
}

public enum Scene
{
    TestScene,
    MenuScene,
}

public enum Transition
{
    CrossFade,
}