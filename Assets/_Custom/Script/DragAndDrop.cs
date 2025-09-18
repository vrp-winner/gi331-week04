using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    private InputAction touchPressAction;
    private InputAction touchPositionAction;
    
    [SerializeField] private float touchDragPhysicsSpeed = 10;
    [SerializeField] private float touchDragSpeed = 0.1f;

    private InputActionMap touchMap;
    private Camera mainCamera;
    private Vector3 velocity =  Vector3.zero;
    
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    private void Awake()
    {
        mainCamera = Camera.main;
        touchPressAction = playerInput.actions["TouchPress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
        touchMap = playerInput.actions.FindActionMap("Touch");
    }

    private void OnEnable()
    {
        touchMap.Enable();
        touchPressAction.performed += TouchPressed;
    }

    private void OnDisable()
    {
        touchPressAction.performed -= TouchPressed;
        touchMap.Disable();
    }

    private void TouchPressed(InputAction.CallbackContext context)
    {
        Vector3 position = touchPositionAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && (hit.collider.gameObject.CompareTag("Draggable")  || gameObject.layer == LayerMask.NameToLayer("Draggable")))
            {
                StartCoroutine(DragUpdate(hit.collider.gameObject));
            }
        }
    }

    private IEnumerator DragUpdate(GameObject clickedobject)
    {
        float initialDistance = Vector3.Distance(clickedobject.transform.position, mainCamera.transform.position);
        clickedobject.TryGetComponent<Rigidbody>(out var rb);
        while (touchPressAction.ReadValue<float>() != 0)
        {
            Vector3 position = touchPositionAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(position);
            if (rb != null)
            {
                Vector3 direction = ray.GetPoint(initialDistance) - clickedobject.transform.position;
                rb.linearVelocity = direction *  touchDragPhysicsSpeed;
                yield return waitForFixedUpdate;
            }
            else
            {
                clickedobject.transform.position = Vector3.SmoothDamp(clickedobject.transform.position,
                    ray.GetPoint(initialDistance), ref velocity, touchDragSpeed);
                yield return null;
            }
        }
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
