using UnityEngine;

/// <summary>
/// A Squirrel component that acts as a `StateMachine` for squirrel behaviors.
/// State implementations can pull components they need directly from the GameObject.
/// </summary>
public class Squirrel : StateMachine
{
    [Header("Movement")]
    [SerializeField, Tooltip("Base walk speed in units per second.")]
    private float walkSpeed = 1.5f;

    [SerializeField, Tooltip("Run speed multiplier applied to walk speed."), Range(1f, 4f)]
    private float runSpeedMultiplier = 2f;

    /// <summary>
    /// The base walk speed (units/sec).
    /// </summary>
    public float WalkSpeed
    {
        get => walkSpeed;
        set => walkSpeed = Mathf.Max(0f, value);
    }

    /// <summary>
    /// Multiplier applied to WalkSpeed when running.
    /// </summary>
    public float RunSpeedMultiplier
    {
        get => runSpeedMultiplier;
        set => runSpeedMultiplier = Mathf.Max(0f, value);
    }

    /// <summary>
    /// Convenience property for effective run speed (walkSpeed * multiplier).
    /// </summary>
    public float RunSpeed => WalkSpeed * RunSpeedMultiplier;

    /// <summary>
    /// Get the movement speed depending on whether the squirrel is running.
    /// </summary>
    public float GetSpeed(bool running) => running ? RunSpeed : WalkSpeed;

    // Input handling -------------------------------------------------------
    private InputSystem_Actions _inputActions;

    [Header("Effects")]
    [SerializeField, Tooltip("Optional particle system for movement trail.")]
    private ParticleSystem movementParticles;

    /// <summary>
    /// Particle system used for the movement trail (assign in inspector).
    /// </summary>
    public ParticleSystem MovementParticles
    {
        get => movementParticles;
        set => movementParticles = value;
    }

    /// <summary>
    /// Latest move input read from the input system (X=horizontal, Y=vertical).
    /// </summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>
    /// Whether the player is holding the sprint button.
    /// </summary>
    public bool IsSprinting => _inputActions != null && _inputActions.Player.Sprint.ReadValue<float>() > 0f;

    private void OnEnable()
    {
        if (_inputActions == null)
            _inputActions = new InputSystem_Actions();

        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Player.Disable();
    }

    private void Start()
    {
        // Initialize the state machine with the movement state
        Initialize(new SquirrelMove(this));
    }

    private void Update()
    {
        if (_inputActions != null)
            MoveInput = _inputActions.Player.Move.ReadValue<UnityEngine.Vector2>();

        // Squirrel overrides Update, so tick the state machine manually.
        CurrentState?.Tick();
    }
}
