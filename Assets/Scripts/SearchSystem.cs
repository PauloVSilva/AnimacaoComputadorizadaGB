using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchSystem : MonoBehaviour
{
    [SerializeField] private bool _seekIsNull;

    [SerializeField] private float _searchRadius;

    [SerializeField] private List<GameObject> _otherTargetsGameObjects;

    public float SearchRadius
    {
        get
        {
            return _searchRadius;
        }
        set
        {
            _searchRadius = value;
        }
    }

    private Coroutine _seekNewTarget;

    public event System.Action<MonoBehaviour> OnTargetFound;
    public event System.Action OnCouldNotFindTarget;

    private void Awake()
    {
        _seekNewTarget = null;
    }

    private void Update()
    {
        _seekIsNull = _seekNewTarget == null;
    }



    public void InitiateSearch<T>(int attempts, float delay) where T : MonoBehaviour
    {
        if (_seekNewTarget != null)
        {
            StopCoroutine(_seekNewTarget);
        }

        _seekNewTarget = null;

        _seekNewTarget = StartCoroutine(LookForTargetInterval<T>(attempts, delay));
    }


    private IEnumerator LookForTargetInterval<T>(int attempts, float delay) where T : MonoBehaviour
    {
        yield return new WaitForSeconds(delay);

        if (LookForTarget<T>() == null)
        {
            attempts--;

            Debug.Log("Attemps left: " + attempts);

            if (attempts > 0)
            {
                _seekNewTarget = StartCoroutine(LookForTargetInterval<T>(attempts, delay));
            }
            else
            {
                Debug.Log("Could not find target " + LookForTarget<T>());

                OnCouldNotFindTarget?.Invoke();

                if (_seekNewTarget != null)
                {
                    StopCoroutine(_seekNewTarget);
                }

                _seekNewTarget = null;
            }
        }
        else
        {
            Debug.Log("Target found " + LookForTarget<T>());

            if (_seekNewTarget != null)
            {
                StopCoroutine(_seekNewTarget);
            }

            _seekNewTarget = null;
        }
    }

    private T LookForTarget<T>() where T : MonoBehaviour
    {
        T newTarget = FindTarget<T>();

        if (newTarget != null)
        {
            OnTargetFound?.Invoke(newTarget);
        }

        return newTarget;
    }

    private T FindTarget<T>() where T : MonoBehaviour
    {
        _otherTargetsGameObjects.Clear();

        List<T> targets = FindObjectsOfType<T>().ToList();
        List<T> otherTargets = targets.Where(target => target.gameObject != gameObject && !IsSameTeam(target)).ToList();

        foreach (T target in otherTargets)
        {
            _otherTargetsGameObjects.Add(target.gameObject);
        }

        if (otherTargets.Count > 0)
        {
            GameObject nearestObject = FindNearestTarget(otherTargets.ToList());

            if (nearestObject != null)
            {
                return nearestObject.GetComponent<T>();
            }
        }

        return null;
    }

    private bool IsSameTeam(MonoBehaviour target)
    {
        // Implement logic to check if the target is on the same team
        // You need to have a way to determine the team of the current object and the target
        // Replace the following line with your actual team-checking logic
        return target.GetComponent<Enemy>()?.Team == GetComponent<Enemy>()?.Team;
    }

    private GameObject FindNearestTarget<T>(List<T> list) where T : MonoBehaviour
    {
        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (T item in list)
        {
            if (item.gameObject == gameObject) continue;

            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance < nearestDistance && distance <= _searchRadius)
            {
                nearest = item.gameObject;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
