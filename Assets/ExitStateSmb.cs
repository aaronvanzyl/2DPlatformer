using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExitStateSmb : StateMachineBehaviour
{
    public UnityEvent ExitEvent;
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        ExitEvent.Invoke();
    }
}
