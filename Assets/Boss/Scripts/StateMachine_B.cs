using System.Collections.Generic;
using UnityEngine;

public class StateMachine_B : MonoBehaviour
{
    public State_B currentState = null;
    private Stack<State_B> stateStack = new Stack<State_B>();
    
    public void Transit(State_B toState){
        State_B exitState = currentState;
        State_B enterState = toState;
        stateStack.Clear();
        while(enterState.level < exitState.level){
            exitState.Exit();
            exitState = exitState.parent;
        }
        while(exitState.level < enterState.level){
            stateStack.Push(enterState);
            enterState = enterState.parent;
        }
        while(exitState != enterState){
            exitState.Exit();
            stateStack.Push(enterState);
            exitState = exitState.parent;
            enterState = enterState.parent;
        }
        while(0 < stateStack.Count){
            stateStack.Pop().Enter();
        }
        currentState = toState;
    }

    public void InitializeMachine(State_B State)
    {
        currentState = State;
        currentState.Enter();
    }
}