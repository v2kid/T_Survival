using UnityEngine;

public class HealingArea : MonoBehaviour, IAreaEffect
{
    private float healingAmount = 5;
    private float healingInterval;
    private float activationDelay;
    private float existingDuration;
    private float healingRadius = 1.6f;

    public LayerMask playerLayer;

    private bool isActive = false;
    private bool isEffectRunning = true;
    private float timer = 0f;

    public bool IsRunning => isEffectRunning;

    public void Initialize(AreaEffectConfig config)
    {
        healingInterval = config.interval;
        activationDelay = config.activationDelay;
        existingDuration = config.duration;

        isActive = false;
        isEffectRunning = true;
        timer = 0f;
    }

    public void RestartEffect(AreaEffectConfig config)
    {
        Initialize(config);
    }

    public void Stop()
    {
        isEffectRunning = false;
        isActive = false;
    }

    private void Update()
    {
        if (!isEffectRunning) return;

        // Wait for activation delay
        if (!isActive)
        {
            activationDelay -= Time.deltaTime;
            if (activationDelay <= 0f)
                isActive = true;
            return;
        }

        timer += Time.deltaTime;
        existingDuration -= Time.deltaTime;

        if (timer >= healingInterval)
        {
            timer = 0f;
            CheckAndHealPlayer();
        }
        if (existingDuration <= 0f)
        {
            Stop();
        }
    }

    private void CheckAndHealPlayer()
    {
        if (PlayerStats.Instance != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerStats.Instance.transform.position);

            if (distanceToPlayer <= healingRadius)
            {
                PlayerStats.Instance.Heal(healingAmount);
            }
        }
    }
}