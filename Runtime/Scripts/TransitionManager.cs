using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{

    private Transition _currentTransition = null;
    
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
