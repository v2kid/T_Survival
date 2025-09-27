using UnityEngine;
using TMPro;

public class UIShop : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI hpRegen;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI armor;
    [SerializeField] private TextMeshProUGUI lifeSteal;
    [SerializeField] private TextMeshProUGUI lifeStealRate;
    [SerializeField] private TextMeshProUGUI critChance;
    [SerializeField] private TextMeshProUGUI critMultiplier;
    [SerializeField] private TextMeshProUGUI coinDropChance;

    private void Start()
    {
        var stats = PlayerStats.Instance;

        stats.HpRegen.Subscribe((o, n) => hpRegen.text = o == n ? $"HpRegen: {n:F0}" : $"HpRegen: {o:F0} -> {n:F0}", true);
        stats.Coin.Subscribe((o, n) => coinText.text = o == n ? $"Coin: {n}" : $"Coin: {o} -> {n}", true);
        stats.MaxHealth.Subscribe((o, n) => maxHealthText.text = o == n ? $"MaxHealth: {n:F0}" : $"MaxHealth: {o:F0} -> {n:F0}", true);
        stats.MeleeDamage.Subscribe((o, n) => damageText.text = o == n ? $"Damage: {n:F0}" : $"Damage: {o:F0} -> {n:F0}", true);
        stats.Armor.Subscribe((o, n) => armor.text = o == n ? $"Armor: {n:F0}" : $"Armor: {o:F0} -> {n:F0}", true);
        stats.LifeSteal.Subscribe((o, n) => lifeSteal.text = o == n ? $"LifeSteal: {(n * 100f):F1}%" : $"LifeSteal: {(o * 100f):F1}% -> {(n * 100f):F1}%", true);
        stats.LifeStealRate.Subscribe((o, n) => lifeStealRate.text = o == n ? $"LifeStealRate: {(n * 100f):F1}%" : $"LifeStealRate: {(o * 100f):F1}% -> {(n * 100f):F1}%", true);
        stats.CritChance.Subscribe((o, n) => critChance.text = o == n ? $"CritChance: {(n * 100f):F1}%" : $"CritChance: {(o * 100f):F1}% -> {(n * 100f):F1}%", true);
        stats.CritMultiplier.Subscribe((o, n) => critMultiplier.text = o == n ? $"CritMultiplier: {n:F2}x" : $"CritMultiplier: {o:F2}x -> {n:F2}x", true);
        stats.CoinDropChance.Subscribe((o, n) => coinDropChance.text = o == n ? $"CoinDropChance: {(n * 100f):F1}%" : $"CoinDropChance: {(o * 100f):F1}% -> {(n * 100f):F1}%", true);
    }
}
