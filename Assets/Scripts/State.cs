using UnityEngine;

/// <summary>
/// Abstract base class for a simple, non-serializable state used by the StateMachine.
/// Derive from this and override Enter/Exit/Tick/FixedTick as needed.
/// </summary>
public abstract class State
{
    // The owning StateMachine. Set by the StateMachine when the state is activated.
    protected StateMachine Owner { get; private set; }

    internal void SetOwner(StateMachine owner) => Owner = owner;

    /// <summary>
    /// Called once when the state becomes active.
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// Called once when the state is replaced by another state.
    /// </summary>
    public virtual void Exit() { }

    /// <summary>
    /// Called every frame while the state is active.
    /// </summary>
    public virtual void Tick() { }

    /// <summary>
    /// Called every fixed frame while the state is active.
    /// </summary>
    public virtual void FixedTick() { }
}