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
    private readonly ParticleSystem _movementParticles;
    private readonly Transform _particleTransform;
    private readonly Vector3 _particleBaseEuler;
    private bool _particlesPlaying = false;
    private int _lastFacing = 1;

    public SquirrelMove(Squirrel machine)
    {
        _machine = machine;
        _transform = machine.transform;
        _spriteRenderer = machine.GetComponent<SpriteRenderer>();
        _animator = machine.GetComponent<Animator>();

        _movementParticles = machine.MovementParticles != null
            ? machine.MovementParticles
            : machine.GetComponentInChildren<ParticleSystem>();
        if (_movementParticles != null)
        {
            _particleTransform = _movementParticles.transform;
            _particleBaseEuler = _particleTransform.localEulerAngles;
            _movementParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            _particleTransform = null;
            _particleBaseEuler = Vector3.zero;
        }
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

        if (_movementParticles != null)
        {
            _movementParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _particlesPlaying = false;
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

            UpdateParticleState(false, 0f);
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

        UpdateParticleState(true, input.x);
    }

    private void UpdateParticleState(bool moving, float horizontalInput)
    {
        if (_movementParticles == null)
            return;

        if (!moving)
        {
            if (_particlesPlaying)
            {
                _movementParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _particlesPlaying = false;
            }
            return;
        }

        // Track last horizontal facing direction when input is present
        if (Mathf.Abs(horizontalInput) > 0.01f)
            _lastFacing = horizontalInput < 0f ? -1 : 1;

        // Flip particle emission rotation when facing changes
        if (_particleTransform != null)
        {
            // Mirror the emission by rotating 180° around Y when facing left.
            float yOffset = _lastFacing == -1 ? 180f : 0f;
            var newRot = new Vector3(_particleBaseEuler.x, _particleBaseEuler.y + yOffset, _particleBaseEuler.z);
            _particleTransform.localRotation = Quaternion.Euler(newRot);
        }

        if (!_particlesPlaying)
        {
            _movementParticles.Play();
            _particlesPlaying = true;
        }
    }
}
