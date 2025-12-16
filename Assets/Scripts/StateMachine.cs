using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A simple MonoBehaviour state machine. Call ChangeState(...) to switch states.
/// It will call the active state's Enter/Exit/Tick/FixedTick methods automatically.
/// Supports coroutine-based Enter/Exit and exposes state change events.
/// </summary>
public class StateMachine : MonoBehaviour
{
    private State _currentState;

    private bool _isTransitioning = false;
    private State _queuedState = null;

    /// <summary>
    /// The currently active state (can be null).
    /// </summary>
    public State CurrentState => _currentState;

    /// <summary>
    /// Event fired after a state change completes. (oldState, newState)
    /// </summary>
    public event Action<State, State> OnStateChanged;

    /// <summary>
    /// Event fired when a state has finished entering.
    /// </summary>
    public event Action<State> OnStateEntered;

    /// <summary>
    /// Event fired when a state has finished exiting.
    /// </summary>
    public event Action<State> OnStateExited;

    /// <summary>
    /// Optional debug logging for transitions.
    /// </summary>
    public bool DebugMode = false;

    /// <summary>
    /// Initialize the machine with a starting state.
    /// </summary>
    public void Initialize(State startingState)
    {
        ChangeState(startingState);
    }

    /// <summary>
    /// Change to a new state. If a transition is already in progress, the request will be queued
    /// (only one queued state is stored).
    /// </summary>
    public void ChangeState(State newState)
    {
        if (_currentState == newState) return;

        if (_isTransitioning)
        {
            // Overwrite previous queued state with the latest request
            _queuedState = newState;
            if (DebugMode) Debug.Log($"StateMachine: queued state change to {newState?.GetType().Name}");
            return;
        }

        StartCoroutine(RunChangeState(newState));
    }

    private IEnumerator RunChangeState(State newState)
    {
        _isTransitioning = true;
        var oldState = _currentState;

        if (oldState != null)
        {
            if (DebugMode) Debug.Log($"StateMachine: exiting {oldState.GetType().Name}");
            oldState.Exit();
            yield return StartCoroutine(oldState.ExitCoroutine());
            OnStateExited?.Invoke(oldState);
        }

        _currentState = newState;

        if (_currentState != null)
        {
            _currentState.SetOwner(this);
            if (DebugMode) Debug.Log($"StateMachine: entering {_currentState.GetType().Name}");
            _currentState.Enter();
            yield return StartCoroutine(_currentState.EnterCoroutine());
            OnStateChanged?.Invoke(oldState, _currentState);
            OnStateEntered?.Invoke(_currentState);
        }
        else
        {
            OnStateChanged?.Invoke(oldState, null);
        }

        _isTransitioning = false;

        // Process any queued state change
        if (_queuedState != null && _queuedState != _currentState)
        {
            var queued = _queuedState;
            _queuedState = null;
            StartCoroutine(RunChangeState(queued));
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