using Maskborn;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealthBar
{
    public static PlayerStats Instance { get; private set; }

    [Header("Level & EXP")]

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Combat")]
    public float meleeDamage = 10f;
    public float attackSpeed = 1f; // đòn/s
    public float attackRange = 1.5f;

    [Range(0f, 1f)] public float critChance = 0f;   // 0–1  
    public float critMultiplier = 2f;

    [Header("Defense")]
    public float armor = 0f;                        // giảm sát thương phẳng
    [Range(0f, 1f)] public float dodgeChance = 0f;  // 0–1

    [Header("Other")]
    public float lifeSteal = 0f;               // lượng máu hồi theo % sát thương
    [Range(0f, 1f)] public float lifeStealRate = 0f; // 0–1
    public float hpRegen = 1f;

    private UIHealthBar _healthBar;
    private float timer;
    private float regenInterval = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        RegisterHealthBar(transform, maxHealth);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= regenInterval)
        {
            timer = 0f;
            HandleRegen(hpRegen);
        }
    }

    private void OnDestroy()
    {
    }



 

    public void RegisterHealthBar(Transform target, float maxHealth)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth);
    }

    public void SetHealth(float health, float maxHealth)
    {
        _healthBar.SetHealth(health, maxHealth);
    }

    public void UnregisterHealthBar(UIHealthBar healthBar)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float amount)
    {
        DamageResult result = DamageHelper.CalculateDamage(
            amount,
            critChance,
            critMultiplier,
            armor,
            dodgeChance,
            lifeStealRate
        );

        if (result.IsMiss)
        {
            UIDamageTextManager.Instance.ShowDamageText(transform.position, 0, TextType.Miss);
            Debug.Log("Player dodged the attack!");
            return;
        }

        currentHealth -= result.FinalDamage;
        currentHealth = Mathf.Max(currentHealth, 0);
        SetHealth(currentHealth, maxHealth);
        UIDamageTextManager.Instance.ShowDamageText(
            transform.position,
            result.FinalDamage,
            result.IsCrit ? TextType.Critical : TextType.Normal
        );
    }

    public DamageResult DealDamage()
    {
        DamageResult result = DamageHelper.CalculateDamage(
            meleeDamage,
            critChance,
            critMultiplier,
            0, // target armor
            0, // target dodge
            lifeStealRate
        );
        if (result.IsLifeSteal)
        {
            float healAmount = result.FinalDamage * lifeSteal;
            Heal(healAmount);
        }
        return result;
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        SetHealth(currentHealth, maxHealth);
        UIDamageTextManager.Instance.ShowDamageText(transform.position, amount, TextType.Heal);
        var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Heal);
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.Play();
    }

    private void HandleRegen(float amount) //no show text
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        SetHealth(currentHealth, maxHealth);
    }
}
