using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartManager : MonoBehaviour
{
    //Externals
    public GameObject heartBorder;
    public GameObject heartImage;
    public float maxProgress = 100f;
    public float increment = 50f;
    
    //Internals
    private float _progress = 0f;
    private bool _blueIsOnGoal = false;
    private bool _redIsOnGoal = false;
    private bool _isGrowing = false;
    private SpriteRenderer _spriteRenderer;
    private Image _image;
    //I use this variable to not call levelCompleted more than once during the end level animation
    private bool _firstTimeReachMaxProgress = false;

    private void Start()
    {
        _spriteRenderer = heartImage.GetComponent<SpriteRenderer>();
        _image = heartImage.GetComponent<Image>();
        _image.fillAmount = 0;
    }

    private void Update()
    {
        //Here have to be the animation of the heart based on _progress
        
        if (_isGrowing)
        {
            _progress += increment * Time.deltaTime;
            if (_progress > maxProgress && !_firstTimeReachMaxProgress)
            {
                //Go to next Level
                _firstTimeReachMaxProgress = true;
                GameManager.Instance.LevelCompleted();
            }
        }
        else
        {
            if (_progress - increment * Time.deltaTime < 0)
            {
                _progress = 0;
            }
            else
                _progress -= increment * Time.deltaTime;
        }

        if (!_firstTimeReachMaxProgress)
        {
            _image.fillAmount = _progress/100;
        }

        

    }

    public void SetIsOnGoal(CharColor color, bool isOnGoal)
    {
        if (color == CharColor.Blue)
            _blueIsOnGoal = isOnGoal;
        else if (color == CharColor.Red)
            _redIsOnGoal = isOnGoal;
        if (_blueIsOnGoal && _redIsOnGoal)
            _isGrowing = true;
        else _isGrowing = false;
    }
}
