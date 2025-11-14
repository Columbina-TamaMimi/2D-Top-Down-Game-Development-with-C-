using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;

    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;
    private Animator animator;
    private float LastInputX;
    private float LastInputY;

    void Start()
    {
        animator = GetComponent<Animator>();

        // ตรวจสอบว่ามี waypointParent หรือไม่
        if (waypointParent == null)
        {
            Debug.LogError("WaypointMover: waypointParent ไม่ได้ถูกกำหนด! กรุณาลาก GameObject ที่มี Waypoints มาใส่");
            enabled = false; // ปิดการทำงานของ Script
            return;
        }

        // ตรวจสอบว่ามี Waypoint ลูกอยู่หรือไม่
        if (waypointParent.childCount == 0)
        {
            Debug.LogError("WaypointMover: " + waypointParent.name + " ไม่มี Waypoint ลูกเลย! กรุณาเพิ่ม Waypoint");
            enabled = false;
            return;
        }

        // โหลด Waypoints
        waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }

        Debug.Log("WaypointMover: โหลด Waypoints สำเร็จ " + waypoints.Length + " จุด");
    }

    void Update()
    {
        // เช็คว่า waypoints ถูกโหลดแล้ว
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (PauseController.IsGamePaused || isWaiting)
        {
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetFloat("InputX", LastInputX);
                animator.SetFloat("InputY", LastInputY);
            }
            return;
        }

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        // ป้องกัน Index เกินขอบเขต
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }

        Transform target = waypoints[currentWaypointIndex];

        // ตรวจสอบว่า target ไม่เป็น null
        if (target == null)
        {
            Debug.LogWarning("WaypointMover: Waypoint ที่ index " + currentWaypointIndex + " เป็น null!");
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;

        // เก็บทิศทางล่าสุด
        if (direction.magnitude > 0)
        {
            LastInputX = direction.x;
            LastInputY = direction.y;
        }

        // เคลื่อนที่
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // อัปเดต Animator
        if (animator != null)
        {
            animator.SetFloat("InputX", direction.x);
            animator.SetFloat("InputY", direction.y);
            animator.SetBool("isWalking", direction.magnitude > 0f);
        }

        // ถึงจุดหมายแล้ว
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("InputX", LastInputX);
            animator.SetFloat("InputY", LastInputY);
        }

        yield return new WaitForSeconds(waitTime);

        // เปลี่ยนไปจุดถัดไป
        if (loopWaypoints)
        {
            // วนลูป
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            // ไม่วนลูป - หยุดที่จุดสุดท้าย
            currentWaypointIndex = Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);
        }

        isWaiting = false;
    }

    // แสดง Gizmos ใน Editor
    void OnDrawGizmos()
    {
        if (waypointParent == null)
            return;

        // วาดเส้นเชื่อม Waypoints
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            Transform current = waypointParent.GetChild(i);
            if (current == null) continue;

            // วงกลมที่จุด Waypoint
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(current.position, 0.3f);

            // เส้นเชื่อมไปจุดถัดไป
            if (i < waypointParent.childCount - 1)
            {
                Transform next = waypointParent.GetChild(i + 1);
                if (next != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
            else if (loopWaypoints && waypointParent.childCount > 0)
            {
                // วนกลับไปจุดแรก
                Transform first = waypointParent.GetChild(0);
                if (first != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(current.position, first.position);
                }
            }
        }
    }
}