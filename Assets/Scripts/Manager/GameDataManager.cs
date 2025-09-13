using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public Dictionary<VisualEffectID, BaseVisualEffect> VisualEffectDictionary { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        LoadAllVisualEffect();
    }

    private void Start()
    {
    }

  


    private void LoadAllVisualEffect()
    {
        BaseVisualEffect[] vfxs = Resources.LoadAll<BaseVisualEffect>("Effects/");
        VisualEffectDictionary = new();
        for (int i = 0; i < vfxs.Length; i++)
        {
            BaseVisualEffect vfx = vfxs[i];
            vfx.Initialize();
            VisualEffectDictionary.Add(vfx.VfxID, vfx);
        }
    }
}