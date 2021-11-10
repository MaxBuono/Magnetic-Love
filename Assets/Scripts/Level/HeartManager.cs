using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartManager : MonoBehaviour
{
    //Externals
    public float _maxProgress = 100f;
    public float _increment = 50f;
    
    //Internals
    public float _progress = 0f;
    private bool blueIsOnGoal = false;
    private bool redIsOnGoal = false;
    private bool _isGrowing = false;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //Here have to be the animation of the heart based on _progress
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (_isGrowing)
        {
            _progress += _increment * Time.fixedDeltaTime;
            if (_progress > _maxProgress)
            {
                //Go to next Level
                GameManager.Instance.LevelCompleted();
            }
        }
        else
        {
            if (_progress - _increment * Time.fixedDeltaTime < 0)
            {
                _progress = 0;
            }else _progress -= _increment * Time.fixedDeltaTime;
        }
    }

    public void setBooleans(CharColor _color, bool _isOnGoal)
    {
        if (_color == CharColor.Blue)
            blueIsOnGoal = _isOnGoal;
        else if (_color == CharColor.Red)
            redIsOnGoal = _isOnGoal;
        if (blueIsOnGoal && redIsOnGoal)
            _isGrowing = true;
        else _isGrowing = false;
    }
}
