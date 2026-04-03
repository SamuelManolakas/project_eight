using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State currentState = null;
    private Stack<State> stateStack = new Stack<State>();
    
    public void Transit(State toState){
        State exitState = currentState;
        State enterState = toState;
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

    public void InitializeMachine(State State)
    {
        currentState = State;
        currentState.Enter();
    }
}