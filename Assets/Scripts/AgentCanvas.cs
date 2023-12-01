using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentCanvas : MonoBehaviour
{
    [SerializeField] private HealthSystem _healthSystem;
    [SerializeField] private Enemy _agent;

    [SerializeField] private Slider _healthBar;
    [SerializeField] private TextMeshProUGUI _agentState;

    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);

        _healthBar.minValue = 0;
        _healthBar.maxValue = _healthSystem.MaxHealth;
        _healthBar.value = _healthSystem.Health;

        _agentState.text = _agent.State.ToString();
    }
}
