using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : InteractableObjects
{

  public override void onFocus()
  {
    Debug.Log("Looking at "+ gameObject.name);
  }

  public override void onInteract()
  {
    Debug.Log("Interactded with " + gameObject.name);
  }

  public override void onLoseFocus()
  {
    Debug.Log("Not Looking anymore"+ gameObject.name);
  }
}
