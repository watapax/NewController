using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public LayerMask groundLayer;
    public float speed = 1f;
    public float groundRayLenght = 0.11f;
    public Transform rayOrigin;
    public Transform spriteTransform;
    public bool debugMode;

    Rigidbody2D rb;
    RaycastHit2D[] hit, nextHit;

    int hitCount;

    float rotationSpeed = 10;
    float inputX;
    float lastRayDistance;

    bool landing = false;
    bool faceRight = true;

    Vector3 calculatedVelocity;
    Vector3 nextPosition;

    void Awake()
    {
        hit = new RaycastHit2D[1];
        nextHit = new RaycastHit2D[1];
        rb = GetComponent<Rigidbody2D>();
    }

    bool IsGrounded()
    {
        hitCount = 0;
        Array.Clear(hit, 0, hit.Length);
        hitCount = Physics2D.RaycastNonAlloc(rayOrigin.position, Vector2.down,  hit, groundRayLenght, groundLayer);
        
        if(!landing && hitCount>0)
        {
            CheckLanding();
        }

        return hitCount == 1;

    }

    void RotateSpriteOnGround()
    {
        Quaternion rot = Quaternion.FromToRotation(transform.up, hit[0].normal);
        float zAngle = rot.eulerAngles.z;
        Vector3 nextAngle = Vector3.forward * zAngle;
       // spriteTransform.localEulerAngles = Vector3.Lerp(spriteTransform.localEulerAngles, nextAngle, Time.deltaTime * rotationSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotationSpeed);

    }   

    void CheckLanding()
    {
        nextPosition = hit[0].point;
        Quaternion rot = Quaternion.FromToRotation(transform.up, hit[0].normal);
        spriteTransform.rotation = rot;
        landing = true;
    }

    void MostrarRays()
    {
        if (debugMode)
        {
            Vector3 hitPos = hit[0].point;
            Vector3 hitNormal = hit[0].normal;
            Vector3 direction = Vector3.Cross(Vector3.forward, hitNormal);
            Debug.DrawRay(hitPos, direction * 1, Color.red);
            Debug.DrawRay(rayOrigin.position, -transform.up * 2, Color.blue);
        }
    }

    void FixSlope()
    {     
        if(inputX == 0)
        {
            if (hit[0].normal != Vector2.up)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

    }

    void GroundVelocity()
    {
        if (inputX != 0)
        {
            transform.position = nextPosition;
            Array.Clear(nextHit, 0, nextHit.Length);
            calculatedVelocity = Vector3.Cross(Vector3.forward, hit[0].normal) * speed * -inputX;
            //rb.velocity = calculatedVelocity;
            Vector3 predictNextPosition = transform.position + calculatedVelocity * Time.deltaTime;
            Vector3 rayOrigin = predictNextPosition + Vector3.up;
            Debug.DrawRay(rayOrigin, Vector3.down * 1.5f, Color.magenta);
            int hitCount = Physics2D.RaycastNonAlloc(rayOrigin, Vector3.down, hit, 1.5f, groundLayer);

            if (hitCount > 0)
            {
                nextPosition = hit[0].point;
                lastRayDistance = hit[0].distance;
            }
            else
            {
                nextPosition = rayOrigin + Vector3.down * lastRayDistance;
            }
        }
        else
            nextPosition = transform.position;
    }

    void AirVelocity()
    {
        spriteTransform.rotation = Quaternion.Slerp(spriteTransform.rotation, Quaternion.identity, Time.deltaTime * 20);
        Vector2 direccion = Vector2.right * inputX * speed * Time.deltaTime;
        transform.Translate(direccion);
    }

    void CheckInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
    }
    
    void CheckFlip()
    {
        if((inputX < 0 && faceRight)||(inputX>0 && !faceRight))
        {
            Vector3 localAngle = transform.localEulerAngles;
            localAngle.y = localAngle.y == 180 ? 0 : 180;
            transform.localEulerAngles = localAngle;
            faceRight = !faceRight;
        }
    }
    
    void Update()
    {
        CheckInput();
    }

    void FixedUpdate()
    {
        if (IsGrounded())
        {
            GroundVelocity();
            RotateSpriteOnGround();
            FixSlope();
            MostrarRays();
        }
        else
        {
            landing = false;
            AirVelocity();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
