using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;

public class BattleHUD : MonoBehaviour
{

    public TMP_Text pokemonName;
    public TMP_Text pokemonLevel;
    public HealthBar _healthBar;
    private Pokemon _pokemon;
    public GameObject statusBox;

    public GameObject expBar;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        pokemonName.text = pokemon.Base.Name;
        SetLevelText();
        _healthBar.SetHP(pokemon.HP/pokemon.MaxHP);
        SetExp();
        StartCoroutine(UpdatePokemonData());
        SetStatusConditionData();
        _pokemon.OnStatusConditionChanged += SetStatusConditionData;
    }

    public IEnumerator UpdatePokemonData()
    {
        if(_pokemon.hasHPChanged)
        {
           yield return _healthBar.SetSmoothHP(_pokemon);
            _pokemon.hasHPChanged = false;
        }
    }

    public void SetExp()
    {
        if(expBar == null)
        {
            return;
        }

        expBar.transform.localScale = new Vector3(NormalizeExp(), expBar.transform.localScale.y, 1f);
    }

    public IEnumerator SetSmoothExp(bool needToResetBar = false) //valor por defecto
    {
        if(expBar == null)
        {
            yield break;
        }

        if(needToResetBar)
        {
            expBar.transform.localScale = new Vector3(0, expBar.transform.localScale.y, 1f);
        }

        yield return expBar.transform.DOScaleX(NormalizeExp(), 2f).WaitForCompletion();

    }

    private float NormalizeExp()
    {
        float currentLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level); //min
        float nextLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level + 1); //max

        float normalizedExp = ((_pokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp));

        return Mathf.Clamp01(normalizedExp);
    }

    public void SetLevelText()
    {
        pokemonLevel.text = $"Lv {_pokemon.Level}";
    }

    void SetStatusConditionData()
    {
        if(_pokemon.StatusCondition == null)
        {
            statusBox.SetActive(false);
        }
        else
        {
            statusBox.SetActive(true);
            statusBox.GetComponent<Image>().color = ColorManager.StatusConditionColor.GetColorFromStatusCondition(_pokemon.StatusCondition.Id);
            statusBox.GetComponentInChildren<TMP_Text>().text = _pokemon.StatusCondition.Id.ToString().ToUpper();
        }
    }
}
