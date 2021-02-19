﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _speed = 10.0f;
    Vector3 _destPos;

    void Start()
    {
        // 리스너 등록
        GameManager.Input.MouseAction -= OnMouseCliked;     // 두 번 등록 방지
        GameManager.Input.MouseAction += OnMouseCliked;
    }

    float wait_run_ratio;

    public enum PlayerState
    {
        Idle,
        Moving,
        Die,
    }

    void UpdateIdle()
    {
        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 0, 10.0f * Time.deltaTime);
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("wait_run_ratio", wait_run_ratio);
        animator.Play("WAIT_RUN");
    }

    void UpdateMoving()
    {
        Vector3 dir = _destPos - transform.position;
        if (dir.magnitude < 0.0001f)    // float 오차 범위로
        {
            // 도착했을 때
            _state = PlayerState.Idle;
        }
        else
        {
            float moveDist = Mathf.Clamp(_speed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);
        }

        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 1, 10.0f * Time.deltaTime);
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("wait_run_ratio", wait_run_ratio);
        animator.Play("WAIT_RUN");
    }

    PlayerState _state = PlayerState.Idle;

    void Update()
    {
        switch (_state)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Moving:
                UpdateMoving();
                break;
        }
    }

    void OnMouseCliked(Define.MouseEvent evt)
    {
        if (_state == PlayerState.Die) return;
        if (evt != Define.MouseEvent.Click) return;

        // ScreenToWorldPoint() + direction.normalized
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        // ray를 최대 100f 거리만큼 발사하여 충돌체 정보를 hit에 저장
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Raycast 충돌 발생하면 목적지로 지정
            _destPos = hit.point;
            Debug.Log($"destPost={_destPos}");
            _state = PlayerState.Moving;
        }

        // 메인 카메라 위치에서 dir 방향으로 100의 길이만큼 1초 동안 빨간색 광선 발사
        Debug.DrawRay(Camera.main.transform.position, ray.direction * 100f, Color.red, 1f);
    }
}
