using UnityEngine;

public class Door : Interactable
{
    private bool isClosed = true;

    protected override void Interact()
    {
        isClosed = !isClosed;
        gameObject.SetActive(isClosed);
        Debug.Log(isClosed ? "Door opened!" : "Door closed!");
    }
}