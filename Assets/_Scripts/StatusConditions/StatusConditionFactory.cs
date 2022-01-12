using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusConditionFactory
{

    //Inicializar el atributo Id según la clave del diccionario que toque
    public static void InitFactory()
    {
        foreach (var condition in StatusConditions)
        {
            var id = condition.Key;
            var statusCondition = condition.Value;
            statusCondition.Id = id;
        }
    }

    public static Dictionary<StatusConditionID, StatusCondition> StatusConditions { get; set; } =
        new Dictionary<StatusConditionID, StatusCondition>()
        {
            {
                StatusConditionID.psn,
                new StatusCondition()
                {
                    Name = "Poison",
                    Description = "Hace que el pokémon sufra daño en cada turno",
                    StartMessage = "ha sido envenenado",

                    OnFinishTurn = PoisonEffect
                }
            },
            {
                StatusConditionID.brn,
                new StatusCondition()
                {
                    Name = "Burn",
                    Description = "Hace que el pokémon sufra daño en cada turno",
                    StartMessage = "ha sido quemado",

                    OnFinishTurn = BurnEffect
                }
            },
            {
                StatusConditionID.par,
                new StatusCondition()
                {
                    Name = "Paralyze",
                    Description = "Hace que el pokémon pueda perder un turno",
                    StartMessage = "ha sido paralizado",

                    OnStartTurn = ParalyzeEffect
                }
            },
            {
                StatusConditionID.frz,
                new StatusCondition()
                {
                    Name = "Frozen",
                    Description = "Hace que el pokémon pierda turno, pero se puede curar",
                    StartMessage = "ha sido congelado",

                    OnStartTurn = FrozenEffect
                }
            },
            {
                StatusConditionID.slp,
                new StatusCondition()
                {
                    Name = "Sleep",
                    Description = "Hace que el pokémon duerma durante un número fijo de turnos",
                    StartMessage = "se ha dormido",

                    OnApplyStatusCondition = (Pokemon pokemon) =>
                    {
                        pokemon.statusNumberTurns = Random.Range(2,6);
                        Debug.Log($"Turnos dormido: {pokemon.statusNumberTurns}");
                    },

                    OnStartTurn = SleepEffect
                }
            },
            {
                StatusConditionID.con,
                new StatusCondition()
                {
                    Name = "Confusion",
                    Description = "El pokemon puede dañarse a sí mismo un número fijo de turnos",
                    StartMessage = "se siente confuso",

                    OnApplyStatusCondition = (Pokemon pokemon) =>
                    {
                        pokemon.VolatileStatusNumberTurns = Random.Range(1,6);
                        Debug.Log($"Turnos confuso: {pokemon.statusNumberTurns}");
                    },

                    OnStartTurn = ConfusionEffect
                }
            }
        };


        static void PoisonEffect(Pokemon pokemon)
        {
            pokemon.UpdateHP(Mathf.CeilToInt((float)pokemon.MaxHP/8));
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos del veneno.");
        }

        static void BurnEffect(Pokemon pokemon)
        {
            pokemon.UpdateHP(Mathf.CeilToInt((float)pokemon.MaxHP/15));
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos de la quemadura.");
        }

        static bool ParalyzeEffect(Pokemon pokemon)
        {
            //TODO: revisar el tema de la probabiliad de la paralisis
            if(Random.Range(1,100) < 25)
            {
                pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está paralizado y no se puede mover.");
                return false;
            }
            return true;
        }
        static bool FrozenEffect(Pokemon pokemon)
        {
            //TODO: revisar el tema de la probabiliad de la paralisis
            if(Random.Range(1,100) < 25)
            {
                pokemon.CureStatusCondition();
                pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no está congelado");
                return true;
            }
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está congelado y no se puede mover");
            return false;
        }

        static bool SleepEffect(Pokemon pokemon)
        {
            if(pokemon.statusNumberTurns <=0)
            {
                pokemon.CureStatusCondition();
                pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} se ha despertado.");
                return true;
            }
            pokemon.statusNumberTurns--;
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue dormido.");
            return false;
        }

        static bool ConfusionEffect(Pokemon pokemon)
        {
            if(pokemon.VolatileStatusNumberTurns <=0)
            {
                pokemon.CureVolatileStatusCondition();
                pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no está confuso.");
                return true;
            }
            pokemon.statusNumberTurns--;
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está confuso");

            if(Random.Range(0, 2) == 0)
            {
                return true;
            }
            else
            {
                //Daño así mismo.
                pokemon.UpdateHP(pokemon.MaxHP/6); //TODO: mirar el daño real
                pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está tan confuso que se hirió a sí mismo");
                return false;
            }
        }

}

public enum StatusConditionID
{
    none, brn, frz, par, psn, slp, con //Acrónimos de los estados 
}