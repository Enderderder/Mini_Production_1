using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public abstract class StateMachine : MonoBehaviour
{
    Dictionary<StateType, IState> _states = new Dictionary<StateType, IState>();
    IState CurrentState = null;
    public string currentIstate;
    // Registration
    public void Register(IState state)
    {
        _states[state.ID] = state;
    }
    public bool SetStartState(StateType initState)
    {
        if (_states.ContainsKey(initState))
        {
            CurrentState = _states[initState];
            CurrentState.OnStart();
            return true;
        }
        else
            return false;
    }
    // Change State
    public bool ChangeState(StateType newStateID, string current)
    {
        //check if state is registered
        if (!_states.ContainsKey(newStateID)
            || CurrentState.ID == newStateID)//to prevent transitioning with the same state
            return false;

        //if true, execute currentstate.OnEnd
        CurrentState.OnEnd();

        //then, execute newstate.Onstart
        IState newstate = _states[newStateID];
        newstate.OnStart();

        //set currentState with newState
        CurrentState = newstate;
        currentIstate = current;

        return true;

    }

    // Update
    public void UpdateMachine()
    {
        if (CurrentState != null)
            CurrentState.OnUpdate();
    }
}

public enum StateType
{
    Idle,
    Patroling,
    Chasing,
    Engaged,
    Attacking,
    Dead,
}
        
public abstract class IState
{
    protected StateMachine Machine;
    public IState(StateMachine machine)
    {
        Machine = machine;
    }
    public abstract string Name{ get; }
    public abstract StateType ID { get; }
    public abstract void OnStart();
    public abstract void OnUpdate();
    public abstract void OnEnd();

}

