using UnityEngine;

/// <summary>
/// A simple MonoBehaviour state machine. Call ChangeState(...) to switch states.
/// It will call the active state's Enter/Exit/Tick/FixedTick methods automatically.
/// </summary>
public class StateMachine : MonoBehaviour
{
    private State _currentState;

    /// <summary>
    /// The currently active state (can be null).
    /// </summary>
    public State CurrentState => _currentState;

    /// <summary>
    /// Initialize the machine with a starting state.
    /// </summary>
    public void Initialize(State startingState)
    {
        ChangeState(startingState);
    }

    /// <summary>
    /// Change to a new state. Exits the current state (if any) before Enter on the new state.
    /// </summary>
    public void ChangeState(State newState)
    {
        if (_currentState == newState) return;

        if (_currentState != null)
            _currentState.Exit();

        _currentState = newState;

        if (_currentState != null)
        {
            _currentState.SetOwner(this);
            _currentState.Enter();
        }
    }

    private void Update()
    {
        _currentState?.Tick();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedTick();
    }
}