using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
    [SerializeField] GameObject groundCheck;

    InputAction moveAction;
    InputAction jumpAction;
    Rigidbody rb;
    OpenInventory inventory;
    Animator anim;

    

    private void LockCamera() => cam.enabled = false;
    private void UnlockCamera() => cam.enabled = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        inventory = GetComponent<OpenInventory>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inventory.isInventoryOpen)
        {
            UnlockCamera();
            Vector2 input = moveAction.ReadValue<Vector2>();
            float cameraRot = cam.transform.eulerAngles.y;
            Vector3 move = new Vector3(input.x, 0, input.y);

            Vector3 movement = Quaternion.Euler(0, cameraRot, 0) * move;
            transform.Translate(movement * Time.deltaTime * 5f, Space.World);
            transform.rotation = Quaternion.LookRotation(movement.normalized, Vector3.up);

            if (jumpAction.triggered && groundCheck.GetComponent<GroundCheck>().isGrounded)
            {
                rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            }
            anim.SetFloat("Speed", movement.magnitude);
        }
        else
        {
            LockCamera();
            anim.SetFloat("Speed", 0f);
        }
        transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
    }

}
