using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LearnableMovementSelectionUI : MonoBehaviour
{
    [SerializeField] TMP_Text[] movementsTexts;
    private int currentSelectedMovement = 0;

    /*
        private void Start()
        {
            movementsTexts = GetComponentsInChildren<TMP_Text>(true);
        }*/

    public void SetMovements(List<MoveBase> pokemonMoves, MoveBase newMove)
    {
        currentSelectedMovement = 0;

        for (int i = 0; i < pokemonMoves.Count; i++)
        {
            movementsTexts[i].text = pokemonMoves[i].Name;
        }

        movementsTexts[pokemonMoves.Count].text = newMove.Name;
    }

    public void HandleForgetMoveSelection(Action<int> onSelected) //Evento/acción que actua como delegado
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            int direction = Mathf.FloorToInt(Input.GetAxisRaw("Vertical"));
            currentSelectedMovement -= direction;

            onSelected.Invoke(-1); //El usuario a cambiado de acción
        }

        currentSelectedMovement = Mathf.Clamp(currentSelectedMovement, 0, PokemonBase.NUMBER_OF_LEARNABLE_MOVES);

        UpdateForgetMoveSelection(currentSelectedMovement);

        if(Input.GetAxisRaw("Submit") != 0)
        {
            onSelected?.Invoke(currentSelectedMovement);
        }
    }

    public void UpdateForgetMoveSelection(int selectedMove)
    {
        for(int i = 0; i <= PokemonBase.NUMBER_OF_LEARNABLE_MOVES; i++)
        {
            movementsTexts[i].color = (i == selectedMove ? ColorManager.SharedInstance.selectedColor : Color.black);
        }
    }
}
