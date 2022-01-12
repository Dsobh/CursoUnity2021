using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private float charactersPerSecond = 10f;
    private int currentLine = 0;
    private Dialog currentDialog;
    bool isWriting;

    public bool IsBeingShown; //Si el dialogo se está mostrando o no
    private Action onDialogClose;

    //Singleton
    public static DialogManager SharedInstance;

    public event Action OnDialogStart, OnDialogFinish;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 0.01f;

    void Awake()
    {
        if(SharedInstance == null)
        {
            SharedInstance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if(Input.GetAxisRaw("Submit") != 0 && !isWriting)
        {
            if(timeSinceLastClick >= timeBetweenClicks)
            {
                timeSinceLastClick = 0;
                currentLine++;
                if(currentLine < currentDialog.Lines.Count)
                {
                    StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));
                }
                else
                {
                    currentLine = 0;
                    IsBeingShown = false;
                    dialogBox.SetActive(false);
                    onDialogClose?.Invoke();
                    OnDialogFinish?.Invoke();
                }
            }
        }
    }

    public void ShowDialog(Dialog dialog, Action onDialogFinish = null)
    {
        OnDialogStart?.Invoke();
        dialogBox.SetActive(true);
        currentDialog = dialog;
        IsBeingShown = true;
        this.onDialogClose = onDialogFinish;
        StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));
    }

    public IEnumerator SetDialog(string line)
    {
        dialogText.text = "";
        foreach (var character in line)
        {
            isWriting = true;
            if(character != ' ')
            {
                SoundManager.SharedInstance.PlayRandomCharacterSound();
            }
            dialogText.text += character;
            yield return new WaitForSeconds(1/charactersPerSecond);
        }
        isWriting = false;
    }
}
