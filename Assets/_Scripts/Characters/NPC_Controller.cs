using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcState
{
    Idle,
    Walking,
    Talking
}

public class NPC_Controller : MonoBehaviour, Interactable //Implementando una interfaz
{

    [SerializeField] Dialog dialog;

    private NpcState state;
    [SerializeField] private float idleTime = 2f; //Tiempo que estará en estado Idle
    private float idleTimer = 0f; //Contador
    [SerializeField] private List<Vector2> moveDirections;
    private int currentDirection;

    Character _character;

    void Awake()
    {
        _character = GetComponent<Character>();
    }

    public void Interact(Vector3 source)
    {
        if(state == NpcState.Idle)
        {
            state = NpcState.Talking;
            _character.LookTowards(source);
            DialogManager.SharedInstance.ShowDialog(dialog, () =>
            {
                idleTimer = 0f;
                state = NpcState.Idle;
            });
        }
            
    }

    void Update()
    {
        if (state == NpcState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                idleTimer = 0f;
                StartCoroutine(Walk());

            }
        }
        _character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NpcState.Walking;

        var oldPosition = transform.position;
        var direction = Vector2.zero;

        if (moveDirections.Count > 0)
        {
            direction = moveDirections[currentDirection];
        }
        else
        {
            //Vector 2 aleatorio
        }

        yield return _character.MoveTowards(direction);

        //Solo pasamos al siguiente punto de la ruta si puede moverse y no choca con el player
        if(moveDirections.Count > 0 && transform.position != oldPosition)
        {
            currentDirection = (currentDirection + 1) % moveDirections.Count;
        }

        state = NpcState.Idle;
    }
}
