using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection {Down, Up, Left, Right}

public class CharacterAnimator : MonoBehaviour
{
    public float moveX, moveY;
    public bool isMoving;

    [SerializeField] List<Sprite> walkDownSprites, walkUpSprites, walkLeftSprites, walkRightSprites;
    private CustomAnimator walkDownAnim, walkUpAnim, walkLeftAnim, walkRightAnim;
    private CustomAnimator currentAnimator;

    [SerializeField] private FacingDirection defaultDirection = FacingDirection.Down;
    public FacingDirection DefaultDirection => defaultDirection;

    private SpriteRenderer renderer;

    bool wasPreviouslyMoving = false;

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new CustomAnimator(renderer, walkDownSprites);
        walkUpAnim = new CustomAnimator(renderer, walkUpSprites);
        walkLeftAnim = new CustomAnimator(renderer, walkLeftSprites);
        walkRightAnim = new CustomAnimator(renderer, walkRightSprites);
        
        SetFacingDirection(defaultDirection);
        currentAnimator = walkDownAnim;
    }

    private void Update()
    {
        var previousAnimator = currentAnimator;
        if(moveX == 1)
        {
            currentAnimator = walkRightAnim;
        }
        else if(moveX == -1)
        {
            currentAnimator = walkLeftAnim;
        }
        else if(moveY == 1)
        {   
            currentAnimator = walkUpAnim;
        }
        else if(moveY == -1)
        {
            currentAnimator = walkDownAnim;
        }

        if(previousAnimator != currentAnimator || isMoving != wasPreviouslyMoving)
        {
            currentAnimator.Start();
        }

        if(isMoving)
        {
            currentAnimator.HandleUpdate();
        }
        else
        {
            renderer.sprite = currentAnimator.AnimationFrames[0];
        }

        wasPreviouslyMoving = isMoving;
    }

    public void SetFacingDirection(FacingDirection direction)
    {
        if(direction == FacingDirection.Down)
        {
            moveY = -1;
        }
        else if(direction == FacingDirection.Up)
        {
            moveY = 1;
        }
        else if(direction == FacingDirection.Left)
        {
            moveX = -1;
        }
        else if(direction == FacingDirection.Right)
        {
            moveX = 1;
        }
    }
}
