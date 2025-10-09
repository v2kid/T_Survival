using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set; }

    [Header("Radar Settings")]
    public float radarRange = 50f;
    public RectTransform radarRect;
    public GameObject enemyIconPrefab;
    public GameObject playerIconPrefab;

    [Header("Player")]
    private Transform player;
    private GameObject playerIcon;

    [Header("Enemies")]
    private List<Transform> registeredEnemies = new List<Transform>();
    private List<GameObject> enemyIcons = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Auto-register player if PlayerStats exists
        if (PlayerStats.Instance != null)
        {
            RegisterPlayer(PlayerStats.Instance.transform);
        }
    }

    private void Update()
    {
        UpdateRadar();
    }

    public void RegisterPlayer(Transform playerTransform)
    {
        player = playerTransform;

        if (playerIconPrefab != null && radarRect != null)
        {
            // Create player icon at center
            playerIcon = Instantiate(playerIconPrefab, radarRect);
            playerIcon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Debug.Log("Player registered on radar");
        }
    }

    public void RegisterEnemy(Transform enemyTransform)
    {
        if (!registeredEnemies.Contains(enemyTransform))
        {
            registeredEnemies.Add(enemyTransform);
            Debug.Log($"Enemy registered: {enemyTransform.name}");
        }
    }

    public void UnregisterEnemy(Transform enemyTransform)
    {
        registeredEnemies.Remove(enemyTransform);
        Debug.Log($"Enemy unregistered: {enemyTransform.name}");
    }

    private void UpdateRadar()
    {
        if (player == null || radarRect == null) return;

        // Clear old enemy icons
        foreach (var icon in enemyIcons)
        {
            if (icon != null) Destroy(icon);
        }
        enemyIcons.Clear();

        // Clean up null/destroyed enemies
        registeredEnemies.RemoveAll(enemy => enemy == null);

        // Create enemy icons
        foreach (var enemy in registeredEnemies)
        {
            if (enemy == null) continue;

            Vector3 offset = enemy.position - player.position;
            float distance = offset.magnitude;

            // Skip if out of radar range
            if (distance > radarRange) continue;

            // Convert world position to radar position
            float radarX = (offset.x / radarRange) * (radarRect.rect.width / 2f);
            float radarY = (offset.z / radarRange) * (radarRect.rect.height / 2f);

            // Create enemy icon
            GameObject icon = Instantiate(enemyIconPrefab, radarRect);
            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(radarX, radarY);
            enemyIcons.Add(icon);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}