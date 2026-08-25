using UnityEngine;
using UnityEngine.Events;

public class BunkerStateManager : MonoBehaviour
{
    [Header("Estados")]
    [SerializeField] private BunkerSystemState energyState = BunkerSystemState.Stable;
    [SerializeField] private BunkerSystemState oxygenState = BunkerSystemState.Stable;
    [SerializeField] private BunkerSystemState communicationsState = BunkerSystemState.Stable;
    [SerializeField] private bool exteriorContaminationActive;
    [SerializeField] private bool electricalOverloadActive;

    [Header("Eventos")]
    public UnityEvent onEnergyChanged = new UnityEvent();
    public UnityEvent onOxygenChanged = new UnityEvent();
    public UnityEvent onCommunicationsChanged = new UnityEvent();
    public UnityEvent onAnySystemCollapsed = new UnityEvent();
    public UnityEvent onExteriorContaminationChanged = new UnityEvent();
    public UnityEvent onElectricalOverloadChanged = new UnityEvent();

    public BunkerSystemState GetSystemState(BunkerSystemType systemType)
    {
        switch (systemType)
        {
            case BunkerSystemType.Energy:
                return energyState;
            case BunkerSystemType.Oxygen:
                return oxygenState;
            case BunkerSystemType.Communications:
                return communicationsState;
            default:
                Debug.LogWarning("Sistema de bunker no reconocido: " + systemType);
                return BunkerSystemState.Stable;
        }
    }

    public void SetSystemState(BunkerSystemType systemType, BunkerSystemState newState)
    {
        BunkerSystemState currentState = GetSystemState(systemType);

        if (currentState == newState)
        {
            return;
        }

        switch (systemType)
        {
            case BunkerSystemType.Energy:
                energyState = newState;
                onEnergyChanged?.Invoke();
                break;
            case BunkerSystemType.Oxygen:
                oxygenState = newState;
                onOxygenChanged?.Invoke();
                break;
            case BunkerSystemType.Communications:
                communicationsState = newState;
                onCommunicationsChanged?.Invoke();
                break;
            default:
                Debug.LogWarning("No se pudo cambiar el sistema de bunker: " + systemType);
                return;
        }

        if (newState == BunkerSystemState.Collapse)
        {
            onAnySystemCollapsed?.Invoke();
        }
    }

    public void ModifySystemState(BunkerSystemType systemType, int delta)
    {
        int currentLevel = (int)GetSystemState(systemType);
        int newLevel = Mathf.Clamp(currentLevel - delta, 0, 3);
        SetSystemState(systemType, (BunkerSystemState)newLevel);
    }

    public bool CheckCondition(TaskConditionType condition)
    {
        switch (condition)
        {
            case TaskConditionType.AlwaysTrue:
                return true;
            case TaskConditionType.AlwaysFalse:
                return false;

            case TaskConditionType.OxygenIsStable:
                return oxygenState == BunkerSystemState.Stable;
            case TaskConditionType.OxygenIsLowOrWorse:
                return IsLowOrWorse(oxygenState);
            case TaskConditionType.OxygenIsCriticalOrWorse:
                return IsCriticalOrWorse(oxygenState);

            case TaskConditionType.EnergyIsStable:
                return energyState == BunkerSystemState.Stable;
            case TaskConditionType.EnergyIsLowOrWorse:
                return IsLowOrWorse(energyState);
            case TaskConditionType.EnergyIsCriticalOrWorse:
                return IsCriticalOrWorse(energyState);

            case TaskConditionType.CommunicationsAreStable:
                return communicationsState == BunkerSystemState.Stable;
            case TaskConditionType.CommunicationsAreLowOrWorse:
                return IsLowOrWorse(communicationsState);
            case TaskConditionType.CommunicationsAreCriticalOrWorse:
                return IsCriticalOrWorse(communicationsState);

            case TaskConditionType.AnySystemIsLowOrWorse:
                return IsLowOrWorse(energyState) || IsLowOrWorse(oxygenState) || IsLowOrWorse(communicationsState);
            case TaskConditionType.AnySystemIsCriticalOrWorse:
                return IsCriticalOrWorse(energyState) || IsCriticalOrWorse(oxygenState) || IsCriticalOrWorse(communicationsState);
            case TaskConditionType.ExteriorContaminationIsActive:
                return exteriorContaminationActive;
            case TaskConditionType.ExteriorContaminationIsInactive:
                return !exteriorContaminationActive;
            case TaskConditionType.ElectricalOverloadIsActive:
                return electricalOverloadActive;
            case TaskConditionType.ElectricalOverloadIsInactive:
                return !electricalOverloadActive;
            default:
                Debug.LogWarning("Condicion de tarea no reconocida: " + condition);
                return false;
        }
    }

    public bool IsExteriorContaminationActive()
    {
        return exteriorContaminationActive;
    }

    public void SetExteriorContaminationActive(bool isActive)
    {
        if (exteriorContaminationActive == isActive)
        {
            return;
        }

        exteriorContaminationActive = isActive;
        onExteriorContaminationChanged?.Invoke();

        Debug.Log("Contaminacion exterior: " + (exteriorContaminationActive ? "ACTIVA" : "INACTIVA"));
    }

    public void ToggleExteriorContamination()
    {
        SetExteriorContaminationActive(!exteriorContaminationActive);
    }

    public bool IsElectricalOverloadActive()
    {
        return electricalOverloadActive;
    }

    public void SetElectricalOverloadActive(bool isActive)
    {
        if (electricalOverloadActive == isActive)
        {
            return;
        }

        electricalOverloadActive = isActive;
        onElectricalOverloadChanged?.Invoke();

        Debug.Log("Sobrecarga electrica entrante: " + (electricalOverloadActive ? "ACTIVA" : "INACTIVA"));
    }

    public void ToggleElectricalOverload()
    {
        SetElectricalOverloadActive(!electricalOverloadActive);
    }

    public bool IsCollapsed(BunkerSystemType systemType)
    {
        return GetSystemState(systemType) == BunkerSystemState.Collapse;
    }

    public void ResetStates()
    {
        SetSystemState(BunkerSystemType.Energy, BunkerSystemState.Stable);
        SetSystemState(BunkerSystemType.Oxygen, BunkerSystemState.Stable);
        SetSystemState(BunkerSystemType.Communications, BunkerSystemState.Stable);
        SetExteriorContaminationActive(false);
        SetElectricalOverloadActive(false);
    }

    private bool IsLowOrWorse(BunkerSystemState state)
    {
        return state >= BunkerSystemState.Low;
    }

    private bool IsCriticalOrWorse(BunkerSystemState state)
    {
        return state >= BunkerSystemState.Critical;
    }
}
