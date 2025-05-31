using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    public Camera cam;
    private void FixedUpdate()
    {
        //RotatePiece();
    }

    private void RotatePiece()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0f, 0f, rotateSpeed);
        } 
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0f, 0f, -1*rotateSpeed);
        }
    }
    
    private void OnMouseDrag()
    {
        transform.position = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        
        RotatePiece();
    }

    
}
