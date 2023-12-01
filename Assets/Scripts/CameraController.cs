using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private AgentController _agentController;

    [SerializeField] private CinemachineVirtualCamera _cinemachine;

    private void LateUpdate()
    {
        if (_cinemachine.Follow == null || _cinemachine.LookAt == null)
        {
            Enemy newAgent = _agentController.RandomAgent;

            _cinemachine.Follow = newAgent.transform;
            _cinemachine.LookAt = newAgent.transform;
        }
    }
}
