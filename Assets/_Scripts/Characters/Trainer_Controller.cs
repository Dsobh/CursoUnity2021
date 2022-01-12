using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: esta clase se puede refactorizar, tiene codigo duplicado
public class Trainer_Controller : MonoBehaviour, Interactable
{
    [SerializeField] private Sprite trainerSprite;
    [SerializeField] private string trainerName;
    [SerializeField] private Dialog dialog, afterLoseDialog;
    [SerializeField] private GameObject exclamationMessage;
    [SerializeField] private GameObject fov;

    private bool trainerLostBattle = false;

    public Sprite TrainerSprite => trainerSprite;
    public string TrainerName => trainerName;

    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovDirection(_character._Animator.DefaultDirection);
    }

    private void Update()
    {
        _character.HandleUpdate();
    }

    IEnumerator ShowExclamationMark()
    {
        exclamationMessage.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        exclamationMessage.SetActive(false);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        yield return ShowExclamationMark();

        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.RoundToInt(moveVector.x), Mathf.RoundToInt(moveVector.y));
        yield return _character.MoveTowards(moveVector);

        DialogManager.SharedInstance.ShowDialog(dialog, () =>
        {
            GameManager.SharedInstance.StartTrainerBattle(this);
        });
    }

    public void SetFovDirection(FacingDirection direction)
    {
        //Angulo por defecto es mirar hacia abajo
        float angle = 0f;
        if(direction == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (direction == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if(direction == FacingDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public void Interact(Vector3 source)
    {
        if(!trainerLostBattle)
        {   
            StartCoroutine(ShowExclamationMark());
        }
        
        _character.LookTowards(source);

        if(!trainerLostBattle)
        {
            DialogManager.SharedInstance.ShowDialog(dialog, () =>
            {
            GameManager.SharedInstance.StartTrainerBattle(this);
            });
        }
        else
        {
            DialogManager.SharedInstance.ShowDialog(afterLoseDialog);
        }
        
    }

    public void AfterTrainerLostBattle()
    {
        trainerLostBattle = true;
        fov.gameObject.SetActive(false);
    }
}
