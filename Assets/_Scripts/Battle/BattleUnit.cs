using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    public PokemonBase _base;
    public int _level;
    public Pokemon _pokemon {get; set;}
    [SerializeField] private bool isPlayer;
    public bool IsPlayer => isPlayer;

    [SerializeField] BattleHUD hud;
    public BattleHUD Hud => hud;

    private Vector3 initialPosition;
    private Color initialColor;

    [SerializeField] float startTimeAnimation = 1f, attackTimeAnimation = 0.3f, faintTimeAnimation = 1f, hitTimeAnimation = 0.15f, captureTimeAnimation = 0.4f;

    Image pokemonImage;

    void Awake()
    {
        pokemonImage = GetComponent<Image>();
        initialPosition = pokemonImage.transform.localPosition; //No movemos el objeto padre si no solo la imagen
        initialColor = pokemonImage.color;
    }

    public void SetupPokemon(Pokemon pokemon)
    {
        _pokemon = pokemon;

        pokemonImage.sprite = (isPlayer ? _pokemon.Base.BackSprite : _pokemon.Base.FrontSprite);

        pokemonImage.color = initialColor;

        hud.gameObject.SetActive(true);
        hud.SetPokemonData(pokemon);
        transform.localScale = new Vector3(1, 1, 1);

        PlayStartAnimation();
    }

    public void PlayStartAnimation()
    {
        pokemonImage.transform.localPosition = new Vector3(initialPosition.x + (isPlayer ? -1:1) * 400, initialPosition.y);

        pokemonImage.transform.DOLocalMoveX(initialPosition.x, startTimeAnimation);
    }

    public void PlayAttackAnimation()
    {
        //DOTween sequence -> dos animaciones una detras de otra
        //DoTween Join -> dos animaciones juntas
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer? 1:-1)*60, attackTimeAnimation));
        seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x, attackTimeAnimation));
    }

    public void PlayReceiveDamageAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOColor(Color.gray, hitTimeAnimation));
        seq.Append(pokemonImage.DOColor(initialColor, hitTimeAnimation));
    }

    public void PlayFaintAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.transform.DOLocalMoveY(initialPosition.y - 200, faintTimeAnimation));
        seq.Join(pokemonImage.DOFade(0, faintTimeAnimation));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOFade(0, captureTimeAnimation));
        seq.Join(transform.DOScale(new Vector3(0.25f, 0.25f, 1), captureTimeAnimation));
        seq.Join(transform.DOLocalMoveY(initialPosition.y+50, captureTimeAnimation));
        yield return seq.WaitForCompletion();
    }

    public IEnumerator PlayBrakeOutAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOFade(1, captureTimeAnimation));
        seq.Join(transform.DOScale(new Vector3(1f, 1f, 1), captureTimeAnimation));
        seq.Join(transform.DOLocalMoveY(initialPosition.y, captureTimeAnimation));
        yield return seq.WaitForCompletion();
    }

    public void ClearHUD()
    {
        hud.gameObject.SetActive(false);
    }
}
