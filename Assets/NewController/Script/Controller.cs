using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public LayerMask groundLayer;
    public float speed = 1f;
    public float groundRayLenght = 0.11f;
    public Transform rayGround, rayWall;
    public Transform spriteRotTransform, spriteFlipTransform;
    public bool debugMode;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2;
    public float gravity = 9f;
    public float maxFallVelocity = 30;
    public float jumpForce = 10;

    Rigidbody2D rb;
    RaycastHit2D[] hitGround, hitNextGround,hitWallRight, hitWallLeft;

    int hitCount;
   public  int jumpBuffer;

    public float rotationSpeed = 15;
    float inputX;
    float lastRayDistance;


    bool landing = false;
    bool faceRight = true;
    bool isGrounded;

    Vector3 calculatedVelocity;
    Vector3 nextPosition;
    Vector3 directionAngle = Vector3.zero;
    Vector2 originWallRay;
    Vector2 wallRayDirection;
    Vector3 prevRayGroundPos;
    public Vector3 velocity;


    void Awake()
    {
        hitGround = new RaycastHit2D[1];
        hitNextGround = new RaycastHit2D[1];
        hitWallRight = new RaycastHit2D[1];
        hitWallLeft = new RaycastHit2D[1];
        rb = GetComponent<Rigidbody2D>();
        prevRayGroundPos = rayGround.position;
    }

    bool IsGrounded()
    {
        hitCount = 0;
        Array.Clear(hitGround, 0, hitGround.Length);
        hitCount = Physics2D.RaycastNonAlloc(rayGround.position, Vector2.down,  hitGround, groundRayLenght, groundLayer);
        
        if(!landing && hitCount>0)
        {
            CheckLanding();
        }

        isGrounded = hitCount == 1;
        return isGrounded;

    }

    bool IsOnWall()
    {
        Array.Clear(hitWallRight, 0, hitWallRight.Length);
       

        return Physics2D.RaycastNonAlloc(rayWall.position, rayWall.right, hitWallRight, 0.08f, groundLayer) > 0;
    }

    void RotateSpriteOnGround()
    {
        Quaternion rot = Quaternion.FromToRotation(transform.up, hitGround[0].normal);
        spriteRotTransform.rotation = Quaternion.Slerp(spriteRotTransform.rotation, rot, Time.deltaTime * rotationSpeed);
    }   

    void CheckLanding()
    {
        transform.position = hitGround[0].point;
        Quaternion rot = Quaternion.FromToRotation(transform.up, hitGround[0].normal);
        spriteRotTransform.rotation = rot;
        calculatedVelocity.y = 0;
        velocity.y = 0;
        landing = true;
    }

    void MostrarRays()
    {
        if (debugMode)
        {
            Vector3 hitPos = hitGround[0].point;
            Vector3 hitNormal = hitGround[0].normal;
            Vector3 direction = Vector3.Cross(Vector3.forward, hitNormal);
            Debug.DrawRay(hitPos, direction * 1, Color.red);
            Debug.DrawRay(rayGround.position, -transform.up * 2, Color.blue);
            Debug.DrawRay(rayWall.position, rayWall.right * 0.08f, Color.blue);
        }
    }

    void FixSlope()
    {     
        if(inputX == 0)
        {
            if (hitGround[0].normal != Vector2.up)
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
        if (inputX != 0 && !IsOnWall())
        {
            Array.Clear(hitNextGround, 0, hitNextGround.Length);
            calculatedVelocity = Vector3.Cross(Vector3.forward, hitGround[0].normal) * speed * -inputX;
            //rb.velocity = calculatedVelocity;
            Vector3 predictNextPosition = transform.position + calculatedVelocity * Time.deltaTime;
            Vector3 rayOrigin = predictNextPosition + Vector3.up;
            Debug.DrawRay(rayOrigin, Vector3.down * 1.5f, Color.magenta);
            int hitCount = Physics2D.RaycastNonAlloc(rayOrigin, Vector3.down, hitGround, 1.5f, groundLayer);

            if (hitCount > 0)
            {
                nextPosition = hitGround[0].point;
                velocity = nextPosition - transform.position;
                lastRayDistance = hitGround[0].distance;
            }
            else
            {
                nextPosition = rayOrigin + Vector3.down * lastRayDistance;
            }
        }
        else
            velocity = Vector3.zero;
        

 
    }


    void PrevenirAtravezarPiso()
    {
        if(velocity.y < 0)
        {
            Vector3 direccionRayo = (rayGround.position - prevRayGroundPos).normalized;
            float distanciaRayo = direccionRayo.magnitude;
            RaycastHit2D hitPiso = Physics2D.Raycast(prevRayGroundPos, direccionRayo, distanciaRayo, groundLayer);
            Debug.DrawRay(prevRayGroundPos, direccionRayo * distanciaRayo, Color.magenta);
            if(hitPiso.collider != null)
            {
                transform.position = hitPiso.point;
                velocity.y = 0;
            }
        }

        prevRayGroundPos = rayGround.position;

        
    }

    void AirVelocity()
    {

        //if (velocity.y < 0)
        //{
        //    float lenght = rayGround.position.y - prevRayGroundPos.y;
        //    RaycastHit2D checkGround = Physics2D.Raycast(prevRayGroundPos, Vector2.down, lenght, groundLayer);
        //    Debug.DrawRay(prevRayGroundPos, Vector2.down * lenght, Color.cyan);
        //    if(checkGround.collider!= null)
        //    {
        //        transform.position = checkGround.point;
        //    }
        //}

        //spriteRotTransform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * 20);
        //Vector2 direccion = Vector2.right * inputX * speed * Time.deltaTime;
        //velocity.x = inputX * speed * Time.deltaTime;
        //velocity.y -= gravity * Time.deltaTime;


        //if (velocity.y < maxFallVelocity)
        //{
        //    velocity = Vector2.ClampMagnitude(velocity, maxFallVelocity);
        //}


        //prevRayGroundPos = rayGround.position;


        velocity.y += gravity * Time.deltaTime;

        if (velocity.y < maxFallVelocity)
        {
            velocity = Vector2.ClampMagnitude(velocity, maxFallVelocity);
        }



    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            calculatedVelocity.y = jumpForce;           
        }
    }



    void CheckInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
       
    }
    
    void CheckFlip()
    {
        if((inputX < 0 && faceRight)||(inputX > 0 && !faceRight))
        {           
            faceRight = !faceRight;
            directionAngle.y = faceRight ? 0 : 180;
            spriteFlipTransform.localEulerAngles = directionAngle; 

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
            //Jump();
            MostrarRays();
        }
        else
        {
            landing = false;
            AirVelocity();
            PrevenirAtravezarPiso();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        transform.position += velocity;
        CheckFlip();
    }
}
