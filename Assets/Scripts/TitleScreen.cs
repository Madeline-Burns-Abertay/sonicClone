using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] float delaySeconds = 15f;
    InputAction start;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        start = InputSystem.actions.FindAction("Submit");
        StartCoroutine(playDemo(delaySeconds));
    }

    // Update is called once per frame
    void Update()
    {
        if (start.IsInProgress())
        {
            SceneManager.LoadScene("HillAct1");
        }
    }

    IEnumerator playDemo(float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Demo");
    }
}
