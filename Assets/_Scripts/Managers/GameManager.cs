using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum GameState
{
    Travel,
    Battle,
    Dialog,
    Cutscene
}

[RequireComponent(typeof(ColorManager))]
public class GameManager : MonoBehaviour
{

    [SerializeField] PlayerController _playerController;
    private Trainer_Controller trainer;
    [SerializeField] BattleManager _battleManager;
    [SerializeField] Camera worldMainCamera;
    [SerializeField] private Image TransitionPanel;
    private GameState _gameState;

    public AudioClip worldClip, battleClip;

    public static GameManager SharedInstance;

    void Awake()
    {
        if(SharedInstance != null)
        {
            Destroy(this);
        }

        SharedInstance = this;
        
        _gameState = GameState.Travel;
    }

    void Start()
    {
        StatusConditionFactory.InitFactory();
        SoundManager.SharedInstance.PlayMusic(worldClip);
        _playerController.OnPokemonEncountered += StartPokemonBattle;
        _playerController.OnEnterTrainerFoV += (Collider2D trainerCollider) =>
        {
            var trainer = trainerCollider.GetComponentInParent<Trainer_Controller>();
            if(trainer != null)
            {
                _gameState = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(_playerController));
            }
        };

        //Contra entrenadores seria distinto
        _battleManager.OnBattleFinish += FinishPokemonBattle;
        DialogManager.SharedInstance.OnDialogStart += () =>
        {
            _gameState = GameState.Dialog;
        };

        DialogManager.SharedInstance.OnDialogFinish += () =>
        {
            //_gameState = GameState.Travel; //Tal cual está ahora no volvemos al estado normal cuando hay un dialogo
            //TODO: si el dialogo es con un entrenador pokemon no vamos a travel si no a battle
        };
    }

    void StartPokemonBattle()
    {
        StartCoroutine(FadeInBattle());
    }

    public void StartTrainerBattle(Trainer_Controller trainer)
    {
        this.trainer = trainer;
        StartCoroutine(FadeInTrainerBattle(trainer));
    }

    IEnumerator FadeInBattle()
    {

        SoundManager.SharedInstance.PlayMusic(battleClip);
        _gameState = GameState.Battle;

        yield return TransitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        _battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        
        var playerParty = _playerController.GetComponent<PokemonParty>();
        
        //Si hay mas zonas habra que recuperar todos los obejtos y luego detectar en cual estas por colision o por distancia
        var wildPokemon = FindObjectOfType<PokemonMapArea>().GetComponent<PokemonMapArea>().GetRandomWildPokemon();

        //Una copia del pokemon del pool para que la plantilla del pool no se modifique
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        _battleManager.HandleStartBattle(playerParty, wildPokemonCopy);

        yield return TransitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator FadeInTrainerBattle(Trainer_Controller trainer)
    {
        SoundManager.SharedInstance.PlayMusic(battleClip);
        _gameState = GameState.Battle;

        yield return TransitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        _battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        
        var playerParty = _playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        _battleManager.HandleStartTrainerBattle(playerParty, trainerParty);

        yield return TransitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
    }

    void FinishPokemonBattle(bool playerHasWon)
    {
        if(trainer != null && playerHasWon)
        {
            trainer.AfterTrainerLostBattle();
            trainer = null;
        }

        StartCoroutine(FadeOutBattle());
    }

    IEnumerator FadeOutBattle()
    {
        yield return TransitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        SoundManager.SharedInstance.PlayMusic(worldClip);

        _battleManager.gameObject.SetActive(false);
        worldMainCamera.gameObject.SetActive(true);

        yield return TransitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        _gameState = GameState.Travel;
    }

    void Update()
    {
        if (_gameState == GameState.Travel)
        {
            _playerController.HandleUpdate();
        }
        else if (_gameState == GameState.Battle)
        {
            Debug.Log("Handle update del game manager");
            _battleManager.HandleUpdate();
        }
        else if(_gameState == GameState.Dialog)
        {
            DialogManager.SharedInstance.HandleUpdate();
        }
    }
}
