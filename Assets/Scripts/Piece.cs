using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Camera cam;
    private void OnMouseDrag()
    {
        transform.position = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
    }

    
}
