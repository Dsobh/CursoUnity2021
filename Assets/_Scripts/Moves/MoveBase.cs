using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string name;
    public string Name => name;
    [TextArea] [SerializeField] private string description;
    public string Description => description;
    [SerializeField] private PokemonType type;
    public PokemonType Type => type;
    [SerializeField] private int power;
    public int Power => power;
    [SerializeField] private int accuracy;
    public int Accuracy => accuracy;
    [SerializeField] private bool alwaysHit;
    public bool AlwaysHit => alwaysHit;
    [SerializeField] private int pp;
    public int PP => pp;
    [SerializeField] private int priority;
    public int Priority => priority;
    [SerializeField] MoveType moveType;
    public MoveType MoveType => moveType;
    [SerializeField] private MoveStatEffect effects;
    [SerializeField] private MoveTarget target;
    public MoveStatEffect Effects => effects;
    public MoveTarget Target => target;
    [SerializeField] private List<SecondaryMoveStatEffect> secondaryEffects;
    public List<SecondaryMoveStatEffect> SecondaryEffects => secondaryEffects;

    public bool isSpecialMove => moveType == MoveType.Special;

    /*if(type == PokemonType.Fuego || type == PokemonType.Agua || type == PokemonType.Planta || type == PokemonType.Hielo 
        || type == PokemonType.Electrico || type == PokemonType.Dragon || type == PokemonType.Dark || type == PokemonType.Psiquico)
    {
        return true;
    }else
    {
        return false;
    }*/

}


public enum MoveType
{
    Physical,
    Special,
    Stats //Modifica estadísticas de un pokemon
}

[System.Serializable]
public class MoveStatEffect
{
    [SerializeField] List<StatBoosting> boostings;
    [SerializeField] StatusConditionID status;
    [SerializeField] StatusConditionID volatileStatus;

    public List<StatBoosting> Boostings => boostings;
    public StatusConditionID Status => status;
    public StatusConditionID VolatileStatus => volatileStatus;
}

[System.Serializable]
public class SecondaryMoveStatEffect : MoveStatEffect //Herencia
{
    [SerializeField] private int chance; //probabilidad de que ocurra el efecto
    [SerializeField] private MoveTarget target;

    public int Chance
    {
        get => chance;
    }
    public MoveTarget Target
    {
        get => target;
    }
}

[System.Serializable]
public class StatBoosting
{
    public Stat stat;
    public int boost;
    public MoveTarget target;
}

public enum MoveTarget
{
    Self, Other
}