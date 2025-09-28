using System;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradeSlot : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI statTypeText;
    [SerializeField] private TMPro.TextMeshProUGUI valueText;
    [SerializeField] private TMPro.TextMeshProUGUI cost;
    [SerializeField] private Button upgradeButton;
    public event Action<StatType, float> OnUpgradeClicked;

    private StatType _statType;
    private Rarity _rarity;
    private float _value;

    public void Setup(StatRarityConfig statRarityConfig, StatType statType)
    {
        _statType = statType;
        _rarity = statRarityConfig.rarity;
        cost.text = statRarityConfig.cost.ToString();
        GameDataManager.Instance.GetRarityColor(_rarity, out Color color);
        statTypeText.color = color;
        statTypeText.text = _statType.ToString();
        valueText.text = TextHelper.FormatStat(_statType, statRarityConfig.value);
        _value = statRarityConfig.value;
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnClick);

    }
    private void OnClick()
    {
        OnUpgradeClicked?.Invoke(_statType, _value);
        upgradeButton.interactable = false;
        OnUpgradeClicked = null;
    }

    private void OnDestroy()
    {
        upgradeButton.onClick.RemoveAllListeners();
        OnUpgradeClicked = null;
    }


}