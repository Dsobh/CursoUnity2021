using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : MonoBehaviour
{

    private CharacterAnimator _animator;
    public CharacterAnimator _Animator => _animator;
    public bool IsMoving{get; private set;}

    [SerializeField]
    private float speed;

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator MoveTowards(Vector2 moveVector, Action OnMoveFinish = null)
    {
        //Anulamos la dirección y, siempre tiene prioridad la X. Nunca pueden ser 1 y 1
        //Evitar posibles movimientos en diagonal
        if(moveVector.x != 0)
            moveVector.y = 0;

        //Por si acaso estamos entrando con vectores de movimiento que se salgan de -1 - 1
        _animator.moveX = Mathf.Clamp(moveVector.x, -1, 1);
        _animator.moveY = Mathf.Clamp(moveVector.y, -1, 1);

        Vector3 targetPosition = transform.position;
        //Si no clampeamos esto entonces podemos decidir cuantos pasos da el npc del tiron
        targetPosition.x += moveVector.x;
        targetPosition.y += moveVector.y;

        if(!IsPathAvailable(targetPosition))
        {
            yield break;
        }

        IsMoving = true;

        //Comparamos la distancia usando la precisión de la máquina
        while (Vector3.Distance(transform.position, targetPosition) > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null; //Esperar hasta el siguiente frame
        }

        transform.position = targetPosition;

        IsMoving = false;

        OnMoveFinish?.Invoke();
    }

    public void LookTowards(Vector3 target)
    {
        var diff = target - transform.position;
        var xDiff = Mathf.FloorToInt(diff.x);
        var yDiff = Mathf.FloorToInt(diff.y);

        if(xDiff == 0 | yDiff == 0)
        {
            _animator.moveX = Mathf.Clamp(xDiff, -1f, 1f);
            _animator.moveY = Mathf.Clamp(yDiff, -1f, 1f);
        }
        else
        {
            Debug.LogError("ERROR: El personaje no puede moverse ni mirar en diagonal.");
        }
    }

    public void HandleUpdate()
    {
        _animator.isMoving = IsMoving;
    }

    /// <summary>
    /// Se puede caminar por el camino o no
    /// </summary>
    /// <param name="target"></param>
    /// <returns>False si no; true si sí</returns>
    private bool IsPathAvailable(Vector3 target)
    {
        var path = target - transform.position;
        var direction = path.normalized;

        //Maginitud es que tan largo es el path, le quitamos uno porque no empieza en el centro del personaje si no una unidad adyacente segun hacia donde mire
        return !Physics2D.BoxCast(transform.position + direction, new Vector2(0.3f, 0.3f), 0f, direction, path.magnitude - 1, 
                            GameLayers.SharedInstance.CollisionLayers);
    }

    /// <summary>
    /// Comrpueba si el punto de destino está fuera de una colisión
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool IsAvailable(Vector3 target)
    {
        //El O | de la siguiente línea es a nivel de bit para la gestión de capas
        if (Physics2D.OverlapCircle(target, 0.15f, GameLayers.SharedInstance.SolidObjectsLayer 
                                                    | GameLayers.SharedInstance.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }
}
