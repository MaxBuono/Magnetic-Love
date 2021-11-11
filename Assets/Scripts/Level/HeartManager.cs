using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartManager : MonoBehaviour
{
    //Externals
    public float maxProgress = 100f;
    public float increment = 50f;
    
    //Internals
    public float _progress = 0f;
    private bool _blueIsOnGoal = false;
    private bool _redIsOnGoal = false;
    private bool _isGrowing = false;
    private SpriteRenderer _spriteRenderer;
    //I use this variable to not call levelCompleted more than once during the end level animation
    private bool _firstTimeReachMaxProgress = true;

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
            _progress += increment * Time.fixedDeltaTime;
            if (_progress > maxProgress && _firstTimeReachMaxProgress)
            {
                //Go to next Level
                _firstTimeReachMaxProgress = false;
                GameManager.Instance.LevelCompleted();
            }
        }
        else
        {
            if (_progress - increment * Time.fixedDeltaTime < 0)
            {
                _progress = 0;
            }else _progress -= increment * Time.fixedDeltaTime;
        }
    }

    public void setBooleans(CharColor _color, bool _isOnGoal)
    {
        if (_color == CharColor.Blue)
            _blueIsOnGoal = _isOnGoal;
        else if (_color == CharColor.Red)
            _redIsOnGoal = _isOnGoal;
        if (_blueIsOnGoal && _redIsOnGoal)
            _isGrowing = true;
        else _isGrowing = false;
    }
}
