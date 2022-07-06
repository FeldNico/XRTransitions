using System;
using Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerAnimator: MonoBehaviour
{
    private Renderer _renderer;
    private TransitionManager _transitionManager;

    public bool IsHidden { private set; get; }
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _transitionManager = FindObjectOfType<TransitionManager>();
    }

    private void OnEnable()
    {
        _transitionManager.OnStartTransition += Hide;
        _transitionManager.OnEndTransition += Show;
    }
    
    private void OnDisable()
    {
        _transitionManager.OnStartTransition -= Hide;
        _transitionManager.OnEndTransition -= Show;
    }

    public void Hide(Transition transition)
    {
        var c = _renderer.material.color;
        c.a = 0;
        _renderer.material.color = c;
        IsHidden = true;
    }

    public void Show(Transition transition)
    {
        var c = _renderer.material.color;
        c.a = 1;
        _renderer.material.color = c;
        IsHidden = false;
    }
}