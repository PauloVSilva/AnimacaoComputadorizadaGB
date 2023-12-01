using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedKit : MonoBehaviour
{
    [SerializeField, Min(1)] private int _healPower = 1;

    public event System.Action OnEntityHealed;


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthSystem healthSystem))
        {
            HealEntity(healthSystem);

            //OnEntityHealed?.Invoke();

            Destroy(gameObject);
        }

    }


    private void HealEntity(HealthSystem healthSystem)
    {
        healthSystem.Heal(_healPower);
    }
}
