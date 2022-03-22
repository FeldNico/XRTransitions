using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{

    private List<Context> _contexts;
    public List<Context> Contexts
    {
        get
        {
            if (_contexts == null)
            {
                _contexts = FindObjectsOfType<Context>().ToList();
            }

            return _contexts;
        }
    }

    [SerializeField]
    private List<Transition> _transitions;
    
    
    [SerializeField]
    private Context _currentContext;
    public Context CurrentContext
    {
        get
        {
            if (_currentContext == null )
            {
                Initialization();
            }

            return _currentContext;
        }
        set
        {
            if (value != _currentContext)
            {
                if (_currentContext != null)
                {
                    _currentContext.OnDeactivation?.Invoke();
                }
                if (value != null)
                {
                    value.OnActivation?.Invoke();
                }
            }
            _currentContext = value;
        }
    }

    private void Initialization()
    {
        List<Task> tasks = new List<Task>();
        foreach (Transition transition in _transitions)
        {
            tasks.Add(transition.Initialization());
        }

        Task.WhenAll(tasks.ToArray()).ContinueWith(_ =>
        {
            if (_currentContext == null)
            {
                CurrentContext = Contexts[0];
            }

            foreach (var c in Contexts.Where(context => context != _currentContext))
            {
                c.OnDeactivation?.Invoke();
            }
        });
    }

    private void Start()
    {
        Initialization();
    }
}
