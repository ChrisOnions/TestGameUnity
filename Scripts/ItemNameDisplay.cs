using UnityEngine;
using UnityEngine.UI;

public class ItemNameDisplay : MonoBehaviour
{
    public Text itemNameText; // Reference to the UI Text object
    private void Start()
    {
        // Find the Text component in the children of this GameObject
        itemNameText = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        // Access the PlayerController script and get the currentInteractable
        Interactable currentInteractable = FindObjectOfType<PlayerController>()?.currentInteractable;

        if (currentInteractable != null)
        {
            // Update the text of the UI Text object with the currentInteractable's itemName
            itemNameText.text = currentInteractable.name;
        }
        else
        {
            // If no currentInteractable, clear the text
            itemNameText.text = "";
        }
    }
}
