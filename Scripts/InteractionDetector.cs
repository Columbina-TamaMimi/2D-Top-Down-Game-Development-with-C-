using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; // Closest Interactable
    public GameObject interactionIcon;

    void Start()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // ⭐ แก้ตรงนี้ - เช็คว่า interactableInRange ไม่เป็น null ก่อน
            if (interactableInRange != null)
            {
                interactableInRange.Interact();

                // เช็คอีกครั้งหลัง Interact (เผื่อสถานะเปลี่ยน)
                if (!interactableInRange.CanInteract())
                {
                    if (interactionIcon != null)
                    {
                        interactionIcon.SetActive(false);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }
}