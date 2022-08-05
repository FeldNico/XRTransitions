using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts
{
    public class Context : MonoBehaviour
    {
        public static UnityAction<Context> OnEnter;
        public static UnityAction<Context> OnExit;
    }
}