using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatusCondition
{
    public StatusConditionID Id {get; set;}
    public string Name {get; set;}
    public string Description {get; set;}
    public string StartMessage {get; set;}
    
    //Una acción devuelve void, una función puede tener otro valor de retorno
    public Func<Pokemon, bool> OnStartTurn {get; set;}
    public Action<Pokemon> OnFinishTurn {get; set;}
    public Action<Pokemon> OnApplyStatusCondition{get; set;} //Cuando se aplica la condición de estado

}
