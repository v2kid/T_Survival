using UnityEngine;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private Dictionary<KeyCode, Action> keyDownActions = new();
    // private Dictionary<KeyCode, Action> keyUpActions = new();
    // private Dictionary<KeyCode, Action> keyHoldActions = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // giữ lại khi đổi scene
    }

    private void Update()
    {
        foreach (var kvp in keyDownActions)
        {
            if (Input.GetKeyDown(kvp.Key))
                kvp.Value?.Invoke();
        }

        // foreach (var kvp in keyHoldActions)
        // {
        //     if (Input.GetKey(kvp.Key))
        //         kvp.Value?.Invoke();
        // }

        // foreach (var kvp in keyUpActions)
        // {
        //     if (Input.GetKeyUp(kvp.Key))
        //         kvp.Value?.Invoke();
        // }
    }

    #region Register / Unregister
    public void RegisterKeyDown(KeyCode key, Action action)
    {
        if (!keyDownActions.ContainsKey(key))
            keyDownActions[key] = action;
        else
            keyDownActions[key] += action;
    }

    // public void RegisterKeyUp(KeyCode key, Action action)
    // {
    //     if (!keyUpActions.ContainsKey(key))
    //         keyUpActions[key] = action;
    //     else
    //         keyUpActions[key] += action;
    // }

    // public void RegisterKeyHold(KeyCode key, Action action)
    // {
    //     if (!keyHoldActions.ContainsKey(key))
    //         keyHoldActions[key] = action;
    //     else
    //         keyHoldActions[key] += action;
    // }

    public void UnregisterKeyDown(KeyCode key)
    {
        if (keyDownActions.ContainsKey(key))
            if (keyDownActions[key] == null) keyDownActions.Remove(key);
    }

    // public void UnregisterKeyUp(KeyCode key, Action action)
    // {
    //     if (keyUpActions.ContainsKey(key))
    //     {
    //         keyUpActions[key] -= action;
    //         if (keyUpActions[key] == null) keyUpActions.Remove(key);
    //     }
    // }

    // public void UnregisterKeyHold(KeyCode key, Action action)
    // {
    //     if (keyHoldActions.ContainsKey(key))
    //     {
    //         keyHoldActions[key] -= action;
    //         if (keyHoldActions[key] == null) keyHoldActions.Remove(key);
    //     }
    // }
    #endregion
}



