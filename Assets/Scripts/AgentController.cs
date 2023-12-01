using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    [SerializeField] private List<Enemy> _agents = new();

    [SerializeField] private List<Enemy> _redTeam = new();
    [SerializeField] private List<Enemy> _blueTeam = new();


    public Enemy RandomAgent
    {
        get
        {
            int randomIndex = Random.Range(0, _agents.Count);

            return _agents[randomIndex];
        }
    }



    private void Start()
    {
        Init();
    }

    private void OnDestroy()
    {
        Finish();
    }

    private void Init()
    {
        foreach (Enemy agent in _agents)
        {
            agent.OnDied += UpdateList;
        }
    }

    private void Finish()
    {
        foreach (Enemy agent in _agents)
        {
            agent.OnDied -= UpdateList;
        }
    }

    private void AddToList(Enemy agent)
    {
        if (_agents.Contains(agent)) return;

        _agents.Add(agent);

        if (agent.Team == Team.Red)
        {
            _redTeam.Add(agent);
        }
        if (agent.Team == Team.Blue)
        {
            _blueTeam.Add(agent);
        }
    }

    private void UpdateList()
    {
        List<Enemy> newList = new();

        foreach (Enemy agent in _agents)
        {
            if (agent.ValidEnemy)
            {
                newList.Add(agent);
            }
        }

        _agents = newList;

        _redTeam.Clear();

        foreach (Enemy agent in _agents)
        {
            if (agent.Team == Team.Red)
            {
                _redTeam.Add(agent);
            }
        }

        _blueTeam.Clear();

        foreach (Enemy agent in _agents)
        {
            if (agent.Team == Team.Blue)
            {
                _blueTeam.Add(agent);
            }
        }


        CheckWinners();
    }

    private void CheckWinners()
    {
        if (_redTeam.Count == 0)
        {
            foreach (Enemy agent in _blueTeam)
            {
                agent.ChangeState(EnemyState.Dancing);
            }
        }
        if (_blueTeam.Count == 0)
        {
            foreach (Enemy agent in _redTeam)
            {
                agent.ChangeState(EnemyState.Dancing);
            }
        }
    }
}
