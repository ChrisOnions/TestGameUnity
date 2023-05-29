using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI healthText = default;
  [SerializeField] private TextMeshProUGUI StaminaTesxt = default;

  private void OnEnable()
  {
    PlayerController.onDamage += updateHealth;
    PlayerController.onHealDamage += updateHealth;
    PlayerController.onStaminaChange += updateStamina;

  }

  private void OnDisable()
  {
    PlayerController.onDamage -= updateHealth;
    PlayerController.onHealDamage -= updateHealth;
    PlayerController.onStaminaChange -= updateStamina;
  }


  void Start()
  {
    updateHealth(100);
    updateStamina(100);
  }

  private void updateHealth(float currentHealth)
  {
    healthText.text = currentHealth.ToString("00");
  }

  private void updateStamina(float currentStamina)
  {
    StaminaTesxt.text = currentStamina.ToString("00");
  }

}
