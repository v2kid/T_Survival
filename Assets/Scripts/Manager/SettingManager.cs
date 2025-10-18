using UnityEngine;
[System.Serializable]
public class AudioSettings
{
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    public void ApplySettings()
    {
        // Apply audio settings through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(masterVolume);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }
    }
}

[System.Serializable]
public class GraphicsSettings
{
    public enum ScreenMode { Fullscreen, Windowed, Borderless }
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int refreshRate = 60;

    public void ApplySettings()
    {
        Screen.SetResolution(resolutionWidth, resolutionHeight, FullScreenMode.FullScreenWindow, refreshRate);
        QualitySettings.vSyncCount = 1; // Enable VSync
        QualitySettings.antiAliasing = 4; // 2x MSAA
        QualitySettings.shadows = ShadowQuality.All; // Enable shadows
    }
}

[System.Serializable]
public class ControlSettings
{
    public KeyBinding[] keyBindings;

    public bool IsKeyBindingUsed(KeyCode key)
    {
        foreach (var binding in keyBindings)
        {
            if (binding.key == key) return true;
        }
        return false;
    }



    public void ApplySettings()
    {

    }
}
[System.Serializable]
public class KeyBinding
{
    public string actionName;
    public KeyCode key;
    public KeyAction action;
}

public enum KeyAction
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Jump,
    Sprint,
    LightAttack,
    HeavyAttack,
    Skill1,
    Skill2,
    Skill3
}

public class SettingManager : MonoBehaviour, ISaveable<PlayerSetting>
{
    public static SettingManager Instance;

    public PlayerSetting playerSetting;

    public event System.Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }



    }

    private void Start()
    {
        SaveRegistry.Register<PlayerSetting>(this);
        playerSetting = SaveManager.Instance.LoadData<PlayerSetting>();
        if (playerSetting == null)
        {
            playerSetting = new PlayerSetting();
        }
    }
    public void ApplyAllSettings()
    {
        playerSetting.audioSettings.ApplySettings();
        playerSetting.graphicsSettings.ApplySettings();
        playerSetting.controlSettings.ApplySettings();
        OnSettingsChanged?.Invoke();
    }

    public PlayerSetting GetSaveData()
    {
        return playerSetting;
    }



    public void SetData(PlayerSetting data)
    {
        this.playerSetting = data;
    }
}
[System.Serializable]
[SaveData("PlayerSettings.json")]
public class PlayerSetting
{
    public AudioSettings audioSettings;
    public GraphicsSettings graphicsSettings;
    public ControlSettings controlSettings;
    public PlayerSetting()
    {
        audioSettings = new AudioSettings();
        graphicsSettings = new GraphicsSettings();
        controlSettings = new ControlSettings();
        var n = new KeyBinding[]
        {
            // new KeyBinding { actionName = "Jump", key = KeyCode.Space, action = KeyAction.Jump },
            // new KeyBinding { actionName = "Sprint", key = KeyCode.LeftShift, action = KeyAction.Sprint },
            new KeyBinding { actionName = "Light Attack", key = KeyCode.Mouse0, action = KeyAction.LightAttack },
            new KeyBinding { actionName = "Heavy Attack", key = KeyCode.Mouse1, action = KeyAction.HeavyAttack },
            new KeyBinding { actionName = "Skill 1", key = KeyCode.J, action = KeyAction.Skill1 },
            new KeyBinding { actionName = "Skill 2", key = KeyCode.K, action = KeyAction.Skill2 },
            new KeyBinding { actionName = "Skill 3", key = KeyCode.Alpha3, action = KeyAction.Skill3 },

        };
        controlSettings.keyBindings = n;
    }
}