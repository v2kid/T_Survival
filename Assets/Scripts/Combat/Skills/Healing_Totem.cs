using UnityEngine;

public class Healing_Totem : Skill_Base
{
    public Healing_Totem(AbilitiesSO skillData) : base(skillData)
    {

    }

    protected override void OnUse()
    {
        base.OnUse();
        BaseVisualEffect vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Healing_Totem);
        vfx.transform.position = PlayerStats.Instance.transform.position;
        vfx.Play(5);

    }


}