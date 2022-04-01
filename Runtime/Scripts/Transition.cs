using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        public abstract Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation);
        
        public abstract Task Initialization();
        
    }
}