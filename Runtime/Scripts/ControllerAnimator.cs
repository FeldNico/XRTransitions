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
        Transition.OnStartTransition += _ => Hide();
        Transition.OnEndTransition += _ => Show();
    }

    private void OnDisable()
    {
        Transition.OnStartTransition -= _ => Hide();
        Transition.OnEndTransition -= _ => Show();
    }

    public void Hide()
    {
        var c = _renderer.material.color;
        c.a = 0;
        _renderer.material.color = c;
        IsHidden = true;
    }

    public void Show()
    {
        var c = _renderer.material.color;
        c.a = 1;
        _renderer.material.color = c;
        IsHidden = false;
    }
}