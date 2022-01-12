using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimator
{
    private SpriteRenderer renderer;
    private List<Sprite> animationFrames;
    public List<Sprite> AnimationFrames => animationFrames;
    private float frameRate;

    private int currentFrame;
    float timer;


    public CustomAnimator(SpriteRenderer renderer, List<Sprite> animationFrames, float frameRate = 0.25f)
    {
        this.renderer = renderer;
        this.animationFrames = animationFrames;
        this.frameRate = frameRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0f;
        renderer.sprite = animationFrames[currentFrame];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if(timer > frameRate)
        {
            currentFrame = (currentFrame+1) % animationFrames.Count;
            renderer.sprite = animationFrames[currentFrame];
            timer -= frameRate; //Decrementamos en lugar de fijarlo a 0
        }
    }
}
