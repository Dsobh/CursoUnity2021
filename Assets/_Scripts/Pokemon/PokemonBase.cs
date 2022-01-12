using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PokemonType
{
    None,
    Normal,
    Fuego,
    Agua,
    Electrico,
    Planta,
    Hielo,
    Lucha,
    Veneno,
    Tierra,
    Volador,
    Psiquico,
    Bicho,
    Roca,
    Fantasma,
    Dragon,
    Dark,
    Hada,
    Steel,
}

public enum Stat
{
    Attack, Defense, SpAttack, SpDefense, Speed, Accuracy, Evasion
}

public enum GrowthRate
{
    Erratic,
    Fast,
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating
}

public class TypeMatrix
{
    //TODO: Completar el resto de la matriz
    private static float [][] matrix = 
    {
        //                  NOR    FIR   WAT   ELE   GRA   ICE   FIG   POI   GRO   FLY   PSY   BUG   ROC   GHO   DRA   DAR   STE   FAI
        /*NOR*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 0f,   1f,   1f,   0.5f, 1f},
        /*FIR*/ new float[] {1f,   0.5f, 0.5f, 1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 1f,   2f,   1f},
        /*WAT*/ new float[] {1f,   2f,   0.5f, 1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   1f,   1f},
        /*ELE*/ new float[] {1f,   1f,   2f,   0.5f, 0.5f, 1f,   1f,   1f,   0f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f},
        /*GRA*/ new float[] {1f,   0.5f, 2f,   1f,   0.5f, 1f,   1f,   0.5f, 2f,   0.5f, 1f,   0.5f, 2f,   1f,   0.5f, 1f,   0.5f, 1f},
        /*ICE*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*FIG*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*POI*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*GRO*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*FLY*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*PSY*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*BUG*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*ROC*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*GHO*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*DRA*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*DAR*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*STE*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*FAI*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f}
    };
    

    public static float GetMultiplierEffectiveness(PokemonType attackType, PokemonType pokemonDefenderType)
    {
        if(attackType == PokemonType.None || pokemonDefenderType == PokemonType.None)
        {
            return 1.0f;
        }

        int row = (int) attackType;
        int col = (int) pokemonDefenderType;

        return matrix[row - 1][col - 1];
    }
}

/* CAMBIAR COLORES POKEMON
public class TypeColor() 
{
    private static Color[] colors =
    {
        Color.white, // None
        new Color(0.8193042f, 0.9333333f, 0.5254902f), // Bug
        new Color(0.735849f, 0.6178355f, 0.5588287f), // Dark
        new Color(0.6556701f, 0.5568628f, 0.7647059f), // Dragon
        new Color(0.9942768f, 1f, 0.5707547f), // Electric
        new Color(0.9339623f, 0.7621484f, 0.9339623f), // Fairy
        new Color(0.735849f, 0.5600574f, 0.5310609f), // Fight
        new Color(0.990566f, 0.5957404f, 0.5279903f), // Fire
        new Color(0.7358491f, 0.7708895f, 0.9811321f), // Flying
        new Color(0.6094251f, 0.6094251f, 0.7830189f), // Ghost
        new Color(0.4103774f, 1, 0.6846618f), // Grass
        new Color(0.9433962f, 0.7780005f, 0.5562478f), // Ground
        new Color(0.7216981f, 0.9072328f, 1), // Ice
        new Color(0.8734059f, 0.8773585f, 0.8235582f), // Normal
        new Color(0.6981132f, 0.4774831f, 0.6539872f), // Poison
        new Color(1, 0.6650944f, 0.7974522f), // Psychic
        new Color(0.8584906f, 0.8171859f, 0.6519669f), // Rock
        new Color(0.7889819f, 0.7889819f, 0.8490566f), // Steel
        new Color(0.5613208f, 0.7828107f, 1) // Water
    };

    public static Color GetColorFromType(PokemonType type)
    {
        return colors[(int)type];
    }
}
*/

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/New Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] private int id;
    public int Id => id;

    [SerializeField] private string name;
    public string Name => name;

    [TextArea] [SerializeField] private string description;
    public string Description => description;

    [SerializeField] Sprite frontSrpite;
    public Sprite FrontSprite => frontSrpite;
    [SerializeField] Sprite backSrpite;
    public Sprite BackSprite => backSrpite;

    [SerializeField] private PokemonType type1, type2;
    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2; 

    [SerializeField] private int catchRate = 255;
    public int CatchRate => catchRate;

    //Stats
    [SerializeField] private int maxHP;
    public int MaxHP => maxHP;
    [SerializeField] private int attack;
    public int Attack => attack;
    [SerializeField] private int defense;
    public int Defense => defense;
    [SerializeField] private int spAttack;
    public int SpAttack => spAttack;
    [SerializeField] private int spDefense;
    public int SpDefense => spDefense;
    [SerializeField] private int speed;
    public int Speed => speed;
    [SerializeField] private int expBase;
    public int ExpBase => expBase;
    [SerializeField] private GrowthRate _growthRate;
    public GrowthRate GrowthRate => _growthRate; 
    

    [SerializeField] private List<LearnableMove> learnableMoves;
    public List<LearnableMove> LearnableMoves => learnableMoves;

    public static int NUMBER_OF_LEARNABLE_MOVES {get;} = 4; //CONSTANTE

    public int GetNecessaryExpForLevel(int level)
    {
        switch(_growthRate)
        {
            case GrowthRate.Fast:
                return Mathf.FloorToInt(4* Mathf.Pow(level, 3) / 5);
            case GrowthRate.MediumFast:
                return Mathf.FloorToInt(Mathf.Pow(level, 3));
            case GrowthRate.MediumSlow:
                return Mathf.FloorToInt(6*Mathf.Pow(level, 3)/5 - 15 * Mathf.Pow(level, 2) + 100 * level - 140);
            case GrowthRate.Slow:
                return Mathf.FloorToInt(5* Mathf.Pow(level, 3) / 4);
            case GrowthRate.Erratic:
                if(level < 50)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (100 - level) / 50);
                }
                else if(level < 68)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (150 - level) / 100);
                }
                else if(level < 98)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * Mathf.FloorToInt(((1911 - 10 * level) / 3) / 500));
                }
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (160 - level) / 100);
            case GrowthRate.Fluctuating:
                if(level < 15)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (Mathf.FloorToInt((level + 1 )/ 3) + 24) / 50);
                }
                else if(level < 36)
                {
                     return Mathf.FloorToInt(Mathf.Pow(level, 3) * (level + 14 )/ 50);
                }
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (Mathf.FloorToInt((level/2) + 1 ) + 32) / 50);
        }
        return -1;
    }

}

[Serializable]
public class LearnableMove
{
    [SerializeField] private MoveBase move;
    [SerializeField] private int level;

    public MoveBase Move => move;
    public int Level => level;
}