using UnityEngine;

public class Shuriken_Fan : Skill_Base
{
    public Shuriken_Fan(AbilitiesSO skillData) : base(skillData)
    {

    }

    protected override void OnUse()
    {
        base.OnUse();
        Debug.Log("Shuriken Fan used");
        BaseVisualEffect vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Shuriken_Fan);
        vfx.transform.position = PlayerStats.Instance.transform.position + new Vector3(0, 0.5f, 0);
        vfx.Play(3);

    }


}