using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUI : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI MovesText;
    public GameObject GameOverPanel;

    [Header("TestButtons")]
    public Button ReplayButton;
    public Button MakeMoveButton;
    
    void Start()
    {
        GameManager.Instance.OnScoreChanged += (score) =>
        {
            ScoreText.text = $"SCORE:  <color=#FFFFFF>{score}</color>";
        };

        GameManager.Instance.OnMovesChanged += (moves) =>
        {
            MovesText.text = moves.ToString();
        };

        GameManager.Instance.OnGameOver += () =>
        {
            GameOverPanel.SetActive(true);
        };
        GameManager.Instance.OnGameRestarted += () =>
        {
            GameOverPanel.SetActive(false);
        };

        // start hidden - hide GameOver panel on start
        GameOverPanel.SetActive(false);

        ReplayButton.onClick.AddListener(OnReplayClicked);
        MakeMoveButton.onClick.AddListener(OnMakeMoveClicked);
    }
    
    void Update()
    {
        
    }

    private void OnReplayClicked()
    {
        GameManager.Instance.Replay();
    }
    private void OnMakeMoveClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            GameManager.Instance.AddScore(10);
            GameManager.Instance.DoMakeMove();
        }
    }
}
