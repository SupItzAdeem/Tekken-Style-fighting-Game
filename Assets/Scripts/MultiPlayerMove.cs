using UnityEngine;

public class MultiplayerController : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    private Health myHealth;

    public float speed = 4f;
    public float jumpHeight = 2.5f;
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;

    public float attackRange = 2f;
    public int punchDamage = 5;
    public int kickDamage = 7;
    public float blockDistance = 1f;

    private bool isWalkingSoundPlaying = false;
    private bool hasDealtKickDamage = false;
    private bool hasDealtSuperKickDamage = false;

    private int hurricaneHitCount = 0;
    private float hurricaneHitTimer = 0f;
    private float hurricaneHitInterval = 0.3f;
    private bool isHurricaneActive = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        myHealth = GetComponent<Health>();

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;

        GameObject opponent = GameObject.FindGameObjectWithTag("Player");
        if (!opponent) return;

        float horizontal = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;

        Vector3 move = Vector3.right * horizontal * speed;
        bool isWalking = horizontal != 0;

        animator.SetBool("WalkForward", horizontal > 0);
        animator.SetBool("WalkBackward", horizontal < 0);

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

        animator.SetBool("CrouchIdle", Input.GetKey(KeyCode.DownArrow));

        if (myHealth != null && myHealth.IsTakingDamage) return;

        bool canAttack = !IsInAttackAnimation();

        if (canAttack && Input.GetKeyDown(KeyCode.N))
        {
            animator.SetTrigger("PunchTrigger");
            TryDealDamage("Player", punchDamage);
            SoundManager.PlaySound(SoundType.PUNCH);
        }

        if (canAttack && Input.GetKeyDown(KeyCode.M))
        {
            animator.SetTrigger("KickTrigger");
            hasDealtKickDamage = false;
            SoundManager.PlaySound(SoundType.KICK);
        }

        if (canAttack && Input.GetKeyDown(KeyCode.K))
        {
            animator.SetTrigger("SuperMoveKickTrigger");
            hasDealtSuperKickDamage = false;
            SoundManager.PlaySound(SoundType.SUPERKICK);
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

         if (state.IsName("Hurricane"))
        {
            isHurricaneActive = true;
            hurricaneHitTimer += Time.deltaTime;

            if (hurricaneHitCount < 3 && hurricaneHitTimer >= hurricaneHitInterval)
            {
                if (TryDealDamage("Player", punchDamage))
                {
                    hurricaneHitCount++;
                    SoundManager.PlaySound(SoundType.HURRICANE);
                }
                hurricaneHitTimer = 0f;
            }
        }
        else if (isHurricaneActive)
        {
            isHurricaneActive = false;
            hurricaneHitCount = 0;
            hurricaneHitTimer = 0f;
        }

        if (state.IsName("Kick") && !hasDealtKickDamage && TryDealDamage("Player", kickDamage))
            hasDealtKickDamage = true;

        if (state.IsName("SuperMoveKick") && !hasDealtSuperKickDamage && TryDealDamage("Player", kickDamage * 2))
            hasDealtSuperKickDamage = true;

        if (!state.IsName("Kick")) hasDealtKickDamage = false;
        if (!state.IsName("SuperMoveKick")) hasDealtSuperKickDamage = false;

        if (controller.isGrounded && Input.GetKeyDown(KeyCode.UpArrow))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
            SoundManager.PlaySound(SoundType.JUMP);
        }
        if (canAttack && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.L))
        {
            animator.SetTrigger("HurricaneTrigger");
            hasDealtSuperKickDamage = false;
                //SoundManager.PlaySound(SoundType.HURRICANE);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        Vector3 toOpponent = opponent.transform.position - transform.position;
        if (Vector3.Dot(transform.forward, toOpponent.normalized) < -0.5f)
        {
            transform.Rotate(0f, 180f, 0f);
        }

        ClampToCameraBounds();
        animator.applyRootMotion = IsInAttackAnimation();
    }

    void ClampToCameraBounds()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        Vector3 clampedWorldPos = Camera.main.ViewportToWorldPoint(viewPos);
        transform.position = new Vector3(clampedWorldPos.x, transform.position.y, transform.position.z);
    }

    bool TryDealDamage(string targetTag, int damage)
    {
        GameObject target = GameObject.FindGameObjectWithTag(targetTag);
        if (target && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            Health hp = target.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                return true;
            }
        }
        return false;
    }

    bool IsInAttackAnimation()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
    }

    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            Vector3 motion = animator.deltaPosition;
            motion.y = verticalVelocity * Time.deltaTime;
            controller.Move(motion);
        }
    }
}
