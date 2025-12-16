using UnityEngine;

/// <summary>
/// Simple movement state for the Squirrel machine. Reads input from the machine
/// and moves the GameObject transform, flips sprite, and updates an Animator
/// "Speed" parameter when available.
/// </summary>
public class SquirrelMove : State
{
    private readonly Squirrel _machine;
    private readonly Transform _transform;
    private readonly SpriteRenderer _spriteRenderer;
    private readonly Animator _animator;

    public SquirrelMove(Squirrel machine)
    {
        _machine = machine;
        _transform = machine.transform;
        _spriteRenderer = machine.GetComponent<SpriteRenderer>();
        _animator = machine.GetComponent<Animator>();
    }

    public override void Enter()
    {
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
            _animator.SetBool("isRunning", false);
        }
    }

    public override void Exit()
    {
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
            _animator.SetBool("isRunning", false);
        }
    }

    public override void Tick()
    {
        var input = _machine.MoveInput;

        if (input.sqrMagnitude < 0.0001f)
        {
            if (_animator != null)
            {
                _animator.SetFloat("Speed", 0f);
                _animator.SetBool("isRunning", false);
            }
            return;
        }

        bool running = _machine.IsSprinting;
        float speed = _machine.GetSpeed(running);
        Vector3 delta = new Vector3(input.x, input.y, 0f) * speed * Time.deltaTime;
        _transform.position += delta;

        // Flip sprite based on horizontal input
        if (_spriteRenderer != null && Mathf.Abs(input.x) > 0.01f)
            _spriteRenderer.flipX = input.x < 0f;

        // Update animator speed parameter (if available)
        if (_animator != null)
        {
            _animator.SetFloat("Speed", input.magnitude * speed);
            _animator.SetBool("isRunning", true);
        }
    }
}
