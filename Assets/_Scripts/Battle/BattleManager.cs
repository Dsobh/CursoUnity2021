using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

public enum BattleState
{
    StartBattle,
    ActionSelection,
    MovementSelection,
    Busy,
    YesNoChoice,
    PartySelectScreen,
    ForgetMovementScreen,
    EndTurn,
    FinishBattle,
}

public enum BattleAction
{
    Move,
    SwitchPokemon,
    UseItem,
    Escape
}

public enum BattleType
{
    WildPokemon,
    trainer,
    Leader
}

public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;

    [SerializeField] private Image playerImage, trainerImage;

    [SerializeField] private BattleDialogBox _battleDialogBox;

    [SerializeField] private PartyHUD _partyHUD;
    [SerializeField] private LearnableMovementSelectionUI _learnableMovementSelectionUI;

    [SerializeField] private GameObject pokeball;

    public BattleState state;
    public BattleState? previousState; //El interrogante es que puede ser nulo

    public BattleType type;

    public event Action<bool> OnBattleFinish; //Devuelve si gana o pierde

    private PokemonParty playerParty;
    private PokemonParty trainerParty;
    private Pokemon wildPokemon;

    private PlayerController player;
    private Trainer_Controller trainer;

    private MoveBase moveToLearn;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 0.01f;

    int currentSelectedAction;
    int currentSelectedPokemon;
    int currentSelectedMovement;
    bool currentSelectedChoice = true;

    private int escapeAttemps;

    public AudioClip attackClip, damageClip, levelUpClip, endBattleClip, pokeballClip;

    public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        type = BattleType.WildPokemon;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        escapeAttemps = 0;
        StartCoroutine(SetUpBattle());
    }

    public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool isLeader = false)
    {
        type = (isLeader ? BattleType.Leader : BattleType.trainer);
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<Trainer_Controller>();
        StartCoroutine(SetUpBattle());
    }

    public IEnumerator SetUpBattle()
    {
        state = BattleState.StartBattle;

        playerUnit.ClearHUD();
        enemyUnit.ClearHUD();

        //TODO: Refactorizar esto
        if (type == BattleType.WildPokemon)
        {
            enemyUnit.SetupPokemon(wildPokemon);
            playerUnit.SetupPokemon(playerParty.GetFirstNonFaintedPokemon());

            yield return _battleDialogBox.SetDialog($"Un {enemyUnit._pokemon.Base.Name} salvaje apareció");
        }
        else //Entrenador y líder
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            //Animación Player
            var playerInitialPosition = playerImage.transform.localPosition;
            playerImage.transform.localPosition = playerInitialPosition - new Vector3(400f, 0f, 0f);
            playerImage.transform.DOLocalMoveX(playerInitialPosition.x, 0.5f);

            //Animación Trainer
            var trainerInitialPosition = trainerImage.transform.localPosition;
            trainerImage.transform.localPosition = trainerInitialPosition + new Vector3(400f, 0f, 0f);
            trainerImage.transform.DOLocalMoveX(trainerInitialPosition.x, 0.5f);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.PlayerSprite;
            trainerImage.sprite = trainer.TrainerSprite;

            yield return _battleDialogBox.SetDialog($"{trainer.TrainerName} te desafía!!");

            //Enviar primer pokemon del rival
            yield return trainerImage.transform.DOLocalMoveX(trainerImage.transform.localPosition.x + 400, 0.5f).WaitForCompletion();
            trainerImage.gameObject.SetActive(false);
            trainerImage.transform.localPosition = trainerInitialPosition;
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetFirstNonFaintedPokemon();
            enemyUnit.SetupPokemon(enemyPokemon);
            yield return _battleDialogBox.SetDialog($"{trainer.TrainerName} ha enviado a {enemyPokemon.Base.name}");


            //Enviar el primer pokemon del player
            yield return playerImage.transform.DOLocalMoveX(playerImage.transform.localPosition.x - 400, 0.5f).WaitForCompletion();
            playerImage.gameObject.SetActive(false);
            playerImage.transform.localPosition = playerInitialPosition;
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetFirstNonFaintedPokemon();
            playerUnit.SetupPokemon(playerPokemon);
            yield return _battleDialogBox.SetDialog($"Adelante {playerPokemon.Base.name}!!");
        }

        _battleDialogBox.SetPokemonMovements(playerUnit._pokemon.Moves); //Si no va bien ponerla antes del dialogo
        _partyHUD.InitPartyHUD();

        PlayerActionSelection();
    }

    private void BattleFinish(bool playerHasWon)
    {
        SoundManager.SharedInstance.PlaySound(endBattleClip);
        state = BattleState.FinishBattle;
        playerParty.Pokemons.ForEach(p => p.OnBattleFinish());
        OnBattleFinish(playerHasWon);
    }

    private void PlayerActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(_battleDialogBox.SetDialog("Selecciona una acción"));
        _battleDialogBox.ToggleDialogText(true);
        _battleDialogBox.ToggleActionSelector(true);
        _battleDialogBox.ToggleMoveSelector(false);
        //Al inicio de un turno siempre es la selección inicial la 0
        currentSelectedAction = 0;
        _battleDialogBox.SelectAction(currentSelectedAction);
    }

    private void PlayerMovementSelection()
    {
        state = BattleState.MovementSelection;
        _battleDialogBox.ToggleDialogText(false);
        _battleDialogBox.ToggleActionSelector(false);
        _battleDialogBox.ToggleMoveSelector(true);
        currentSelectedMovement = 0;
        _battleDialogBox.SelectMove(currentSelectedMovement, playerUnit._pokemon.Moves[currentSelectedMovement]);
    }

    private void OpenPartySelectionScreen()
    {
        state = BattleState.PartySelectScreen;
        _partyHUD.SetPartyData(playerParty.Pokemons);
        _partyHUD.gameObject.SetActive(true);
        currentSelectedPokemon = playerParty.GetPositionFromPokemon(playerUnit._pokemon);
        _partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);
    }

    //Igual se puede refactorizar esto para no pedir el nextTrainerPokemon en tantos sitios
    IEnumerator YesNoChoice(Pokemon newTrainerPokemon)
    {
        state = BattleState.Busy;

        yield return _battleDialogBox.SetDialog($"{trainer.TrainerName} va a sacar a {newTrainerPokemon.Base.Name}");
        yield return _battleDialogBox.SetDialog($"¿Quieres cambiar de pokémon?");

        state = BattleState.YesNoChoice;
        _battleDialogBox.ToggleYesNoBox(true);

    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;

        if (timeSinceLastClick < timeBetweenClicks || _battleDialogBox.isWritting)
        {
            return;
        }

        if (state == BattleState.ActionSelection)
        {
            HandlePlayerActionSelection();
        }
        else if (state == BattleState.MovementSelection)
        {
            HandlePlayerMoveSelection();
        }
        else if (state == BattleState.PartySelectScreen)
        {
            HandlePlayerPartySelection();
        }
        else if(state == BattleState.YesNoChoice)
        {
            HandleYesNoChoice();
        }
        else if (state == BattleState.ForgetMovementScreen)
        {
            _learnableMovementSelectionUI.HandleForgetMoveSelection((moveIndex) =>
            {
                if (moveIndex < 0)
                {
                    timeSinceLastClick = 0;
                    return;
                }
                StartCoroutine(ForgetOldMove(moveIndex));
            });
        }
    }

    IEnumerator ForgetOldMove(int moveIndex)
    {
        _learnableMovementSelectionUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
        {
            //No aprendo el nuevo
            yield return _battleDialogBox.SetDialog($"{playerUnit._pokemon.Base.Name} no ha aprendido {moveToLearn.Name}");
        }
        else
        {
            //Olvido el seleccionado y aprendo el nuevo
            var selectedMove = playerUnit._pokemon.Moves[moveIndex].Base;
            yield return _battleDialogBox.SetDialog($"{playerUnit._pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}");
            playerUnit._pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;

        state = BattleState.FinishBattle; //TODO: revisar cuando haya entrenadores

    }

    private void HandlePlayerActionSelection()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;

            currentSelectedAction = (currentSelectedAction + 2) % 4; //4 es el número de acciones

            _battleDialogBox.SelectAction(currentSelectedAction);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedAction = (currentSelectedAction + 1) % 2 + 2 * (int)(currentSelectedAction / 2);

            _battleDialogBox.SelectAction(currentSelectedAction);
        }


        //Para movimiento vertical
        /*if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;

            currentSelectedAction = (currentSelectedAction + 1) % 2; //2 es el número de acciones

            _battleDialogBox.SelectAction(currentSelectedAction);
        }*/

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            if (currentSelectedAction == 0)
            {
                //LUCHA
                PlayerMovementSelection();
            }
            else if (currentSelectedAction == 1)
            {
                //PARTY
                previousState = state;
                OpenPartySelectionScreen();
            }
            else if (currentSelectedAction == 2)
            {
                //MOCHILA
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentSelectedAction == 3)
            {
                //HUIR
                StartCoroutine(RunTurns(BattleAction.Escape));
            }
        }
    }

    private void HandlePlayerMoveSelection()
    {

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            int oldSelectedMove = currentSelectedMovement;
            currentSelectedMovement = (currentSelectedMovement + 2) % 4;
            if (currentSelectedMovement >= playerUnit._pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMove;
            }
            _battleDialogBox.SelectMove(currentSelectedMovement, playerUnit._pokemon.Moves[currentSelectedMovement]);

        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {

            timeSinceLastClick = 0;
            int oldSelectedMove = currentSelectedMovement;
            currentSelectedMovement = currentSelectedMovement + 1 % 2 + 2 * (int)(currentSelectedMovement / 2);
            /* if (currentSelectedMove <= 1)
             {
                 currentSelectedMove = (currentSelectedMove + 1) % 2;
             }
             else //Mayor esctricto que 1
             {
                 currentSelectedMove = (currentSelectedMove - 1) % 2 + 2;
             }*/
            if (currentSelectedMovement >= playerUnit._pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMove;
            }
            _battleDialogBox.SelectMove(currentSelectedMovement, playerUnit._pokemon.Moves[currentSelectedMovement]);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            _battleDialogBox.ToggleMoveSelector(false);
            _battleDialogBox.ToggleDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            PlayerActionSelection();
        }

    }

    void HandlePlayerPartySelection()
    {

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon -= (int)Input.GetAxisRaw("Vertical") * 2;
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon += (int)Input.GetAxisRaw("Horizontal");
        }

        currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
        _partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
            if (selectedPokemon.HP <= 0)
            {
                _partyHUD.SetMessage("No puedes enviar un Pokémon debilitado.");
                return;
            }
            else if (selectedPokemon == playerUnit._pokemon)
            {
                _partyHUD.SetMessage("Este Pokémon ya está en batalla.");
                return;
            }

            _partyHUD.gameObject.SetActive(false);
            _battleDialogBox.ToggleActionSelector(false);

            if (previousState == BattleState.ActionSelection)
            {
                previousState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy; //Para hacer las animaciones y luego pasar al enemigo
                StartCoroutine(SwitchPokemon(selectedPokemon));
            }
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {

            if(playerUnit._pokemon.HP <= 0)
            {
                _partyHUD.SetMessage("Tienes que seleccionar un Pokemon para continuar el combate.");
                return;
            }

            _partyHUD.gameObject.SetActive(false);

            if(previousState == BattleState.YesNoChoice)
            {
                state = BattleState.YesNoChoice;
                //Pomode no volver a la pantalla de si o no y sería sacar al siguiente pokemon del entrenador
                //Startcoroutine(SendNextTrainerPokemonToBattle())
                previousState = null;
            }
            else
            {
                PlayerActionSelection();
            }
            
        }
    }

    private void HandleYesNoChoice()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedChoice = !currentSelectedChoice;
        }

        _battleDialogBox.SelectYesNoAction(currentSelectedChoice);

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            _battleDialogBox.ToggleYesNoBox(false);

            if(currentSelectedChoice)
            {
                //Cambiar pokémon
                previousState = BattleState.YesNoChoice;
                OpenPartySelectionScreen();

            }
            else
            {
                //No cambiamos de pokémon
                StartCoroutine(SendNextTrainerPokemonToBattle());
            }
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            timeSinceLastClick = 0;
            //Es lo mismo que seleccionar no
            _battleDialogBox.ToggleYesNoBox(false);
            StartCoroutine(SendNextTrainerPokemonToBattle());
        }
    }


    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit._pokemon.HP > 0)
        {
            yield return _battleDialogBox.SetDialog($"Vuelve {playerUnit._pokemon.Base.Name}.");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
        }

        playerUnit.SetupPokemon(newPokemon);

        _battleDialogBox.SetPokemonMovements(newPokemon.Moves);

        yield return _battleDialogBox.SetDialog($"Adelante {playerUnit._pokemon.Base.Name}.");
        yield return new WaitForSeconds(1.0f);

        if(previousState == null)
        {
            state = BattleState.EndTurn;
        }
        else if(previousState == BattleState.YesNoChoice)
        {
            yield return SendNextTrainerPokemonToBattle();
        }
        
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.EndTurn;
        if (playerAction == BattleAction.Move)
        {
            playerUnit._pokemon.CurrentMove = playerUnit._pokemon.Moves[currentSelectedMovement];
            enemyUnit._pokemon.CurrentMove = enemyUnit._pokemon.RandomMove();

            //TOOD: Revisar que las velocidades de los ataques funcionen así
            bool playerGoesFirst = true;
            int enemyPriority = enemyUnit._pokemon.CurrentMove.Base.Priority;
            int playerPriority = playerUnit._pokemon.CurrentMove.Base.Priority;
            if (enemyPriority > playerPriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyPriority == playerPriority)
            {
                playerGoesFirst = playerUnit._pokemon.Speed >= enemyUnit._pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst ? playerUnit : enemyUnit);
            var secondUnit = (playerGoesFirst ? enemyUnit : playerUnit);

            var secondPokemon = secondUnit._pokemon;

            //Primer turno
            yield return RunMovement(firstUnit, secondUnit, firstUnit._pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);

            //Chequeamos si el pokemon a muerto
            if (state == BattleState.FinishBattle)
            {
                yield break;
            }

            if (secondPokemon.HP > 0)
            {
                //Segundo turno
                yield return RunMovement(secondUnit, firstUnit, secondUnit._pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);

                //Chequeamos si el pokemon a muerto
                if (state == BattleState.FinishBattle)
                {
                    yield break;
                }
            }
        }
        else
        {
            {
                if (playerAction == BattleAction.SwitchPokemon)
                {
                    var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
                    state = BattleState.Busy;
                    yield return SwitchPokemon(selectedPokemon);
                }
                else if (playerAction == BattleAction.UseItem)
                {
                    _battleDialogBox.ToggleActionSelector(false);
                    yield return ThrowPokeball();
                }
                else if (playerAction == BattleAction.Escape)
                {
                    yield return TryToEscapeFromBattle();
                }


                //Turno del enemigo
                var enemyMove = enemyUnit._pokemon.RandomMove();
                yield return RunMovement(enemyUnit, playerUnit, enemyMove);
                yield return RunAfterTurn(enemyUnit);

                //Chequeamos si el pokemon a muerto
                if (state == BattleState.FinishBattle)
                {
                    yield break;
                }
            }
        }

        if (state != BattleState.FinishBattle)
        {
            PlayerActionSelection();
        }
    }

    IEnumerator RunMovement(BattleUnit attacker, BattleUnit target, Move move)
    {

        //Comporbar el estado alterado que impide atacar: paralisis, congelado o dormido
        bool canRunMovement = attacker._pokemon.OnStartTurn();
        if (!canRunMovement)
        {
            yield return ShowStatsMessages(attacker._pokemon);
            yield return attacker.Hud.UpdatePokemonData();
            yield break;
        }
        yield return ShowStatsMessages(attacker._pokemon);

        move.PP--;
        yield return _battleDialogBox.SetDialog($"{attacker._pokemon.Base.Name} ha usado {move.Base.Name}");

        if (MoveHits(move, attacker._pokemon, target._pokemon))
        {
            yield return RunMoveAnims(attacker, target);

            if (move.Base.MoveType == MoveType.Stats)
            {
                yield return RunMoveStats(attacker._pokemon, target._pokemon, move.Base.Effects, move.Base.Target);
            }
            else
            {
                var damagaDetails = target._pokemon.ReceiveDamage(move, attacker._pokemon);
                yield return target.Hud.UpdatePokemonData();
                yield return ShowDamageDetails(damagaDetails);

            }

            //Chequear posibles efectos secundarios
            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0)
            {
                foreach (var sec in move.Base.SecondaryEffects)
                {
                    if (sec.Target == MoveTarget.Other && target._pokemon.HP > 0
                    || sec.Target == MoveTarget.Self && attacker._pokemon.HP > 0)
                    {
                        var rnd = Random.Range(0, 100);
                        if (rnd < sec.Chance)
                        {
                            yield return RunMoveStats(attacker._pokemon, target._pokemon, sec, sec.Target);
                        }
                    }
                }
            }

            if (target._pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(target);
            }
        }
        else
        {
            yield return _battleDialogBox.SetDialog($"{attacker._pokemon.Base.Name} ha fallado");
        }
    }

    //TODO: asegurarnos de que la forma de calcular el exito del ataque es asi
    bool MoveHits(Move move, Pokemon attacker, Pokemon target)
    {
        if (move.Base.AlwaysHit)
        {
            return true;
        }

        float rnd = Random.Range(0, 100);
        float moveAcc = move.Base.Accuracy;

        float accuracyAttacker = attacker.StatsBoosted[Stat.Accuracy];
        float evasion = attacker.StatsBoosted[Stat.Evasion];

        float multiplierAcc = (1.0f + Mathf.Abs(accuracyAttacker) / 3.0f);
        float multiplierEvs = (1.0f + Mathf.Abs(evasion) / 3.0f);
        if (accuracyAttacker > 0)
        {
            moveAcc *= multiplierAcc;
        }
        else
        {
            moveAcc /= multiplierAcc;
        }

        if (evasion > 0) //Va al reves porque cuanto mas evada el defensor menos precisión tendra el movimiento
        {
            moveAcc /= multiplierEvs;
        }
        else
        {
            moveAcc += multiplierEvs;
        }

        return rnd < moveAcc;
    }

    IEnumerator RunMoveAnims(BattleUnit attacker, BattleUnit target)
    {
        attacker.PlayAttackAnimation();
        SoundManager.SharedInstance.PlaySound(attackClip);
        yield return new WaitForSeconds(1f);

        target.PlayReceiveDamageAnimation();
        SoundManager.SharedInstance.PlaySound(damageClip);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator RunMoveStats(Pokemon attacker, Pokemon target, MoveStatEffect effect, MoveTarget moveTarget)
    {
        //Stat boosting
        foreach (var boost in effect.Boostings)
        {
            if (boost.target == MoveTarget.Self)
            {
                attacker.ApplyBoost(boost);
            }
            else
            {
                target.ApplyBoost(boost);
            }
        }

        //Status Condition
        if (effect.Status != StatusConditionID.none)
        {
            if (moveTarget == MoveTarget.Other)
            {
                target.SetConditionStatus(effect.Status);
            }
            else
            {
                attacker.SetConditionStatus(effect.Status);
            }
        }

        //Volatile Status Condition
        if (effect.VolatileStatus != StatusConditionID.none)
        {
            if (moveTarget == MoveTarget.Other)
            {
                target.SetVolatileConditionStatus(effect.VolatileStatus);
            }
            else
            {
                attacker.SetVolatileConditionStatus(effect.VolatileStatus);
            }
        }

        yield return ShowStatsMessages(attacker);
        yield return ShowStatsMessages(target);
    }

    IEnumerator ShowStatsMessages(Pokemon pokemon)
    {
        while (pokemon.StatusChangeMessages.Count > 0)
        {
            var message = pokemon.StatusChangeMessages.Dequeue();
            yield return _battleDialogBox.SetDialog(message);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit attacker)
    {
        if (state == BattleState.FinishBattle)
        {
            yield break;
        }

        yield return new WaitUntil(() => state == BattleState.EndTurn);

        //Comprobar estados alterados como quemadura o veneno al final del turno
        attacker._pokemon.OnFinishTurn();
        yield return ShowStatsMessages(attacker._pokemon); //Desencolamos los mensajes 
        yield return attacker.Hud.UpdatePokemonData();

        if (attacker._pokemon.HP < 0)
        {
            yield return HandlePokemonFainted(attacker);
        }

        yield return new WaitUntil(() => state == BattleState.EndTurn);
    }

    void CheckForBattleFinish(BattleUnit faintedUnit)
    { 
        if (faintedUnit.IsPlayer)
        {
            var nextPokemon = playerParty.GetFirstNonFaintedPokemon();
            if (nextPokemon != null)
            {
                OpenPartySelectionScreen();
            }
            else
            {
                BattleFinish(false);
            }
        }
        else
        {
            if(type == BattleType.WildPokemon)
            {
                BattleFinish(true);
            }
            else //Batalla contra un entrenador
            {
                var nextPokemon = trainerParty.GetFirstNonFaintedPokemon();
                if(nextPokemon != null)
                {
                    StartCoroutine(YesNoChoice(nextPokemon));
                }
                else //El rival se ha quedado sin pokemon
                {
                    BattleFinish(true);
                }
            }
        }
    }

    IEnumerator SendNextTrainerPokemonToBattle()
    {
        state = BattleState.Busy;
        var nextPokemon = trainerParty.GetFirstNonFaintedPokemon();
        enemyUnit.SetupPokemon(nextPokemon);
        yield return _battleDialogBox.SetDialog($"{trainer.TrainerName} ha enviado a {nextPokemon.Base.Name}");
        state = BattleState.EndTurn; //Run turn pone en el video
    }

    IEnumerator ShowDamageDetails(DamageDescription damageDetails)
    {
        if (damageDetails.Critical > 1)
        {
            yield return _battleDialogBox.SetDialog("Un golpe crítico");
        }

        if (damageDetails.Type > 1)
        {
            yield return _battleDialogBox.SetDialog("Es muy efectivo");
        }
        else if (damageDetails.Type < 1)
        {
            yield return _battleDialogBox.SetDialog("No es muy efectivo...");
        }
    }

    IEnumerator ThrowPokeball()
    {

        //TODO: Parece que puedo capurar cualquier pokemon

        state = BattleState.Busy;

        if (type != BattleType.WildPokemon)
        {
            yield return _battleDialogBox.SetDialog("No puedes robar los pokemons de otros entrenadores");
            state = BattleState.EndTurn;
            yield break;
        }

        yield return _battleDialogBox.SetDialog($"Has lanzado una {pokeball.name}!");

        SoundManager.SharedInstance.PlaySound(pokeballClip);
        var pokeballIns = Instantiate(pokeball, playerUnit.transform.position + new Vector3(-2, 0, 0), Quaternion.identity);

        var pokeballSprite = pokeballIns.GetComponent<SpriteRenderer>();

        yield return pokeballSprite.transform.DOLocalJump(enemyUnit.transform.position + new Vector3(0, 2, 0), 2f, 1, 1f).WaitForCompletion();

        yield return enemyUnit.PlayCaptureAnimation();

        yield return pokeballSprite.transform.DOLocalMoveY(enemyUnit.transform.position.y - 1.5f, 0.3f).WaitForCompletion();

        int numberOfShake = TryToCatchPokemon(enemyUnit._pokemon);
        for (int i = 0; i < Mathf.Min(numberOfShake, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);

            yield return pokeballSprite.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.6f).WaitForCompletion();
        }

        if (numberOfShake == 4)
        {
            yield return _battleDialogBox.SetDialog($"¡{enemyUnit._pokemon.Base.Name} ha sido capturado!");
            yield return pokeballSprite.DOFade(0, 1.5f).WaitForCompletion();

            if (playerParty.AddPokemonToParty(enemyUnit._pokemon))
            {
                yield return _battleDialogBox.SetDialog($"{enemyUnit._pokemon.Base.Name} se ha añadido a tu equipo.");
            }
            else
            {
                yield return _battleDialogBox.SetDialog($"{enemyUnit._pokemon.Base.Name} se ha enviado al PC.");

            }


            Destroy(pokeballIns);
            BattleFinish(true);
        }
        else //El pokemon escapa
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeballSprite.DOFade(0, 0.2f).WaitForCompletion();
            yield return enemyUnit.PlayBrakeOutAnimation();

            if (numberOfShake < 2)
            {
                yield return _battleDialogBox.SetDialog($"¡{enemyUnit._pokemon.Base.Name} se ha escapado!");
            }
            else
            {
                yield return _battleDialogBox.SetDialog($"¡Casi lo has atrapado!");
            }
            Destroy(pokeballIns);

            state = BattleState.EndTurn;


        }
    }

    private int TryToCatchPokemon(Pokemon pokemon)
    {
        float bonusPokeball = 1; //TODO: clase pokeball con su multiplicador
        float bonusStat = 1; //TODO: stats para checkear condición de modificación.
        float a = (3 * pokemon.MaxHP - 2 * pokemon.HP) * pokemon.Base.CatchRate * bonusPokeball * bonusStat / 3 * pokemon.MaxHP;

        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680) / a);

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (Random.Range(0, 65535) >= b)
            {
                break;
            }
            else
            {
                shakeCount++;
            }
        }

        return shakeCount;
    }

    private IEnumerator TryToEscapeFromBattle()
    {
        state = BattleState.Busy;

        if (type != BattleType.WildPokemon)
        {
            yield return _battleDialogBox.SetDialog("No puedes huir de combates contra entrenadores pokemon.");
            state = BattleState.EndTurn;
            yield break;
        }

        //Es contra un pokemon salvaje
        escapeAttemps++;
        int playerSpeed = playerUnit._pokemon.Speed;
        int enemySpeed = enemyUnit._pokemon.Speed;

        if (playerSpeed >= enemySpeed)
        {
            yield return _battleDialogBox.SetDialog("Has escapado sin problemas.");
            yield return new WaitForSeconds(1f);
            OnBattleFinish(true);
        }
        else
        {
            int oddsScape = (Mathf.FloorToInt(playerSpeed * 128 / enemySpeed) + 30 * escapeAttemps) % 256;
            if (Random.Range(0, 256) < oddsScape)
            {
                yield return _battleDialogBox.SetDialog("Has escapado sin problemas.");
                yield return new WaitForSeconds(1f);
                OnBattleFinish(true);
            }
            else
            {
                yield return _battleDialogBox.SetDialog("No puedes escapar");
                state = BattleState.EndTurn;
            }
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return _battleDialogBox.SetDialog($"{faintedUnit._pokemon.Base.Name} se ha debilitado");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(1.5f);

        if (!faintedUnit.IsPlayer)
        {
            //TODO:Retocar la formula de la exp
            //Ganar exp
            int expBase = faintedUnit._pokemon.Base.ExpBase; //La experiencia la da el pokemon al que has matado
            int level = faintedUnit._pokemon.Level;
            float multiplier = (type == BattleType.WildPokemon ? 1 : 1.5f); //Mas si es entrenador o lider de gim
            int wonExp = Mathf.FloorToInt((expBase * level * multiplier) / 7);
            playerUnit._pokemon.Exp += wonExp;
            yield return _battleDialogBox.SetDialog($"{playerUnit._pokemon.Base.name} ha ganado {wonExp} puntos de experiencia");
            yield return playerUnit.Hud.SetSmoothExp();
            yield return new WaitForSeconds(0.5f);

            //Check new level varias veces
            while (playerUnit._pokemon.NeedsToLevelUp())
            {
                SoundManager.SharedInstance.PlaySound(levelUpClip);
                playerUnit.Hud.SetLevelText();
                playerUnit._pokemon.hasHPChanged = true;
                playerUnit.Hud.UpdatePokemonData();
                yield return _battleDialogBox.SetDialog($"{playerUnit._pokemon.Base.Name} ha subido de nivel.");

                var newLearnableMove = playerUnit._pokemon.GetLearnableMoveAtCurrentLevel();
                if (newLearnableMove != null)
                {
                    if (playerUnit._pokemon.Moves.Count < PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
                    {
                        playerUnit._pokemon.LearnMove(newLearnableMove);
                        yield return _battleDialogBox.SetDialog($"{playerUnit._pokemon.Base.Name} ha aprendido {newLearnableMove.Move.Name}");
                        _battleDialogBox.SetPokemonMovements(playerUnit._pokemon.Moves);
                    }
                    else
                    {
                        //Olvidar uno de los movimientos
                        yield return _battleDialogBox.SetDialog($"{playerUnit._pokemon.Base.Name} intenta aprender {newLearnableMove.Move.Name}");
                        yield return _battleDialogBox.SetDialog($"Pero {playerUnit._pokemon.Base.Name} ya conoce cuatro movimientos");
                        yield return ChooseMovementToForget(playerUnit._pokemon, newLearnableMove.Move);
                        yield return new WaitUntil(() => state != BattleState.ForgetMovementScreen); //Predicado booleano

                    }
                }

                yield return playerUnit.Hud.SetSmoothExp(true);
            }
        }

        CheckForBattleFinish(faintedUnit);
    }

    IEnumerator ChooseMovementToForget(Pokemon learner, MoveBase newMove)
    {
        state = BattleState.Busy;

        yield return _battleDialogBox.SetDialog("¿Qué movimiento debería olvidar?");
        _learnableMovementSelectionUI.gameObject.SetActive(true);
        _learnableMovementSelectionUI.SetMovements(learner.Moves.Select(mv => mv.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = BattleState.ForgetMovementScreen;
    }
}
