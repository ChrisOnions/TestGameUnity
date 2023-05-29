using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  InteractableObjects : MonoBehaviour
{

  public virtual void Awake()
    {
        gameObject.layer = 9;
    }
    public abstract void onInteract();
    public abstract void onFocus();
    public abstract void onLoseFocus();
}
