using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyMemberHUD : MonoBehaviour
{

    [SerializeField] private TMP_Text nameText, lvlText;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Image pokemonImage;

    private Pokemon _pokemon;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        lvlText.text = $"Lv. {pokemon.Level}";
        healthBar.SetHP(pokemon.HP/pokemon.MaxHP);
        pokemonImage.sprite = pokemon.Base.FrontSprite;
    }

    public void SetSelectedPokemon(bool selected)
    {
        if(selected)
        {
            nameText.color = ColorManager.SharedInstance.selectedColor;
        }else
        {
            nameText.color = Color.black;
        }
    }
}
