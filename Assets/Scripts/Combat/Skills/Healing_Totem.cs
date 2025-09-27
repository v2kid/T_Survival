using UnityEngine;

public class Healing_Totem : Skill_Base
{
    private AreaEffectConfig config;
    public Healing_Totem(AbilitiesSO skillData) : base(skillData)
    {
        config = new AreaEffectConfig
        {
            duration = 8f,
            interval = 1f,
            activationDelay = 0.5f
        };
    }

    protected override void OnUse()
    {

        base.OnUse();
        BaseVisualEffect vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Healing_Totem);
        vfx.transform.position = PlayerStats.Instance.transform.position;
        vfx.Play();
        HealingArea area = vfx.GetComponent<HealingArea>();
        if (area != null)
        {
            area.Initialize(config);
        }
        else
        {
            area = vfx.gameObject.AddComponent<HealingArea>();
            area.Initialize(config);
        }


    }


}