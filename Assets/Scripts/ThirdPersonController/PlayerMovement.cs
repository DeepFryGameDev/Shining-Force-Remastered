using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class PlayerMovement : MonoBehaviour
    {
        public CharacterController controller;

        public Transform cam;

        public bool combat = false;
        public bool canMove = true;

        public float walkSpeed = 1.4f;
        public float sprintSpeed = 2.8f;

        public float turnSmoothTime = 0.1f;
        float turnSmoothVelocity;

        bool sprint = false;

        private void Start()
        {
            if (transform.CompareTag("Player"))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (transform.CompareTag("PlayerUnit"))
            {
                Cursor.lockState = CursorLockMode.None;
            }

        }

        void Update()
        {
            if (canMove)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    sprint = true;
                }
                else
                {
                    sprint = false;
                }

                if (direction.magnitude >= 0.1f)
                {
                    ProcessMove(horizontal, vertical, direction);
                }

                if (!sprint && (horizontal != 0f || vertical != 0f))
                {
                    GetComponent<Animator>().SetBool("isRunning", false);
                    GetComponent<Animator>().SetBool("isWalking", true);
                }
                else if (sprint && (horizontal != 0f || vertical != 0f))
                {
                    GetComponent<Animator>().SetBool("isWalking", false);
                    GetComponent<Animator>().SetBool("isRunning", true);
                }
                else if (horizontal == 0f && vertical == 0f)
                {
                    GetComponent<Animator>().SetBool("isWalking", false);
                    GetComponent<Animator>().SetBool("isRunning", false);
                }
            }
            
        }

        void ProcessMove(float horizontal, float vertical, Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            if (!sprint)
            {
                controller.Move(moveDir.normalized * walkSpeed * Time.deltaTime);
            }
            else
            {
                controller.Move(moveDir.normalized * sprintSpeed * Time.deltaTime);
            }
        }

        public void ToggleCanMove()
        {
            canMove = !canMove;
        }
    }
}
