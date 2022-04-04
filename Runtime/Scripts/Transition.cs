using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts
{
    [Serializable]
    public abstract class Transition : MonoBehaviour

    {
        public UnityAction OnTransition;

        public UnityAction OnTransitionEnd;
        
        public abstract bool IsTransitioning { get; protected set; }

        public abstract Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation);

        public abstract Task Initialization();
    }
}