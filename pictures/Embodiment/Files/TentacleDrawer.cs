using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
//[ExecuteAlways]
public class TentacleDrawer : MonoBehaviour
{
    [SerializeField] LineRenderer line;
    [SerializeField] int Smoothing = 20;
    [SerializeField] Transform body;

    public bool isGrounded;
    public float radius;
    public float minDist = 0.5f;
    public float maxDist = 0.5f;
    public Rigidbody2D rb;
    TentacleManager tentacleManager;
    public bool isMoving = false;
    public Vector2 velocity;

    //default radius = 0.45f
    //default range = 0.34f

    Vector3 defaultTentaculePos;
    Vector2 endPosOfAnimation;
    float tentaculeSide = 0;
    Vector3[] Points;

    Vector3 currentPos;
    Vector3 oldPos;

    Coroutine animCoroutine;

    /* Important Fact:
     * You can stop Coroutines with StopCoroutine()
     * StartCoroutine returns the Coroutine it started
     */

    /* TODO:
     * Add Bulletproofing to walking animation, so tentacules don't attempt to draw in terrain
     * Fix when turning after moving left or right, tentacules overlap
     */

    private void Awake()
    {
        tentacleManager = TentacleManager.instance;

        tentaculeSide = transform.localPosition.x;
        defaultTentaculePos = transform.localPosition;

        currentPos = oldPos = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        Points = new Vector3[Smoothing+1];

        line.positionCount = Smoothing+1;

        DrawTentacle();
    }

    private void FixedUpdate()
    {
        currentPos = transform.position;
        velocity = (oldPos - currentPos) / Time.deltaTime;

        float xdiff = transform.position.x - body.position.x;

        if (Mathf.Abs(xdiff) <= Mathf.Abs(minDist) || Mathf.Abs(xdiff) >= Mathf.Abs(maxDist) && !isMoving)
        {
            AnimateTransform(GetValidPosition());
        }

        endPosOfAnimation = GetValidPosition();
    }

    private void Update()
    {
        isGrounded = GetGroundState();
    }

    private void LateUpdate()
    {
        oldPos = currentPos;
        DrawTentacle();
    }

    void DrawTentacle()
    {
        Vector3 pos3 = transform.position + transform.up/2;

        Vector3 vectorFromBody = pos3 - body.position;
        Vector3 pos2 = vectorFromBody / 2 + body.position;
        
        for (int i = 0; i <= Smoothing; i++)
        {
            Points[i] = CubicLerp(body.position, pos2, pos3, transform.position, ((float)i / Smoothing));
        }

        line.SetPositions(Points);
    }

    Vector2 GetValidPosition()
    {
        float influencePos = body.position.x;
        Vector3 leftBound = new Vector3(influencePos + minDist, tentacleManager.ray.point.y);
        Vector3 rightBound = new Vector3(influencePos + maxDist, tentacleManager.ray.point.y);
        Vector3 median = new Vector3((leftBound.x + rightBound.x) / 2, tentacleManager.ray.point.y);

        Vector3 endPoint;
        //Player is moving left
        if (tentacleManager.rb.velocity.x < 0)
        {
            if (tentaculeSide < 0)
            {
                endPoint = rightBound;
            }
            else
            {
                endPoint = leftBound;
            }
        }
        //Player is moving right
        else if(tentacleManager.rb.velocity.x > 0)
        {
            if (tentaculeSide < 0)
            {
                endPoint = leftBound;
            }
            else
            {
                endPoint = rightBound;
            }
        }
        else
        {
            endPoint = median;
        }

        int layer = LayerMask.GetMask("Jumpables");
        RaycastHit2D hit = Physics2D.Raycast(endPoint + Vector3.up, Vector3.down, Mathf.Infinity, layer);

        if(hit.point.y < tentacleManager.bounds.min.y)
        {
            //hit = Physics2D.Raycast(tentacleManager.BottomRightCorner(), Vector3.left, tentacleManager.bounds.size.x/2, layer);
            hit.point = body.position;
        }

        return hit.point;
    }

