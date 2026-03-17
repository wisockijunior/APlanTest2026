using System;
using UnityEngine;

public enum GameState { Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int StartingMoves = 5;
    public int CurrentScore { get; private set; }
    public int RemainingMoves { get; private set; }
    public GameState State { get; private set; } = GameState.Playing;

    public bool IsPlaying => State == GameState.Playing;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnMovesChanged;
    public event Action OnGameOver;
    public event Action OnGameRestarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResetGame();
    }

    public void ResetGame()
    {
        CurrentScore = 0;
        RemainingMoves = StartingMoves;
        State = GameState.Playing;
        
        OnScoreChanged?.Invoke(CurrentScore);
        OnMovesChanged?.Invoke(RemainingMoves);
        OnGameRestarted?.Invoke();
    }

    public void AddScore(int amount)
    {
        if (!IsPlaying) return;

        // Points awarded based on the number of blocks collected (+1 for 1 block, +2 for 2, etc.)
        // This logic will be handled by the caller (GridManager), passing the total points.
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>
    /// testing environment
    /// emulate a movement and update UI
    /// </summary>
    public void DoMakeMove()
    {
        if (!IsPlaying) return;

        if (RemainingMoves > 0)
        {
            RemainingMoves--;
            AddScore(10);

            OnMovesChanged?.Invoke(RemainingMoves);
            if (RemainingMoves <= 0)
            {
                EndGame();
            }
        }
    }

    private void EndGame()
    {
        State = GameState.GameOver;
        OnGameOver?.Invoke();
    }

    public void Replay()
    {
        ResetGame();
    }
}


