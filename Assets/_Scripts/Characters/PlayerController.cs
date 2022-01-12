using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterAnimator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private string playerName;

    public Sprite PlayerSprite => playerSprite;
    public string PlayerName => playerName;

    //private bool isMoving;

    /*[SerializeField]
    private float speed;*/

    private Vector2 input;

    //private CharacterAnimator _animator;

    private Character _character;

    //public LayerMask solidObjectsLayer, pokemonLayer, interactableLayer;

    public event Action OnPokemonEncountered;
    public event Action<Collider2D> OnEnterTrainerFoV;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;



    void Awake()
    {
       //_animator = GetComponent<CharacterAnimator>();
        _character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;

        //Lógica de movimiento
        if (!_character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            
            /*if (input.x != 0)
            {
                input.y = 0;
            }*/

            if (input != Vector2.zero)
            {
              /*  _animator.moveX = input.x;
                _animator.moveY = input.y;

                Vector3 targetPosition = transform.position;
                targetPosition.x += input.x;
                targetPosition.y += input.y;

                if (IsAvailable(targetPosition))
                {
                    StartCoroutine(MoveTowards(targetPosition));
                }*/
                StartCoroutine(_character.MoveTowards(input, OnMoveFinish));
            }
        }

        _character.HandleUpdate();

        //Lógica de interacción con el mundo
        if (Input.GetAxisRaw("Submit") != 0)
        {
            if (timeSinceLastClick >= timeBetweenClicks)
            {
                Interact();
            }
        }
    }

    /*private void LateUpdate()
    {
        _animator.isMoving = isMoving;
    }*/

/*
    private IEnumerator MoveTowards(Vector3 destination)
    {
        isMoving = true;

        //Comparamos la distancia usando la precisión de la máquina
        while (Vector3.Distance(transform.position, destination) > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null; //Esperar hasta el siguiente frame
        }

        transform.position = destination;

        isMoving = false;

        CheckForPokemon();
    }*/

 /*   private bool IsAvailable(Vector3 target)
    {
        //El O | de la siguiente línea es a nivel de bit para la gestión de capas
        if (Physics2D.OverlapCircle(target, 0.15f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }*/

    private void OnMoveFinish()
    {
        CheckForPokemon();
        CheckForInTrainerFoV();
    }

    private void Interact()
    {
        timeSinceLastClick = 0;
        var facingDirection = new Vector3(_character._Animator.moveX, _character._Animator.moveY);
        var interactPosition = transform.position + facingDirection; //Buscamos la posición contra la que vamos a interactuar

        Debug.DrawLine(transform.position, interactPosition, Color.magenta, 1.0f);

        var collider = Physics2D.OverlapCircle(interactPosition, 0.15f, GameLayers.SharedInstance.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform.position); //Si no es nulo interactuamos
        }
    }

    private void CheckForPokemon()
    {
        //Si el player esta desplazado en y -> transform.position - new Vector3(0, 0.2f)
        //Le podemos meter un offset vertical para que no dispare la colisión con la cabeza
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.SharedInstance.PokemonLayer) != null)
        {
            //TODO: Mejorar esto, tal vez sacarlo a un manager singleton
            if (UnityEngine.Random.Range(0, 100) < 10)
            {
                _character._Animator.isMoving = false;
                OnPokemonEncountered();
            }
        }
    }

    private void CheckForInTrainerFoV()
    {
        //Si el player esta desplazado en y -> transform.position - new Vector3(0, 0.2f)
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.SharedInstance.FoVLayer);
        if (collider != null)
        {
            //TODO: Mejorar esto, tal vez sacarlo a un manager singleton
            if (UnityEngine.Random.Range(0, 100) < 10)
            {
                Debug.Log("En el campo de visión del entrenador");
                _character._Animator.isMoving = false;
                OnEnterTrainerFoV?.Invoke(collider);
            }
        }
    }

}
