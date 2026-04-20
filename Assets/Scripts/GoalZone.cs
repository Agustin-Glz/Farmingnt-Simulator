using UnityEngine;

public class GoalZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TractorController.Instance.SendFruitPickupSignal();
            GameManager.Instance.Ganar();
        }
    }
}