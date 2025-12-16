using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToPointMovement : MonoBehaviour
{
    [Header("Path (closed loop)")]
    [Tooltip("Ordered waypoints defining the closed loop. At least 3 required.")]
    public Transform[] waypoints;

    [Header("Sampling / smoothing")]
    [Tooltip("How many samples between each pair of original waypoints (higher -> smoother, heavier).")]
    [Range(4, 200)]
    public int samplesPerSegment = 40;

    [Header("Motion")]
    [Tooltip("Speed in world units per second along the path.")]
    public float speed = 3f;

    [Tooltip("Height above ground to keep (when ground found).")]
    public float heightOffset = 0.0f;

    [Tooltip("Rotation smoothing (higher -> slower turning).")]
    public float rotationSmoothTime = 0.12f;

    [Header("Ground / physics")]
    [Tooltip("How far down to look for ground from raycastStartHeight")]
    public float maxGroundDistance = 100f;
    [Tooltip("Start Y coordinate for downward raycasts (e.g. 50).")]
    public float raycastStartHeight = 50f;
    public LayerMask groundLayer;

    [Header("Behavior")]
    [Tooltip("If true, movement direction follows the path direction; if false, move backwards.")]
    public bool forward = true;

    // internals
    Rigidbody rb;
    List<Vector3> samples;            // sampled positions along smoothed closed path (x,z used)
    List<float> cumulativeLengths;    // cumulative length along samples
    float totalLength;
    float traveled = 0f;              // distance traveled along path
    Vector3 velocity = Vector3.zero;  // helper for smoothing rotation (unused by SmoothDampQuaternion)
    float currentVelYaw = 0f;
    public float moveDelay;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody required");
        if (waypoints == null || waypoints.Length < 3)
        {
            Debug.LogError("Need at least 3 waypoints for a closed loop.");
            enabled = false;
            return;
        }

        
    }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(moveDelay);
        BuildSamples();
        // position object at nearest point on path initially
        traveled = 0;//FindClosestDistanceOnPath(transform.position);
        // optionally snap vertically to ground at that sample
        SnapVerticalToSample(traveled);
        transform.position = waypoints[0].position ;

    }
    void FixedUpdate()
    {
        if (samples == null || samples.Count == 0) return;
        float dt = Time.fixedDeltaTime;
        // advance traveled distance
        float moveDist = speed * dt * (forward ? 1f : -1f);
        traveled += moveDist;

        // wrap traveled into [0, totalLength)
        if (totalLength > 0f)
        {
            traveled %= totalLength;
            if (traveled < 0f) traveled += totalLength;
        }

        // get target sample position by arclength (interpolated between samples)
        Vector3 target = SamplePositionAtDistance(traveled);

        // Ground check: raycast down from high above target.xz
        Vector3 rayOrigin = new Vector3(target.x, target.y + raycastStartHeight, target.z);
        RaycastHit hit;
        bool groundFound = Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastStartHeight + maxGroundDistance, groundLayer, QueryTriggerInteraction.Ignore);

        // compute desired horizontal velocity to reach target position (x,z)
        Vector3 currentPos = rb.position;
        Vector3 desiredPosXZ = new Vector3(target.x, 0f, target.z);
        Vector3 currentXZ = new Vector3(currentPos.x, 0f, currentPos.z);
        Vector3 deltaXZ = desiredPosXZ - currentXZ;

        // desired horizontal velocity to reach sample in one fixed update (clamped smoothly)
        Vector3 desiredHorizontalVel;
        if (dt > 0f)
        {
            Vector3 needed = deltaXZ / dt;
            // clamp magnitude to avoid instant teleport for large sampling gaps: limit to speed
            float maxHorizSpeed = Mathf.Max(0.0001f, speed * 1.5f); // allow slight overshoot
            if (needed.magnitude > maxHorizSpeed) needed = needed.normalized * maxHorizSpeed;
            desiredHorizontalVel = new Vector3(needed.x, 0f, needed.z);
        }
        else desiredHorizontalVel = Vector3.zero;

        // Vertical handling:
        float currentVy = rb.linearVelocity.y;
        float targetY = rb.position.y;

        if (groundFound)
        {
            // snap/smooth y to hit point + offset
            float desiredY = hit.point.y + heightOffset;
            // compute vertical velocity to move towards desiredY reasonably smoothly
            // Using simple PD / lerp to avoid strong impulses
            float yDiff = desiredY - currentPos.y;
            // desired vertical speed to reach in ~0.1..0.2s (tunable)
            float timeToReach = 0.12f;
            float desiredVy = Mathf.Clamp(yDiff / Mathf.Max(0.02f, timeToReach), -20f, 20f);
            currentVy = desiredVy;
        }
        else
        {
            // no ground: keep vertical velocity as physics dictates (let gravity act)
            // do not overwrite currentVy (read from rb.velocity already)
            currentVy = rb.linearVelocity.y;
        }

        // set final velocity (horizontal from desired, vertical from above)
        Vector3 finalVel = new Vector3(desiredHorizontalVel.x, currentVy, desiredHorizontalVel.z);
        rb.linearVelocity = finalVel;

        // rotation: look along tangential direction of path at current traveled position
        Vector3 tangent = SampleTangentAtDistance(traveled);
        if (!forward) tangent = -tangent;
        tangent.y = 0f;
        if (tangent.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(tangent.normalized, Vector3.up);
            // smooth rotate
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, 1f - Mathf.Exp(-rotationSmoothTime * 60f * Time.fixedDeltaTime));
            rb.MoveRotation(newRot);
        }
    }

    #region Sampling / Path building (Catmull-Rom closed)
    void BuildSamples()
    {
        // Prepare control points (positions from transforms)
        Vector3[] pts = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++) pts[i] = waypoints[i].position;

        // catmull-rom closed: for i in 0..n-1, segment between p[i] and p[i+1]
        samples = new List<Vector3>();
        cumulativeLengths = new List<float>();
        float cum = 0f;
        cumulativeLengths.Add(0f);

        int n = pts.Length;
        for (int i = 0; i < n; i++)
        {
            Vector3 p0 = pts[(i - 1 + n) % n];
            Vector3 p1 = pts[i];
            Vector3 p2 = pts[(i + 1) % n];
            Vector3 p3 = pts[(i + 2) % n];

            for (int s = 0; s < samplesPerSegment; s++)
            {
                float t = (float)s / samplesPerSegment;
                Vector3 pos = CatmullRom(p0, p1, p2, p3, t);
                if (samples.Count > 0)
                {
                    cum += Vector3.Distance(samples[samples.Count - 1], pos);
                    cumulativeLengths.Add(cum);
                }
                samples.Add(pos);
            }
        }

        // close loop by connecting last sample to first
        if (samples.Count > 1)
        {
            cum += Vector3.Distance(samples[samples.Count - 1], samples[0]);
            // add final cumulative length value (equal to total length)
            cumulativeLengths.Add(cum);
        }

        totalLength = (cumulativeLengths.Count > 0) ? cumulativeLengths[cumulativeLengths.Count - 1] : 0f;

        // If mismatched lengths (shouldn't happen), recalc to be safe
        if (samples.Count + 1 != cumulativeLengths.Count)
        {
            // simple rebuild: recompute cumulative lengths
            cumulativeLengths = new List<float>(samples.Count + 1);
            cumulativeLengths.Add(0f);
            float running = 0f;
            for (int i = 1; i < samples.Count; i++)
            {
                running += Vector3.Distance(samples[i - 1], samples[i]);
                cumulativeLengths.Add(running);
            }
            running += Vector3.Distance(samples[samples.Count - 1], samples[0]);
            cumulativeLengths.Add(running);
            totalLength = running;
        }
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom spline (centripetal/standard) - using standard basis (can be tuned)
        // returns point between p1 and p2 for t in [0,1]
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    // returns interpolated position along closed path by distance d (0..totalLength)
    Vector3 SamplePositionAtDistance(float d)
    {
        if (totalLength <= 0f || samples.Count == 0) return transform.position;

        // wrap distance
        d %= totalLength;
        if (d < 0f) d += totalLength;

        // binary search cumulativeLengths
        int left = 0, right = cumulativeLengths.Count - 1;
        while (left <= right)
        {
            int mid = (left + right) / 2;
            if (cumulativeLengths[mid] <= d) left = mid + 1;
            else right = mid - 1;
        }
        int idx = Mathf.Clamp(right, 0, samples.Count - 1);
        int next = (idx + 1) % samples.Count;

        float segmentStart = cumulativeLengths[idx];
        float segmentLength = cumulativeLengths[idx + 1] - segmentStart;
        float t = (segmentLength > 0f) ? ((d - segmentStart) / segmentLength) : 0f;

        Vector3 a = samples[idx];
        Vector3 b = samples[next];
        // interpolate linearly between samples (samples are dense)
        return Vector3.Lerp(a, b, t);
    }

    // approximate tangent at distance d by sampling small epsilon ahead
    Vector3 SampleTangentAtDistance(float d)
    {
        float eps = 0.01f; // small distance
        Vector3 p1 = SamplePositionAtDistance(d);
        Vector3 p2 = SamplePositionAtDistance(d + eps);
        Vector3 tan = (p2 - p1);
        if (tan.sqrMagnitude < 1e-6f)
        {
            // fallback to next sample
            int i = Mathf.RoundToInt((d / Mathf.Max(1e-6f, totalLength)) * samples.Count) % samples.Count;
            Vector3 a = samples[i];
            Vector3 b = samples[(i + 1) % samples.Count];
            tan = b - a;
        }
        return tan.normalized;
    }

    // find nearest distance along path to a world position (approx by checking samples)
    float FindClosestDistanceOnPath(Vector3 worldPos)
    {
        if (samples == null || samples.Count == 0) return 0f;
        float bestDist = float.MaxValue;
        int bestIdx = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            float d = Vector3.SqrMagnitude(new Vector3(samples[i].x, 0f, samples[i].z) - new Vector3(worldPos.x, 0f, worldPos.z));
            if (d < bestDist)
            {
                bestDist = d;
                bestIdx = i;
            }
        }
        // return cumulative length at that sample (clamped)
        return Mathf.Clamp(cumulativeLengths[bestIdx], 0f, totalLength);
    }
    #endregion

    // Try to snap vertical component to ground at given arclength
    void SnapVerticalToSample(float distanceAlong)
    {
        Vector3 target = SamplePositionAtDistance(distanceAlong);
        Vector3 rayOrigin = new Vector3(target.x, target.y + raycastStartHeight, target.z);
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastStartHeight + maxGroundDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            Vector3 pos = rb.position;
            pos.x = target.x;
            pos.z = target.z;
            pos.y = hit.point.y + heightOffset;
            rb.position = pos;
            rb.linearVelocity = Vector3.zero;
        }
    }

    // Utility to visualize path in editor
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        // draw straight lines between waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            var a = waypoints[i].position;
            var b = waypoints[(i + 1) % waypoints.Length].position;
            Gizmos.DrawLine(a, b);
        }

        if (samples != null && samples.Count > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < samples.Count; i++)
            {
                Gizmos.DrawSphere(samples[i], 0.05f);
                Gizmos.DrawLine(samples[i], samples[(i + 1) % samples.Count]);
            }
        }
    }
}
