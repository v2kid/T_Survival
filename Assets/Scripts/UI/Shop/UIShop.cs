using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;

public class UIShop : MonoBehaviour
{
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI hpRegen;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI armor;
    [SerializeField] private TextMeshProUGUI lifeSteal;
    [SerializeField] private TextMeshProUGUI lifeStealRate;
    [SerializeField] private TextMeshProUGUI critChance;
    [SerializeField] private TextMeshProUGUI critMultiplier;
    [SerializeField] private Button countinueButton;

    [SerializeField] private StatUpgradeConfigSO statUpgradeConfig;
    [SerializeField] private UIUpgradeSlot UpgradeSlotPrefab;
    [SerializeField] private Transform upgradeSlotParent;
    [SerializeField] private Button RerollButton;

    //cach slots 
    private List<UIUpgradeSlot> currentSlots = new();
    private System.Random rng = new System.Random();
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        PlayerStats.Instance.Coin.Subscribe((o, n) => coinText.text = n.ToString());
        PlayerStats.Instance.OnStatChanged += HandleStatChanged;
        countinueButton.onClick.AddListener(() =>
        {
            ActiveCanvas(false);
            GameplayManager.Instance.StartGame();
        });
        RerollButton.onClick.AddListener(Reroll);
        Reroll();
    }

    private void HandleStatChanged()
    {
        var stats = PlayerStats.Instance;
        CharacterStats baseStats = stats.Stats;
        CharacterStats modified = stats.ModifiedStats;

        maxHealthText.text = TextHelper.FormatStat(StatType.Health, baseStats.MaxHealth, modified.MaxHealth);
        hpRegen.text = TextHelper.FormatStat(StatType.Health, baseStats.HpRegen, modified.HpRegen);
        damageText.text = TextHelper.FormatStat(StatType.Damage, baseStats.Damage, modified.Damage);
        armor.text = TextHelper.FormatStat(StatType.Armor, baseStats.Armor, modified.Armor);
        lifeSteal.text = TextHelper.FormatStat(StatType.LifeSteal, baseStats.LifeSteal, modified.LifeSteal);
        lifeStealRate.text = TextHelper.FormatStat(StatType.LifeStealRate, baseStats.LifeStealRate, modified.LifeStealRate);
        critChance.text = TextHelper.FormatStat(StatType.CritChance, baseStats.CritChance, modified.CritChance);
        critMultiplier.text = TextHelper.FormatStat(StatType.CritDamage, baseStats.CritMultiplier, modified.CritMultiplier);
    }


    private StatType PickRandomStatType()
    {
        var dict = System.Enum.GetValues(typeof(StatType))
            .Cast<StatType>()
            .ToDictionary(s => s, s => 1f); // equal weight

        var picker = new WeightedRandomPicker<StatType>(dict);
        return picker.GetRandomItem(rng);
    }
    private void Reroll()
    {
        // clear slot cũ
        currentSlots.Clear();
        foreach (Transform child in upgradeSlotParent)
        {
            Destroy(child.gameObject);
        }

        // pick random 3 option
        for (int i = 0; i < 3; i++)
        {
            var statType = PickRandomStatType();
            var rarity = PickRandomRarity();

            var config = statUpgradeConfig.GetConfig(statType, rarity);

            if (config != null)
            {
                var slot = Instantiate(UpgradeSlotPrefab, upgradeSlotParent);
                slot.Setup(config, statType);
                slot.OnUpgradeClicked += (stat, value) =>
                {
                    OnUpgradeClicked(stat, value);
                    Destroy(slot.gameObject);
                };
                currentSlots.Add(slot);
            }
        }
    }

    private void OnUpgradeClicked(StatType statType, float value)
    {
        PlayerStats.Instance.ChangeStat(statType, value);

    }

    private Rarity PickRandomRarity()
    {
        var dict = new Dictionary<Rarity, float>
        {
            { Rarity.Common, 50f },
            { Rarity.Rare, 30f },
            { Rarity.Epic, 15f },
            { Rarity.Legendary, 5f },
        };

        var picker = new WeightedRandomPicker<Rarity>(dict);
        return picker.GetRandomItem(rng);
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
        if (active)
        {
            _canvasGroup.DOFade(1, 0.5f);
        }
        else
        {
            _canvasGroup.DOFade(0, 0.5f);
        }


    }


    private System.Collections.IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        while (_canvasGroup.alpha < 1f)
        {
            _canvasGroup.alpha += Time.deltaTime * 2f; // 0.5s duration
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha -= Time.deltaTime * 2f; // 0.5s duration
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        _canvas.enabled = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ActiveCanvas(!_canvas.enabled);
        }
    }

}
