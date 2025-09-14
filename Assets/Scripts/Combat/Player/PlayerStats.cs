using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealthBar
{
    public static PlayerStats Instance { get; private set; }

    [Header("Currency")]
    [SerializeField] private int coin = 0;
    public ObservableValue<int> CoinObservable { get; private set; }

    [SerializeField, Range(0f, 1f)] private float coinDropChance = 0.1f;
    public ObservableValue<float> CoinDropChance { get; private set; }

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    public ObservableValue<float> MaxHealth { get; private set; }

    public ObservableValue<float> CurrentHealth { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    public ObservableValue<float> MoveSpeed { get; private set; }

    [Header("Combat")]
    [SerializeField] private float meleeDamage = 10f;
    public ObservableValue<float> MeleeDamage { get; private set; }

    [SerializeField] private float attackSpeed = 1f;   // fixed, not observable
    public float AttackSpeed => attackSpeed;

    [SerializeField] private float attackRange = 1.5f; // fixed, not observable
    public float AttackRange => attackRange;

    [SerializeField, Range(0f, 1f)] private float critChance = 0f;
    public ObservableValue<float> CritChance { get; private set; }

    [SerializeField] private float critMultiplier = 2f;
    public ObservableValue<float> CritMultiplier { get; private set; }

    [Header("Defense")]
    [SerializeField] private float armor = 0f; // flat damage reduction
    public ObservableValue<float> Armor { get; private set; }

    [SerializeField, Range(0f, 1f)] private float dodgeChance = 0f;
    public ObservableValue<float> DodgeChance { get; private set; }

    [Header("Other")]
    [SerializeField] private float lifeSteal = 0f; // % heal by dmg dealt
    public ObservableValue<float> LifeSteal { get; private set; }

    [SerializeField, Range(0f, 1f)] private float lifeStealRate = 0f;
    public ObservableValue<float> LifeStealRate { get; private set; }

    [SerializeField] private float hpRegen = 1f;
    public ObservableValue<float> HpRegen { get; private set; }

    private UIHealthBar _healthBar;
    private float timer;
    private float regenInterval = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Khởi tạo observable từ Inspector
        CoinObservable = new ObservableValue<int>(coin);
        CoinDropChance = new ObservableValue<float>(coinDropChance);

        MaxHealth = new ObservableValue<float>(maxHealth);
        CurrentHealth = new ObservableValue<float>(maxHealth);

        MoveSpeed = new ObservableValue<float>(moveSpeed);

        MeleeDamage = new ObservableValue<float>(meleeDamage);
        CritChance = new ObservableValue<float>(critChance);
        CritMultiplier = new ObservableValue<float>(critMultiplier);

        Armor = new ObservableValue<float>(armor);
        DodgeChance = new ObservableValue<float>(dodgeChance);

        LifeSteal = new ObservableValue<float>(lifeSteal);
        LifeStealRate = new ObservableValue<float>(lifeStealRate);
        HpRegen = new ObservableValue<float>(hpRegen);
    }

    private void Start()
    {
        RegisterHealthBar(transform, MaxHealth.Value);
        CoinObservable.Value = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= regenInterval)
        {
            timer = 0f;
            HandleRegen(HpRegen.Value);
        }
    }

    public void RegisterHealthBar(Transform target, float maxHealth)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth);
    }

    public void SetHealth(float health, float maxHealth)
    {
        _healthBar.SetHealth(health, maxHealth);
    }

    public void AddCoin(int amount)
    {
        CoinObservable.Value += amount;
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
}
