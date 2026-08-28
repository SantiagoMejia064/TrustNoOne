using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BunkerEventRandomizer : MonoBehaviour
{
    private enum RandomEventType
    {
        ExteriorContamination,
        ElectricalOverload,
        DuctContamination
    }

    [Serializable]
    private class RandomEventConfig
    {
        public RandomEventType eventType;
        public bool enabled = true;
        [FormerlySerializedAs("toggleIntervalRange")]
        public Vector2 activationIntervalRange = new Vector2(20f, 45f);
        public Vector2 activeDurationRange = new Vector2(8f, 15f);
        [NonSerialized] public float nextToggleTime;
    }

    [SerializeField] private BunkerStateManager bunkerStateManager;
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private List<RandomEventConfig> randomEvents = new List<RandomEventConfig>();

    private void Start()
    {
        foreach (RandomEventConfig eventConfig in randomEvents)
        {
            SetEventState(eventConfig.eventType, false);
            ScheduleNextChange(eventConfig, eventConfig.activationIntervalRange);
        }
    }

    private void Update()
    {
        if (!startAutomatically || bunkerStateManager == null)
        {
            return;
        }

        foreach (RandomEventConfig eventConfig in randomEvents)
        {
            if (!eventConfig.enabled || Time.time < eventConfig.nextToggleTime)
            {
                continue;
            }

            bool activateEvent = !IsEventActive(eventConfig.eventType);
            SetEventState(eventConfig.eventType, activateEvent);

            Vector2 nextRange = activateEvent
                ? eventConfig.activeDurationRange
                : eventConfig.activationIntervalRange;

            ScheduleNextChange(eventConfig, nextRange);
        }
    }

    private bool IsEventActive(RandomEventType eventType)
    {
        switch (eventType)
        {
            case RandomEventType.ExteriorContamination:
                return bunkerStateManager.IsExteriorContaminationActive();
            case RandomEventType.ElectricalOverload:
                return bunkerStateManager.IsElectricalOverloadActive();
            case RandomEventType.DuctContamination:
                return bunkerStateManager.IsDuctContaminationActive();
            default:
                return false;
        }
    }

    private void SetEventState(RandomEventType eventType, bool isActive)
    {
        if (bunkerStateManager == null)
        {
            Debug.LogWarning("No se puede alternar el evento porque falta BunkerStateManager.");
            return;
        }

        switch (eventType)
        {
            case RandomEventType.ExteriorContamination:
                bunkerStateManager.SetExteriorContaminationActive(isActive);
                break;
            case RandomEventType.ElectricalOverload:
                bunkerStateManager.SetElectricalOverloadActive(isActive);
                break;
            case RandomEventType.DuctContamination:
                bunkerStateManager.SetDuctContaminationActive(isActive);
                break;
        }
    }

    private void ScheduleNextChange(RandomEventConfig eventConfig, Vector2 intervalRange)
    {
        float minInterval = Mathf.Max(0.1f, Mathf.Min(intervalRange.x, intervalRange.y));
        float maxInterval = Mathf.Max(minInterval, Mathf.Max(intervalRange.x, intervalRange.y));
        eventConfig.nextToggleTime = Time.time + UnityEngine.Random.Range(minInterval, maxInterval);
    }
}
