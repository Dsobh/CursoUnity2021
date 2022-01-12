using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;

[Serializable]
public class Pokemon
{
    [SerializeField] private PokemonBase _base;
    public PokemonBase Base => _base;

    [SerializeField] private int _level;
    public int Level => _level;

    //TODO: private string genero;
    private int _hp;


    //Vida actual del pokemon
    public int HP
    {
        get => _hp;
        set
        {
            _hp = value;
            _hp = Mathf.FloorToInt(Mathf.Clamp(_hp, 0, MaxHP)); //Para no pasarnos
        }
    }

    private int _exp;
    public int Exp
    {
        get => _exp;
        set => _exp = value;
    }

    private List<Move> _moves;
    public List<Move> Moves => _moves;
    public Move CurrentMove {get; set;}

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;
        InitPokemon();
    }

    public Dictionary<Stat, int> Stats { get; private set; } //Cualquier script puede acceder pero solo se puede setear desde esta clase.
    public Dictionary<Stat, int> StatsBoosted { get; private set; } //Valores de mejora de cada stat

    public StatusCondition StatusCondition {get; set;}
    public int statusNumberTurns {get; set;}

    public StatusCondition VolatileStatusCondition {get; set;}
    public int VolatileStatusNumberTurns {get; set;}

    public Queue<string> StatusChangeMessages {get; private set;} = new Queue<string>();
    public event Action OnStatusConditionChanged;

    public bool hasHPChanged{get; set;} = false;

    public int previousHPValue;

    public void InitPokemon()
    {
        _exp = Base.GetNecessaryExpForLevel(_level);

        _moves = new List<Move>();

        foreach (var lMove in _base.LearnableMoves)
        {
            if (lMove.Level <= _level)
            {
                _moves.Add(new Move(lMove.Move));
            }

            if (_moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
            {
                break;
            }
        }

        CalculateStats();
        _hp = MaxHP;
        previousHPValue = MaxHP;
        hasHPChanged = true;
        ResetBoostings();
        StatusCondition = null;
        VolatileStatusCondition  = null;
    }

    void ResetBoostings()
    {
        StatusChangeMessages = new Queue<string>();
        
        StatsBoosted = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    void CalculateStats()
    {
        //TODO: Establecer las formulas correctas -> iv, ev, stats base y naturaleza
        //5+(nv /100 * ((statBase *2) +IV + EV))
        //510+(nv /100 * ((statBase *2) +IV + EV)) + nv
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((_base.Attack * _level) / 100.0f) + 1);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((_base.Defense * _level) / 100.0f) + 1);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((_base.SpAttack * _level) / 100.0f) + 1);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((_base.SpDefense * _level) / 100.0f) + 1);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((_base.Speed * _level) / 100.0f) + 1);

        MaxHP = Mathf.FloorToInt((_base.MaxHP * _level) / 100.0f) + 10;
    }

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        int boost = StatsBoosted[stat]; //desde -6 hasta 6

        //1 -> 1.5 -> 2 -> 2.5 -> ... -> 4

        float multiplier = (1.0f + Mathf.Abs(boost) / 2.0f);

        if (boost >= 0)
        {
            statValue = Mathf.FloorToInt(statValue * multiplier);
        }
        else
        {
            statValue = Mathf.FloorToInt(statValue / multiplier);
        }

        return statValue;
    }

    public void ApplyBoost(StatBoosting boost)
    {
        var stat = boost.stat;
        var value = boost.boost;

        StatsBoosted[stat] = Mathf.Clamp(StatsBoosted[stat] + value, -6, 6);

        if(value > 0)
        {
            StatusChangeMessages.Enqueue($"{Base.Name} ha incrementado su {stat}.");
        }
        else if(value < 0)
        {
            StatusChangeMessages.Enqueue($"{Base.Name} ha reducido su {stat}.");
        }
        else
        {
            StatusChangeMessages.Enqueue($"{Base.Name} no nota ningún cambio.");
        }
    }

    public int MaxHP { get; private set; }

    public int Attack => GetStat(Stat.Attack);
    public int Defense => GetStat(Stat.Defense);
    public int SpAttack => GetStat(Stat.SpAttack);
    public int SpDefense => GetStat(Stat.SpDefense);
    public int Speed => GetStat(Stat.Speed);


    public DamageDescription ReceiveDamage(Move move, Pokemon pokemonAttacker)
    {
        float critical = 1f;
        if (Random.Range(0, 100f) < 8)
        {
            critical = 2f;
        }

        float type1 = TypeMatrix.GetMultiplierEffectiveness(move.Base.Type, this.Base.Type1);
        float type2 = TypeMatrix.GetMultiplierEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDescription()
        {
            Critical = critical,
            Type = type1 * type2,
            Fainted = false
        };

        float attack = (move.Base.isSpecialMove ? pokemonAttacker.SpAttack : pokemonAttacker.Attack);
        float defense = (move.Base.isSpecialMove ? this.SpDefense : this.Defense);

        //TODO: añadir el resto de modificadores
        float modifiers = Random.Range(0.85f, 1.0f) * type1 * type2 * critical;
        float baseDamage = ((2 * pokemonAttacker._level / 5f + 2) * move.Base.Power * (attack / (float)defense)) / 50f + 2;
        int totalDamage = Mathf.FloorToInt(baseDamage * modifiers);

        UpdateHP(totalDamage);
        if(HP <= 0)
        {
            damageDetails.Fainted = true;
        }
        //TODO: falta añadir el estado fainted = true cuando me quedo sin vida

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        hasHPChanged = true;
        previousHPValue = HP;
        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
        }
    }

    public void SetConditionStatus(StatusConditionID id)
    {
        //Si se pueden tener varios estados alterados hay que iterar sobre una lista de estados
        if(StatusCondition != null)
        {
            return;
        }
        StatusCondition = StatusConditionFactory.StatusConditions[id];
        StatusCondition?.OnApplyStatusCondition?.Invoke(this);
        StatusChangeMessages.Enqueue($"{Base.Name} {StatusCondition.StartMessage}");
        OnStatusConditionChanged?.Invoke();
    }

    public void CureStatusCondition()
    {
        StatusCondition = null;
        OnStatusConditionChanged?.Invoke();
    }

    public void SetVolatileConditionStatus(StatusConditionID id)
    {
        //Si se pueden tener varios estados alterados hay que iterar sobre una lista de estados
        if(VolatileStatusCondition != null)
        {
            return;
        }
        VolatileStatusCondition = StatusConditionFactory.StatusConditions[id];
        VolatileStatusCondition?.OnApplyStatusCondition?.Invoke(this);
        StatusChangeMessages.Enqueue($"{Base.Name} {VolatileStatusCondition.StartMessage}");
    }

    public void CureVolatileStatusCondition()
    {
        VolatileStatusCondition = null;
    }

    public Move RandomMove()
    {

        var movesWithPP = Moves.Where(m => m.PP > 0).ToList();
        if (movesWithPP.Count > 0)
        {
            int randId = Random.Range(0, movesWithPP.Count);
            return movesWithPP[randId];
        }
        //No hay PPs en ningún ataque.
        //TODO: implementar combate que hace daño al enemigo y a ti mismo.
        return null;

    }

    public bool NeedsToLevelUp()
    {
        if (Exp > Base.GetNecessaryExpForLevel(_level + 1))
        {
            int currentMaxHP = this.MaxHP;
            _level++;
            HP += (this.MaxHP - currentMaxHP);
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(lm => lm.Level == this._level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove learnableMove)
    {
        if (Moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
        {
            return;
        }

        Moves.Add(new Move(learnableMove.Move));
    }

    public bool OnStartTurn()
    {
        bool canPerformMovement = true;
        
        if(StatusCondition?.OnStartTurn != null)
        {
            if(!StatusCondition.OnStartTurn(this))
            {
                canPerformMovement = false;
            }
        }

        if(VolatileStatusCondition?.OnStartTurn != null)
        {
            if(!VolatileStatusCondition.OnStartTurn(this))
            {
                canPerformMovement = false;
            }
        }

        return canPerformMovement;
        
    }

    public void OnFinishTurn()
    {
        StatusCondition?.OnFinishTurn?.Invoke(this); //No estamos seguros si el Método onFinisgTurn existe 
        VolatileStatusCondition?.OnFinishTurn?.Invoke(this);
    }

    public void OnBattleFinish()
    {
        CureVolatileStatusCondition(); //Limpiamos los estados volátiles
        ResetBoostings(); 
    }
}

//Igual es mejor usar un struct
public class DamageDescription
{
    public float Critical { get; set; }
    public float Type { get; set; }
    public bool Fainted { get; set; }
}