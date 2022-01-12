using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public GameObject healthBar;
    public TMP_Text currentHealth, maxHealth;

    /// <summary>
    /// Actualiza la barra de vida a partir del valor normalizado de la misma
    /// </summary>
    /// <param name="normalizedValue">Valor de la vida normalizado entre 0 y 1</param>
    public void SetHP(float normalizedValue)
    {
        healthBar.transform.localScale = new Vector3(normalizedValue, 1.0f, 1);
        healthBar.GetComponent<Image>().color = ColorManager.SharedInstance.BarColor(normalizedValue);
    }

    /// <summary>
    /// Establece el texto de la salud y la barra de vida aplicandole una animación suave.
    /// </summary>
    /// <param name="pokemon">Pokemon sobre el que se actualizan los cambios del HUD</param>
    /// <returns></returns>
    public IEnumerator SetSmoothHP(Pokemon pokemon)
    {
        maxHealth.text = $"/{pokemon.MaxHP}";
        float normalizedValue = pokemon.HP/(float)pokemon.MaxHP;
        var seq = DOTween.Sequence();
        seq.Append(healthBar.transform.DOScaleX(normalizedValue, 1f));
        seq.Join(healthBar.GetComponent<Image>().DOColor(ColorManager.SharedInstance.BarColor(normalizedValue), 1f));
        seq.Join(currentHealth.DOCounter(pokemon.previousHPValue, pokemon.HP, 1.0f));
        yield return seq.WaitForCompletion();
    }
}


//Mismo método pero sin DOTween
        /*float currentScale = healthBar.transform.localScale.x;
        float updateQuantity = currentScale - normalizedValue;
        
        while(currentScale - normalizedValue > Mathf.Epsilon)
        {
            currentScale -= updateQuantity * Time.deltaTime;
            healthBar.transform.localScale = new Vector3(currentScale, 1);
            healthBar.GetComponent<Image>().color = barColor;
            yield return null;
        }
        
        //Asegurarnos de que al final del frame se actualiza correctamente
        healthBar.transform.localScale = new Vector3(normalizedValue, 1);*/