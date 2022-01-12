using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] private const int NUM_MAX_POKEMON_IN_PARTY= 6;
    [SerializeField] private List<Pokemon> pokemons;

    public List<Pokemon> Pokemons
    {
        get => pokemons;
        set => pokemons = value;
    }

    private void Start()
    {
        foreach(var pokemon in pokemons)
        {
            pokemon.InitPokemon();
        }
    }

    public Pokemon GetFirstNonFaintedPokemon()
    {
        return pokemons.Where(p => p.HP>0).FirstOrDefault();
    }

    public int GetPositionFromPokemon(Pokemon pokemon)
    {
        for (int i=0; i<Pokemons.Count; i++)
        {
            if(pokemon == Pokemons[i])
            {
                return i;
            }
        }
        return -1; //Posición no válida. No va a llegar aquí nunca
    }

    public bool AddPokemonToParty(Pokemon newPokemon)
    {
        if(pokemons.Count < NUM_MAX_POKEMON_IN_PARTY)
        {
            pokemons.Add(newPokemon);
            return true;
        }
        else
        {
            return false;
            //TODO: Añadir la funcionalidad de enviar al PC de BILL
            //Privaate List<List<Pokemon>> pcBillBoxes = new List<List<Pokemon>>(6)
            //se inicializa en el start
            //var box =new list pokemon(16);
            //for para rellenar
            //Serializarlo
        }
    }
}
