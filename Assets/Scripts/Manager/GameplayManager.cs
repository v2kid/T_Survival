using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    private UIShop _uiShop;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        WaveManager.OnWaveCleared += OnWaveCleared;
        _uiShop = FindFirstObjectByType<UIShop>();
    }

    private void OnWaveCleared()
    {
        PauseGame();
        _uiShop.ActiveCanvas(true);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        WaveManager.Instance.StartNextWave();
    }


    public void PauseGame()
    {
        Time.timeScale = 0f;
    }



}