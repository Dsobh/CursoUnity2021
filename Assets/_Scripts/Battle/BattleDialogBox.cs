using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private GameObject moveSelector;
    [SerializeField] private GameObject actionSelector;
    [SerializeField] private GameObject moveDetails;
    [SerializeField] private GameObject yesNoBox;

    [SerializeField] private List<TMP_Text> movementTexts;
    [SerializeField] private List<TMP_Text> actionTexts;

    [SerializeField] private TMP_Text ppText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text yesText;
    [SerializeField] private TMP_Text noText;

    [SerializeField] private float charactersPerSecond = 10f;
    [SerializeField] private float timeToWaitAfterText = 1.0f;

    public bool isWritting = false;

    public IEnumerator SetDialog(string message)
    {
        isWritting = true;
        dialogText.text = "";
        foreach (var character in message)
        {
            if(character != ' ')
            {
                SoundManager.SharedInstance.PlayRandomCharacterSound();
            }
            dialogText.text += character;
            yield return new WaitForSeconds(1/charactersPerSecond);
        }
        yield return new WaitForSeconds(timeToWaitAfterText);
        isWritting = false;
    }

    public void ToggleDialogText(bool activated)
    {
        dialogText.enabled = activated;
    }

    public void ToggleActionSelector(bool activated)
    {
        actionSelector.SetActive(activated);
    }

    public void ToggleMoveSelector(bool activated)
    {
        moveSelector.SetActive(activated);
        moveDetails.SetActive(activated);
    }

    public void ToggleYesNoBox(bool activated)
    {
        yesNoBox.SetActive(activated);
    }

    public void SelectYesNoAction(bool yesSelected)
    {
        if(yesSelected)
        {
            yesText.color = ColorManager.SharedInstance.selectedColor;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = ColorManager.SharedInstance.selectedColor;
        }
    }

    public void SelectAction(int selectedAction)
    {
        for(int i=0; i< actionTexts.Count; i++)
        {
            actionTexts[i].color = (i==selectedAction ? ColorManager.SharedInstance.selectedColor : Color.black);
        }
    }

    public void SelectMove(int selectedMove, Move move)
    {
        for(int i=0; i< movementTexts.Count; i++)
        {
            movementTexts[i].color = (i==selectedMove ? ColorManager.SharedInstance.selectedColor : Color.black);
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = $"Tipo/{move.Base.Type.ToString().ToUpper()}";

        ppText.color = ColorManager.SharedInstance.PPColor((float)move.PP/move.Base.PP);
    }

    public void SetPokemonMovements(List<Move> moves)
    {
        for(int i=0; i<movementTexts.Count; i++)
        {
            if(i< moves.Count)
            {
                movementTexts[i].text = moves[i].Base.name;
            }else
            {
                movementTexts[i].text = "---";
            }
        }
    }

}
