using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Xml.Linq;


public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Animator animator;
    private bool isTakingDamage = false;
    public bool IsTakingDamage => isTakingDamage;

    public Slider healthBarSlider;
    public Image fillImage;

    private GameOptions gameManager;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        gameManager = FindObjectOfType<GameOptions>();

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = maxHealth;
        }

        animator.SetBool("dieded", false);
    }

    public void TakeDamage(int amount)
    {
        if (isTakingDamage) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining: {currentHealth}");

        if (animator != null)
        {
            animator.SetTrigger("TakeDamageTrigger");
            isTakingDamage = true;
            StartCoroutine(ResetDamageState());
            SoundManager.PlaySound(SoundType.TAKEDAMAGE);
        }

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;

            float percent = (float)currentHealth / maxHealth;

            if (fillImage != null)
            {
                fillImage.color = percent <= 0.25f ? Color.red :
                                  percent <= 0.5f ? Color.yellow :
                                  Color.green;
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ResetDamageState()
    {
        yield return new WaitForSeconds(0.3f);
        isTakingDamage = false;
    }

   void Die()
    {
        Debug.Log($"{gameObject.name} is defeated!");
        
        animator.SetBool("dieded", true);

        if (gameManager != null)
        {
            string winner = gameObject.CompareTag("Player")
                ? GameObject.FindGameObjectWithTag("Opponent")?.name ?? "Opponent"
                : GameObject.FindGameObjectWithTag("Player")?.name ?? "Player";

            gameManager.GameOver(winner);
        }
    }

    public void SetHealthFull(float health)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = health;
        }
    }

    public void SetHealth(float health)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = health;
        }
    }
}
