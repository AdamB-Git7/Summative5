using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Store the PlayerPrefs key used for the saved score.
    private const string SavedScoreKey = "SavedGame.Score";

    // Store the PlayerPrefs key used for the saved-game flag.
    private const string HasSavedGameKey = "SavedGame.HasSave";

    // Store the shared instance of this manager.
    public static GameManager Instance { get; private set; }

    // Store whether the current run is active.
    public bool IsPlaying { get; private set; }

    // Store the score for the current run.
    public int CurrentScore { get; private set; }

    // Expose score changes to UI listeners.
    public event Action<int> OnScoreChanged;

    // Expose game-over state to listeners.
    public event Action OnGameOver;

    // Store the current player name across scenes.
    private static string playerName = "Player";

    // Store whether the next game load should resume a paused run.
    public static bool ResumingSavedGame;

    // Store the paused score across scene changes.
    private static int savedScore;

    // Store whether a paused run exists.
    private static bool hasSavedGame;

    public static void SetPlayerName(string name)
    {
        // Save a trimmed player name or fall back to "Player".
        playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
    }

    public static string GetPlayerName()
    {
        // Return the currently stored player name.
        return playerName;
    }

    private void Awake()
    {
        // Destroy duplicate manager instances.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register this object as the shared manager instance.
        Instance = this;
    }

    private void Start()
    {
        // Start or resume the game as soon as the scene loads.
        StartGame();
    }

    public void StartGame()
    {
        // Refresh saved-game values from PlayerPrefs.
        LoadSavedGameState();

        // Start from the paused score only when resume mode was requested.
        CurrentScore = ResumingSavedGame ? savedScore : 0;

        // Consume the resume flag after using it.
        ResumingSavedGame = false;

        // Mark the game as active.
        IsPlaying = true;

        // Ensure time is running normally.
        Time.timeScale = 1f;

        // Push the initial score to listeners.
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void AddScore(int points)
    {
        // Ignore score changes when gameplay is inactive.
        if (!IsPlaying)
        {
            return;
        }

        // Add the requested points to the current score.
        CurrentScore += points;

        // Notify listeners that the score changed.
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void OnPurlyDied()
    {
        // Ignore duplicate death handling.
        if (!IsPlaying)
        {
            return;
        }

        // Mark gameplay as inactive.
        IsPlaying = false;

        // Freeze the scene.
        Time.timeScale = 0f;

        // Save the current score to disk.
        SaveScore();

        // Stop the background gameplay music when the game ends.
        GameAudio.Instance?.StopMusic();

        // Play the game-over sound effect.
        GameAudio.Instance?.PlayGameOver();

        // Notify listeners that the game ended.
        OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        // Ignore pause requests when gameplay is already inactive.
        if (!IsPlaying)
        {
            return;
        }

        // Mark gameplay as inactive.
        IsPlaying = false;

        // Freeze the scene.
        Time.timeScale = 0f;

        // Save the current score as the paused score.
        savedScore = CurrentScore;

        // Mark that a paused game exists.
        hasSavedGame = true;

        // Persist the paused game data.
        SaveSavedGameState();
    }

    public void ResumeGame()
    {
        // Mark gameplay as active again.
        IsPlaying = true;

        // Resume time.
        Time.timeScale = 1f;
    }

    public static bool HasSavedGame()
    {
        // Refresh the cached saved-game flag when needed.
        if (!hasSavedGame)
        {
            hasSavedGame = PlayerPrefs.GetInt(HasSavedGameKey, 0) == 1;
        }

        // Return whether a paused run exists.
        return hasSavedGame;
    }

    public void RestoreSavedScore()
    {
        // Refresh saved-game values from PlayerPrefs.
        LoadSavedGameState();

        // Restore the paused score into the current session.
        CurrentScore = savedScore;

        // Notify listeners that the score changed.
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void SaveScore()
    {
        // Save the current score to the shared score file.
        ScoreUtility.SaveScore(playerName, CurrentScore);
    }

    public List<ScoreEntry> LoadScores()
    {
        // Return the current top scores from disk.
        return ScoreUtility.LoadTopScores();
    }

    public void RestartGame()
    {
        // Ensure the next scene starts unpaused.
        Time.timeScale = 1f;

        // Reload the current scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // Ensure the menu scene loads unpaused.
        Time.timeScale = 1f;

        // Load the first build-settings scene.
        SceneManager.LoadScene(0);
    }

    private static void SaveSavedGameState()
    {
        // Save whether a paused run exists.
        PlayerPrefs.SetInt(HasSavedGameKey, hasSavedGame ? 1 : 0);

        // Save the paused score value.
        PlayerPrefs.SetInt(SavedScoreKey, savedScore);

        // Flush PlayerPrefs to disk immediately.
        PlayerPrefs.Save();
    }

    private static void LoadSavedGameState()
    {
        // Restore the paused-game flag from PlayerPrefs.
        hasSavedGame = PlayerPrefs.GetInt(HasSavedGameKey, hasSavedGame ? 1 : 0) == 1;

        // Restore the paused score from PlayerPrefs.
        savedScore = PlayerPrefs.GetInt(SavedScoreKey, savedScore);
    }
}

[Serializable]
public class ScoreEntry
{
    // Store the player name shown in the score list.
    public string playerName;

    // Store the score value shown in the score list.
    public int score;
}
