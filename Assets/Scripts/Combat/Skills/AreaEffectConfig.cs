using UnityEngine;

[System.Serializable]
public struct AreaEffectConfig
{
    [Header("Timing Settings")]
    public float activationDelay;
    public float duration;
    public float interval;


}


public interface IAreaEffect
{
    void Initialize(AreaEffectConfig config);
    void RestartEffect(AreaEffectConfig config);
    void Stop();
    bool IsRunning { get; }
}
