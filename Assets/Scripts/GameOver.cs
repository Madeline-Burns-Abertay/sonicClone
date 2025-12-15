using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
	InputAction navigate, submit;
	Vector2 previousInput;
	bool shouldContinue = true;
	[SerializeField] TMP_Text optionText;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		navigate = InputSystem.actions.FindAction("Navigate");
		submit = InputSystem.actions.FindAction("Submit");
	}

	// Update is called once per frame
	void Update()
	{
		Vector2 input = navigate.ReadValue<Vector2>();
		Debug.Log(input);
		bool changeOptions = input.y != 0 && input != previousInput;
		previousInput = input;
		shouldContinue ^= changeOptions;
		optionText.text = $"Continue?\n{(shouldContinue ? ">Yes \nNo" : "Yes\n>No ")}"; // extra spaces to keep it centered
		if (submit.WasPressedThisFrame())
		{
			string nextScene = $"{(shouldContinue ? "HillAct1" : "TitleScreen")}";
			SceneManager.LoadScene(nextScene);
		}
	}
}