    bool GetGroundState()
    {
        int layer = LayerMask.GetMask("Jumpables");
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, 0.04f), Vector3.down, 0.08f, layer);

        return hit.collider != null;
    }

    public void AnimateTransform( Vector2 newPos,  int inputFrames = 16, float angle = 0f)
    {
        //If isMoving is false, don't try to animate the tentacles again
        //endPosOfAnimation = newPos;
        if(!isMoving)
            animCoroutine = StartCoroutine(AnimateTransformE(angle, inputFrames));
    }

    public void StopAnimation()
    {
        StopCoroutine(animCoroutine);
        isMoving = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    IEnumerator AnimateTransformE(float angle = 0f, int inputFrames = 16)
    {
        isMoving = true;
        Quaternion newAngle = Quaternion.Euler(0, 0, angle);
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        int frames = 16;
        int elapsedFrames = 0;

        // min frames 4 : min dist 0.25f
        // Min dist is the minimum dist can go before frames defaults to 4
        /*if (dist <= 0.35f)
        {
            frames = 4;
        }
        // max frames 14 : max dist 1.25f
        // Max dist is the maximum dist can go before frames defaults to 14
        else if(dist >= 1.25f)
        {
            frames = 14;
        }
        //add a fraction of the frames needed to get to 14 frames, based on the how much dist is
        // 1.25 - dist = part of the dist between 0.35 and 1.25
        // part / 0.9
        //If dist is between max and min, increase frames depending on dist
        else
        {
            float part = 1.25f - dist;
            float fraction = part / 0.9f;
            frames = 6 + (int)(8 * fraction);
        }*/
        

        while (elapsedFrames != frames)
        {
            float dist = Vector3.Distance(startPos, endPosOfAnimation);

            float temp;
            Vector2 pos2;
            Vector2 pos3;

            if (tentacleManager.rb.velocity.x < 0)
            {
                temp = -1;
                pos2 = new Vector3((startPos.x - dist / 2) + 0.1f, startPos.y + dist / 2, 0f);
                pos3 = new Vector3((startPos.x - dist / 2) - 0.1f, startPos.y + dist / 2, 0f);
            }
            else if(tentacleManager.rb.velocity.x > 0)
            {
                temp = 1;
                pos2 = new Vector3((startPos.x + dist / 2) - 0.1f, startPos.y + dist / 2, 0f);
                pos3 = new Vector3((startPos.x + dist / 2) + 0.1f, startPos.y + dist / 2, 0f);
            }
            else
            {
                temp = 0;
                pos2 = new Vector3((startPos.x + dist / 2) - 0.1f, startPos.y + dist / 2, 0f);
                pos3 = new Vector3((startPos.x + dist / 2) + 0.1f, startPos.y + dist / 2, 0f);
            }
            //pos2 = body.position;
            //pos3 = endPosOfAnimation + Vector2.up / 2;

            float ratio = (float)elapsedFrames / frames;

            //Updates tentacle position
            transform.position = CubicLerp(startPos, pos2, pos3, endPosOfAnimation, ratio);
            //Updates tentacle rotation
            transform.rotation = Quaternion.Euler(0, 0, temp * tentacleManager.animTentCurve.Evaluate(ratio));
            //transform.rotation = Quaternion.Lerp(startRot, newAngle, ratio);

            elapsedFrames = (elapsedFrames + 1) % (frames + 1);

            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, 0);
        isMoving = false;
    }

    public Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(ab, bc, t);
    }

    public Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 ab_bc = QuadraticLerp(a, b, c, t);
        Vector3 bc_cd = QuadraticLerp(b, c, d, t);

        return Vector3.Lerp(ab_bc, bc_cd, t);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 pos3 = transform.position + (transform.up / 2);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos3, 0.2f);
        Gizmos.DrawLine(transform.position, pos3);

        Vector3 vectorFromBody = pos3 - body.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(vectorFromBody/2 + body.position, 0.2f);
        Gizmos.DrawLine(body.position, vectorFromBody + body.position);

        float dist = -0.9f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(body.position.x, body.position.y + dist, 0f), 
            new Vector3(body.position.x + minDist, body.position.y + dist, 0f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(body.position.x + minDist, body.position.y + dist, 0f),
            new Vector3(body.position.x + maxDist, body.position.y + dist, 0f));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector3)endPosOfAnimation, 0.2f);
    }

}
