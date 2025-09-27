using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealthBar, IDamageable
{
    public static PlayerStats Instance { get; private set; }

    [Header("Currency")]
    [SerializeField] private ObservableValue<int> coin = new(0);
    public ObservableValue<int> Coin => coin;

    [SerializeField, Range(0f, 1f)] private ObservableValue<float> coinDropChance = new(0.1f);
    public ObservableValue<float> CoinDropChance => coinDropChance;

    [Header("Health")]
    [SerializeField] private ObservableValue<float> maxHealth = new(100f);
    public ObservableValue<float> MaxHealth => maxHealth;

    [SerializeField] private ObservableValue<float> currentHealth = new(100f);
    public ObservableValue<float> CurrentHealth => currentHealth;

    [Header("Movement")]
    [SerializeField] private ObservableValue<float> moveSpeed = new(5f);
    public ObservableValue<float> MoveSpeed => moveSpeed;

    [Header("Combat")]
    [SerializeField] private ObservableValue<float> meleeDamage = new(10f);
    public ObservableValue<float> MeleeDamage => meleeDamage;

    [SerializeField] private float attackSpeed = 1f; // fixed, not observable
    public float AttackSpeed => attackSpeed;

    [SerializeField] private float attackRange = 1.5f; // fixed, not observable
    public float AttackRange => attackRange;

    [SerializeField, Range(0f, 1f)] private ObservableValue<float> critChance = new(0f);
    public ObservableValue<float> CritChance => critChance;

    [SerializeField] private ObservableValue<float> critMultiplier = new(2f);
    public ObservableValue<float> CritMultiplier => critMultiplier;

    [Header("Defense")]
    [SerializeField] private ObservableValue<float> armor = new(0f); // flat damage reduction
    public ObservableValue<float> Armor => armor;

    [SerializeField, Range(0f, 1f)] private ObservableValue<float> dodgeChance = new(0f);
    public ObservableValue<float> DodgeChance => dodgeChance;

    [Header("Other")]
    [SerializeField] private ObservableValue<float> lifeSteal = new(0f); // % heal by dmg dealt
    public ObservableValue<float> LifeSteal => lifeSteal;

    [SerializeField, Range(0f, 1f)] private ObservableValue<float> lifeStealRate = new(0f);
    public ObservableValue<float> LifeStealRate => lifeStealRate;

    [SerializeField] private ObservableValue<float> hpRegen = new(1f);
    public ObservableValue<float> HpRegen => hpRegen;

    private UIHealthBar _healthBar;
    private float timer;
    private float regenInterval = 1f;
    public List<SkillID> AllSkills = new List<SkillID>();
    public List<Skill_Base> SkillInstances = new List<Skill_Base>();

    private KeyCode Skill_1 = KeyCode.J;
    private KeyCode Skill_2 = KeyCode.K;



    public event Action OnDataInit;


    private void RegisterAllSkills()
    {
        InputManager.Instance.RegisterKeyDown(Skill_1, () => UseSkill(Skill_1));
        InputManager.Instance.RegisterKeyDown(Skill_2, () => UseSkill(Skill_2));
        for (int i = 0; i < AllSkills.Count; i++)
        {
            SkillID skillID = AllSkills[i];
            AbilitiesSO skillData = GameDataManager.Instance.AllSkills.Find(s => s.skillID == skillID);
            if (skillData != null)
            {
                Skill_Base skillInstance = SkillFactory.CreateSkill(skillData);
                if (skillInstance != null)
                {
                    SkillInstances.Add(skillInstance);
                }
                else
                {
                    Debug.LogWarning($"Failed to create skill instance for {skillID}");
                }
            }
            else
            {
                Debug.LogWarning($"Skill data not found for {skillID}");
            }
        }
    }

    private void UpdateSkillCooldowns(float deltaTime)
    {
        foreach (var skill in SkillInstances)
        {
            skill.UpdateCooldown(deltaTime);
        }
    }

    private void UseSkill(KeyCode key)
    {
        int index = -1;
        if (key == Skill_1) index = 0;
        else if (key == Skill_2) index = 1;

        if (index >= 0 && index < SkillInstances.Count)
        {
            Skill_Base skill = SkillInstances[index];
            if (skill.TryUse())
            {
                Debug.Log($"Used skill: {skill.SkillData.skillName}");
            }
            else
            {
            }
        }
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    private void Start()
    {
        RegisterHealthBar(transform, MaxHealth.Value);
        RegisterAllSkills();
        OnDataInit?.Invoke();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= regenInterval)
        {
            timer = 0f;
            HandleRegen(HpRegen.Value);
        }
        UpdateSkillCooldowns(Time.deltaTime);
    }

    public void RegisterHealthBar(Transform target, float maxHealth)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth);
    }

    public void SetHealth(float health, float maxHealth)
    {
        _healthBar.SetHealth(health, maxHealth);
    }



    public void TakeDamage(float amount)
    {
        DamageResult result = DamageHelper.CalculateDamage(
            amount,
            CritChance.Value,
            CritMultiplier.Value,
            Armor.Value,
            DodgeChance.Value,
            LifeStealRate.Value
        );

        TakeDamage(result, transform.position);
    }

    public DamageResult DealDamage()
    {
        DamageResult result = DamageHelper.CalculateDamage(
            MeleeDamage.Value,
            CritChance.Value,
            CritMultiplier.Value,
            0, // target armor
            0, // target dodge
            LifeStealRate.Value
        );

        if (result.IsLifeSteal)
        {
            float healAmount = result.FinalDamage * LifeSteal.Value;
            Heal(healAmount);
        }
        return result;
    }

    public void Heal(float amount)
    {
        Debug.Log($"Healing for {amount}");
        CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + amount, MaxHealth.Value);
        SetHealth(CurrentHealth.Value, MaxHealth.Value);

        UIDamageTextManager.Instance.ShowDamageText(transform.position, amount, TextType.Heal);

        var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Heal);
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.Play();
    }

    private void HandleRegen(float amount) // no show text
    {
        CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + amount, MaxHealth.Value);
        SetHealth(CurrentHealth.Value, MaxHealth.Value);
    }


    public void UnregisterHealthBar()
    {
        UIHealthBarController.Instance.UnregisterHealthBar(_healthBar);
    }



    private void OnDestroy()
    {
    }

    public void TakeDamage(DamageResult result, Vector3 hitPoint)
    {
        if (result.IsMiss)
        {
            UIDamageTextManager.Instance.ShowDamageText(transform.position, 0, TextType.Miss);
            Debug.Log("Player dodged the attack!");
            return;
        }

        CurrentHealth.Value -= result.FinalDamage;
        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value, 0);
        SetHealth(CurrentHealth.Value, MaxHealth.Value);

        UIDamageTextManager.Instance.ShowDamageText(
            transform.position,
            result.FinalDamage,
            result.IsCrit ? TextType.Critical : TextType.Normal
        );
    }

    public float GetCurrentHealth() => CurrentHealth.Value;



    public float GetMaxHealth() => MaxHealth.Value;

}
