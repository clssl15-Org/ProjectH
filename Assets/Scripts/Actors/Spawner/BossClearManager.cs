using System;
using System.Collections.Generic;
using Actors;
using UnityEngine;

public class BossClearManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] bossMonsters;

    [SerializeField]
    private GameObject clearObject;

    private readonly List<(IMonster monster, Action handler)> _subscriptions = new();
    private int _remainingAlive;

    private void OnEnable()
    {
        ClearSubscriptions();

        if (bossMonsters == null || bossMonsters.Length == 0)
            return;

        foreach (var go in bossMonsters)
        {
            if (go == null)
                continue;
            if (!go.TryGetComponent<IMonster>(out var monster))
                continue;

            if (!monster.IsAlive)
                continue;

            _remainingAlive++;
            IMonster captured = monster;
            Action handler = null;
            handler = () =>
            {
                captured.Destroyed -= handler;
                _remainingAlive--;
                if (_remainingAlive <= 0)
                    ShowClearObject();
            };
            captured.Destroyed += handler;
            _subscriptions.Add((captured, handler));
        }

        if (_remainingAlive == 0 && HasAnyBossReference())
            ShowClearObject();
    }

    private void OnDisable()
    {
        ClearSubscriptions();
    }

    private bool HasAnyBossReference()
    {
        foreach (var go in bossMonsters)
        {
            if (go == null)
                continue;
            if (go.GetComponent<IMonster>() != null)
                return true;
        }

        return false;
    }

    private void ClearSubscriptions()
    {
        foreach (var (monster, handler) in _subscriptions)
        {
            if (monster == null)
                continue;
            var mb = monster as MonoBehaviour;
            if (mb != null)
                monster.Destroyed -= handler;
        }

        _subscriptions.Clear();
        _remainingAlive = 0;
    }

    private void ShowClearObject()
    {
        if (clearObject != null)
            clearObject.SetActive(true);
    }
}
