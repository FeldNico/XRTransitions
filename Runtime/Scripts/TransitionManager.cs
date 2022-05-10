using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{

    [SerializeField]
    private Camera _mainCamera;
    public Camera MainCamera => _mainCamera;
    
    [SerializeField]
    private Transform _leftEyeTransform;
    public Transform LeftEyeTransform => _leftEyeTransform;
    
    [SerializeField]
    private Transform _rightEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;

    private Transition _currentTransition = null;
    
    [SerializeReference]
    public List<Transition> Transitions;

    public Transition CurrentTransition => _currentTransition;
    
    private async void Initialization()
    {
        List<Task> tasks = new List<Task>();
        foreach (Transition transition in Transitions)
        {
            tasks.Add(transition.Initialization());
            transition.OnTransition += () =>
            {
                _currentTransition = transition;
            };
            transition.OnTransitionEnd += () =>
            {
                if (_currentTransition == transition)
                {
                    _currentTransition = null;
                }
            };
        }

        await Task.WhenAll(tasks);
    }

    private void Start()
    {
        Initialization();
    }
}
