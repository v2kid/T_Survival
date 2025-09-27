using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealingArea : MonoBehaviour
{
    public float healingAmount = 5f;
    public float healingInterval = 1f;
    public LayerMask playerLayer; // Set this in the inspector to the Player layer

    private float timer = 0f;
    private SphereCollider sphereCollider;

    private void Awake()
    {
        // Add or get SphereCollider, set as trigger and set radius
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }


    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= healingInterval)
        {
            timer = 0f;
            PlayerStats.Instance.Heal(healingAmount);
        }
    }


}