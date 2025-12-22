using UnityEngine;
using UnityEngine.InputSystem;

public class SenseiController : MonoBehaviour
{
    [Header("Ses Efektleri")]
    public AudioSource footstepSource;
    public AudioSource combatSource;
    public AudioClip swingSound;    
    public AudioClip walkLoopSound;
    public AudioClip runLoopSound;
    public AudioClip jumpSound;
    public AudioClip landSound;

    [Header("Ses Seviyeleri")]
    [Range(0f, 1f)] public float footstepVolume = 0.4f;
    [Range(0f, 1f)] public float combatVolume = 0.6f;
    [Range(0f, 1f)] public float jumpLandVolume = 0.5f;

    [Header("Saldırı ve Hasar")]
    public DamageDealer attackDamageDealer; // Inspector'dan Hitbox'ı sürükle
    public float attackActiveDuration = 0.25f; // Hasarın aktif kalacağı süre

    [Header("Hareket Ayarları")]
    public float walkSpeed = 8f;
    public float runSpeed = 12f;

    [Header("Saldırı Animasyonları")]
    public string attack1TriggerName = "Attack1";
    public string attack2TriggerName = "Attack2";
    public string attack3TriggerName = "Attack3";
    public float comboResetTime = 0.8f;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 12f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    [Header("Game Over Ayarları")]
    public GameObject gameOverPanel; // Hazırladığın DeathPanel

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 currentMoveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private bool runHeld;
    private int currentComboIndex;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (footstepSource != null)
        {
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (isDead) { StopFootstepSound(); return; }
        FlipCharacter();
        HandleFootstepSound();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded) PlayLandSound();

        HandleMovement();
        UpdateAnimationParameters();
    }

    // --- SALDIRI MANTIĞI (ANIMASYONSUZ HASAR) ---
    public void OnAttackPrimary(InputValue value)
    {
        if (!isDead && value.isPressed)
        {
            HandleComboAttack();
            PlaySwing();
            StartManualAttack();
        }
    }

    private void StartManualAttack()
    {
        if (attackDamageDealer != null)
        {
            attackDamageDealer.StartAttack(); // Hasarı aç
            CancelInvoke("EndManualAttack"); // Eğer hızlı basılırsa önceki kapatmayı iptal et
            Invoke("EndManualAttack", attackActiveDuration); // Belirlenen süre sonra kapat
        }
    }

    private void EndManualAttack()
    {
        if (attackDamageDealer != null)
        {
            attackDamageDealer.EndAttack(); // Hasarı kapat
        }
    }

    // --- SES MANTIĞI ---
    private void HandleFootstepSound()
    {
        bool isMoving = Mathf.Abs(currentMoveInput.x) > 0.01f;
        if (isMoving && isGrounded)
        {
            AudioClip selectedClip = runHeld ? runLoopSound : walkLoopSound;
            footstepSource.volume = footstepVolume;

            if (!footstepSource.isPlaying || footstepSource.clip != selectedClip)
            {
                footstepSource.clip = selectedClip;
                footstepSource.Play();
            }
        }
        else { StopFootstepSound(); }
    }

    private void StopFootstepSound()
    {
        if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
    }

    public void PlaySwing()
    {
        if (combatSource != null && swingSound != null)
        {
            if (combatSource.isPlaying && combatSource.time < 0.15f) return;
            combatSource.pitch = Random.Range(0.85f, 1.15f);
            combatSource.PlayOneShot(swingSound, combatVolume);
        }
    }

    private void PlayJumpSound() { if (combatSource && jumpSound) combatSource.PlayOneShot(jumpSound, jumpLandVolume); }
    private void PlayLandSound() { if (combatSource && landSound) combatSource.PlayOneShot(landSound, jumpLandVolume); }

    // --- HAREKET VE DİĞER ---
    public void OnMove(InputValue value) => currentMoveInput = value.Get<Vector2>();
    public void OnRun(InputValue value) => runHeld = value.isPressed;
    
    public void OnJump(InputValue value)
    {
        if (isGrounded && value.isPressed && !isDead)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
            StopFootstepSound();
            PlayJumpSound();
        }
    }

    private void HandleMovement()
    {
        float targetSpeed = runHeld ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(currentMoveInput.x * targetSpeed, rb.linearVelocity.y);
    }

    private void UpdateAnimationParameters()
    {
        bool isMoving = Mathf.Abs(currentMoveInput.x) > 0.01f;
        anim.SetBool("isRunning", isMoving && runHeld && isGrounded);
        anim.SetBool("isWalking", isMoving && !runHeld && isGrounded);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private void HandleComboAttack()
    {
        if (Time.time - lastAttackTime > comboResetTime) currentComboIndex = 0;
        anim.SetTrigger(GetComboTrigger(currentComboIndex));
        currentComboIndex = (currentComboIndex + 1) % 3;
        lastAttackTime = Time.time;
    }

    private string GetComboTrigger(int index) => index == 0 ? attack1TriggerName : index == 1 ? attack2TriggerName : attack3TriggerName;

    private void FlipCharacter()
    {
        if (currentMoveInput.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (currentMoveInput.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    // Bu fonksiyonu HealthSystem'daki OnDeath event'ine bağla
    public void OnPlayerDeath()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("Die");
        StopFootstepSound();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}