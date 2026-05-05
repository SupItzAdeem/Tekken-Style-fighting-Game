using UnityEngine;
using System.Collections;

public class AIMoveHero : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;

    public float speed = 4f;
    public float actionInterval = 2f;
    public float jumpHeight = 2.5f;
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;

    public float blockDistance = 1f;
    private float actionTimer = 0f;
    private int currentAction = -1;
    private bool isFlipped = false;

    public float attackRange = 2f;
    public int punchDamage = 5;
    public int kickDamage = 7;

    private Health myHealth;
    private bool isWalkingSoundPlaying = false;
    private bool hasDealtKickDamage = false;
    private bool hasDealtSuperKickDamage = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        myHealth = GetComponent<Health>();
        SetPlayerTag("Opponent");
    }

    public void SetPlayerTag(string tag)
    {
        this.gameObject.tag = tag;
    }

    void Update()
    {
        // Fix z position from slipping
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        bool isBlockedForward = player != null &&
            Physics.Raycast(transform.position, transform.forward, out RaycastHit hitForward, blockDistance) &&
            hitForward.collider.CompareTag("Player");

        bool isBlockedBackward = player != null &&
            Physics.Raycast(transform.position, -transform.forward, out RaycastHit hitBackward, blockDistance) &&
            hitBackward.collider.CompareTag("Player");

        if (player != null)
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float dot = Vector3.Dot(transform.forward, toPlayer.normalized);

            if (dot < -0.5f)
            {
                transform.Rotate(0f, 180f, 0f);
                isFlipped = !isFlipped;
            }
        }

        actionTimer -= Time.deltaTime;

        if (actionTimer <= 0f)
        {
            currentAction = Random.Range(0, 7);
            actionTimer = actionInterval;

            animator.SetBool("WalkForward", false);
            animator.SetBool("WalkBackward", false);
            animator.SetBool("CrouchIdle", false);

            bool canAttack = myHealth != null && !myHealth.IsTakingDamage && !IsInAttackAnimation();

            switch (currentAction)
            {
                case 0:
                    animator.SetBool("WalkForward", true);
                    break;
                case 1:
                    animator.SetBool("WalkBackward", true);
                    break;
                case 2:
                    animator.SetBool("CrouchIdle", true);
                    break;
                case 3:
                    if (canAttack)
                    {
                        animator.SetTrigger("PunchTrigger");
                        TryDealDamage("Player", punchDamage);
                        SoundManager.PlaySound(SoundType.PUNCH);
                    }
                    break;
                case 4:
                    if (canAttack)
                    {
                        animator.SetTrigger("KickTrigger");
                        hasDealtKickDamage = false;
                        SoundManager.PlaySound(SoundType.KICK);
                    }
                    break;
                case 5:
                    if (controller.isGrounded)
                    {
                        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                        animator.SetTrigger("JumpTrigger");
                        SoundManager.PlaySound(SoundType.JUMP);
                    }
                    break;
                case 6:
                    if (canAttack && controller.isGrounded)
                    {
                        animator.SetTrigger("SuperMoveKickTrigger");
                        hasDealtSuperKickDamage = false;
                        SoundManager.PlaySound(SoundType.SUPERKICK);
                    }
                    break;
            }
        }

        // Attack damage
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName("Kick") && !hasDealtKickDamage)
        {
            if (TryDealDamage("Player", kickDamage))
                hasDealtKickDamage = true;
        }

        if (currentState.IsName("SuperMoveKick") && !hasDealtSuperKickDamage)
        {
            if (TryDealDamage("Player", kickDamage * 2))
                hasDealtSuperKickDamage = true;
        }

        if (!currentState.IsName("Kick"))
            hasDealtKickDamage = false;

        if (!currentState.IsName("SuperMoveKick"))
            hasDealtSuperKickDamage = false;

        // Movement
        Vector3 move = Vector3.zero;
        bool isWalking = false;

        if (animator.GetBool("WalkForward"))
        {
            if (!isBlockedForward)
            {
                move += (isFlipped ? -transform.forward : transform.forward) * speed;
                isWalking = true;
            }
        }
        else if (animator.GetBool("WalkBackward"))
        {
            if (!isBlockedBackward)
            {
                move += (isFlipped ? transform.forward : -transform.forward) * speed;
                isWalking = true;
            }
        }

        if (isWalking && !isWalkingSoundPlaying)
        {
            SoundManager.PlayWalkingSound();
            isWalkingSoundPlaying = true;
        }
        else if (!isWalking && isWalkingSoundPlaying)
        {
            SoundManager.StopWalkingSound();
            isWalkingSoundPlaying = false;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // Clamp within camera bounds
        ClampToCameraBounds();

        // Enable root motion only when attacking
        animator.applyRootMotion = IsInAttackAnimation();
    }

    // Damage application
    bool TryDealDamage(string targetTag, int damageAmount)
    {
        GameObject target = GameObject.FindGameObjectWithTag(targetTag);
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damageAmount);
                return true;
            }
        }
        return false;
    }

    // Clamp player to within the visible camera bounds
    void ClampToCameraBounds()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        Vector3 clampedWorldPos = Camera.main.ViewportToWorldPoint(viewPos);
        transform.position = new Vector3(clampedWorldPos.x, transform.position.y, transform.position.z);
    }

    // Check if the current animation state is tagged as "Attack"
    bool IsInAttackAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsTag("Attack");
    }

    // Apply root motion movement
    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            Vector3 rootMotion = animator.deltaPosition;
            rootMotion.y = verticalVelocity * Time.deltaTime;
            controller.Move(rootMotion);
        }
    }
}
