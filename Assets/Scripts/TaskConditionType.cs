public enum TaskConditionType
{
    AlwaysTrue,
    AlwaysFalse,

    OxygenIsStable,
    OxygenIsLowOrWorse,
    OxygenIsCriticalOrWorse,

    EnergyIsStable,
    EnergyIsLowOrWorse,
    EnergyIsCriticalOrWorse,

    CommunicationsAreStable,
    CommunicationsAreLowOrWorse,
    CommunicationsAreCriticalOrWorse,

    AnySystemIsLowOrWorse,
    AnySystemIsCriticalOrWorse,

    ExteriorContaminationIsActive,
    ExteriorContaminationIsInactive,

    ElectricalOverloadIsActive,
    ElectricalOverloadIsInactive,

    AuxiliaryGeneratorIsAvailable
}
