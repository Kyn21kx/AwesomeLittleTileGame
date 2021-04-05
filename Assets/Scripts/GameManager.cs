using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {

	public TileShuffler shuffler;
	public TMP_InputField timeInputTxt;
	public TextMeshProUGUI userInfoText;
	public TextMeshProUGUI timeDisplayText;
	private float time;
	private bool started;
	private bool gameOver;

	private void Start() {
		//Disable the tiles' control script
		//Using Invoke to make sure the values from the shuffler get initialized properly
		time = 0f;
		started = false;
		gameOver = false;
		Invoke("DisableTiles", 0.5f);
	}

	private void DisableTiles() {
		shuffler.enabled = false;
		userInfoText.SetText("Por favor ingresa la cantidad de tiempo que te tomará resolver el puzzle y presiona Enter!");
	}

	private void Update() {
		SetTimeStart();
		if (started && !gameOver) {
			//Count down time to win
			CheckCounterReduce();
			DisplayTime();
		}
		if (gameOver && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) {
			SceneManager.LoadScene(0);
		}
	}

	private void CheckCounterReduce() {
		if (time <= 0f) {
			//Check if you've won or not
			if (shuffler.Won) {
				userInfoText.SetText("Felicidades, has ganado!!!");
			}
			else {
				userInfoText.SetText("Fin del juego, perdiste :(\nPresiona Enter para reiniciar");
				shuffler.enabled = false;
				gameOver = true;
			}
		}
		time -= Time.deltaTime;
	}

	private void DisplayTime() {
		int minutes = (int)(time / 60);
		int seconds = (int)time % 60;
		string s_seconds = seconds.ToString();
		if (seconds < 10) {
			s_seconds = "0" + seconds;
		}
		if (minutes == 0) {
			timeDisplayText.color = Color.red;
		}
		timeDisplayText.SetText($"{minutes}:{s_seconds}");
	}

	private void SetTimeStart() {
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) && !started) {
			time = GetInputTime();
			if (time <= 0f || time > 59 * 60) {
				userInfoText.SetText("Entrada inválida, intenta de nuevo...");
				return;
			}
			userInfoText.SetText("");
			started = true;
			//Now let's enable the shuffler
			shuffler.enabled = true;
			timeInputTxt.gameObject.SetActive(false);
		}
	}

	private float GetInputTime() {
		try {
			return float.Parse(timeInputTxt.text);
		}
		catch(System.Exception) {
			return -1f;
		}
	}

}
