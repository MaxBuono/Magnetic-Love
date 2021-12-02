using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartManagerSplit : MonoBehaviour
{
    //Externals
    public GameObject heartBorder;
    public GameObject heartImageBlue;
    public GameObject heartImageRed;
    public float maxProgress = 100f;
    public float increment = 50f;
    
    //Internals
    private float _progressBlue = 0f;
    private float _progressRed = 0f;
    private bool _blueIsOnGoal = false;
    private bool _redIsOnGoal = false;
    private bool _isGrowingBlue = false;
    private bool _isGrowingRed = false;
    private SpriteRenderer _spriteRendererBlue;
    private SpriteRenderer _spriteRendererRed;
    private Image _imageBlue;
    private Image _imageRed;

    private ulong idRedAudio = 0;
    private ulong idBlueAudio = 0;
    //I use this variable to not call levelCompleted more than once during the end level animation
    private bool _firstTimeReachMaxProgress = false;

    private void Start()
    {
        _spriteRendererBlue = heartImageBlue.GetComponent<SpriteRenderer>();
        _spriteRendererRed = heartImageBlue.GetComponent<SpriteRenderer>();
        _imageBlue = heartImageBlue.GetComponent<Image>();
        _imageRed = heartImageRed.GetComponent<Image>();
        _imageBlue.fillAmount = 0;
        _imageRed.fillAmount = 0;
    }

    private void Update()
    {
        //Here have to be the animation of the heart based on _progress
        
        if (_isGrowingBlue)
        {
            if (_progressBlue <= maxProgress)
            {
                _progressBlue += increment * Time.deltaTime;
                Debug.Log(idBlueAudio);
            }
        }
        else if(!_isGrowingBlue)
        {
            if (_progressBlue - increment * Time.deltaTime < 0)
            {
                _progressBlue = 0;
            }
            else
                _progressBlue -= increment * Time.deltaTime;
        }
        
        if (_isGrowingRed)
        {
            if (_progressRed <= maxProgress)
            {
                _progressRed += increment * Time.deltaTime;
                Debug.Log(idRedAudio);
            }
        }
        else if(!_isGrowingRed)
        {
            if (_progressRed - increment * Time.deltaTime < 0)
            {
                _progressRed = 0;
            }
            else
                _progressRed -= increment * Time.deltaTime;
        }
        
        if (_progressRed> maxProgress && _progressBlue > maxProgress && !_firstTimeReachMaxProgress)
        {
            //Go to next Level
            _firstTimeReachMaxProgress = true;
            GameManager.Instance.LevelCompleted();
        }

        if (!_firstTimeReachMaxProgress)
        {
            _imageBlue.fillAmount = _progressBlue/100;
            _imageRed.fillAmount = _progressRed/100;
        }
        
    }

    public void SetIsOnGoal(CharColor color, bool isOnGoal)
    {

        if (color == CharColor.Blue)
        {
            _isGrowingBlue = isOnGoal;
            if (isOnGoal) 
                idBlueAudio = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.heartCharge, Vector3.zero);
            else
                AudioManager.Instance.StopOneShotSound(idBlueAudio);
        }
        
        if (color == CharColor.Red)
        {
            _isGrowingRed = isOnGoal;
            if (isOnGoal)
                idRedAudio = AudioManager.Instance.PlayOneShotSound("SFX", AudioManager.Instance.heartCharge, Vector3.zero);
            else
                AudioManager.Instance.StopOneShotSound(idRedAudio);
        }
    }
}
