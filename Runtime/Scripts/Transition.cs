using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts
{
    public abstract class Transition: MonoBehaviour
    {
        public UnityAction OnTransition;

        public UnityAction OnTransitionEnd;
        
        public abstract Context StartContext { get; protected set; }
        
        public abstract Context TargetContext { get; protected set; }
        
        public abstract bool IsTransitioning  { get; protected set; }

        protected abstract Task TriggerTransition();
        
        public abstract Task Initialization();
        
    }
}