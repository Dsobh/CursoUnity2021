using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyHUD : MonoBehaviour
{
    private PartyMemberHUD[] membersHUDs;
    [SerializeField] TMP_Text messageText;

    private List<Pokemon> pokemons;

    public void InitPartyHUD()
    {
        membersHUDs = GetComponentsInChildren<PartyMemberHUD>(true); //El true devuelve también las inactivas en la jerarquia

    }

    public void SetPartyData(List<Pokemon> pokemons)
    {

        this.pokemons = pokemons;

        messageText.text = $"Selecciona un pokemon.";

        for(int i=0; i<membersHUDs.Length; i++)
        {
            if(i < pokemons.Count)
            {
                membersHUDs[i].gameObject.SetActive(true);
                membersHUDs[i].SetPokemonData(pokemons[i]);
            }else
            {
                membersHUDs[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateSelectedPokemon(int selectedPokemon)
    {
        for(int i=0; i<pokemons.Count; i++)
        {
            membersHUDs[i].SetSelectedPokemon(i == selectedPokemon);
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
