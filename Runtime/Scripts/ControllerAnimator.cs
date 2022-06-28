using System;
using Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerAnimator: MonoBehaviour
{
    private Material _material;
    private TransitionManager _transitionManager;

    private void Awake()
    {
        _material = GetComponent<Renderer>().sharedMaterial;
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
        var c = _material.color;
        c.a = 1;
        _material.color = c;
    }

    public void Show(Transition transition)
    {
        var c = _material.color;
        c.a = 0;
        _material.color = c;
    }
}