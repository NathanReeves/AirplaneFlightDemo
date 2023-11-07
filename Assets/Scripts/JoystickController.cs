using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform JoystickBackground;
    public RectTransform JoystickHandle;
    public LineRenderer JoystickLine;
    public RectTransform JoystickLineCanvas;

    private Vector2 joystickCenter;
    private float screenSizeMultiplier;
    private Vector2 screenCenter;

    private const float MAX_JOYSTICK_DISTANCE = 200f;

    public Vector2 InputDirection { get; private set; }

    void Start()
    {
        JoystickLine.endColor = Color.cyan;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        JoystickBackground.gameObject.SetActive(true);
        JoystickHandle.gameObject.SetActive(true);
        JoystickBackground.position = eventData.position;
        JoystickHandle.position = eventData.position;
        joystickCenter = (Vector2)Input.mousePosition;

        JoystickLine.positionCount = 2;
        screenSizeMultiplier = Screen.width / JoystickLineCanvas.sizeDelta.x;
        screenCenter = new Vector2(Screen.width, Screen.height) / 2;
        JoystickLine.SetPosition(0, ((Vector2)Input.mousePosition - screenCenter) / screenSizeMultiplier);
        JoystickLine.SetPosition(1, ((Vector2)Input.mousePosition - screenCenter) / screenSizeMultiplier);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - joystickCenter;
        float distance = Vector2.ClampMagnitude(direction, MAX_JOYSTICK_DISTANCE).magnitude;
        JoystickHandle.position = joystickCenter + direction.normalized * distance;

        CalculateInputDirection();
        updateJoystickLine(JoystickHandle.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        JoystickBackground.gameObject.SetActive(false);
        JoystickHandle.gameObject.SetActive(false);
        InputDirection = Vector2.zero;
        JoystickLine.positionCount = 0;
    }

    private void CalculateInputDirection()
    {
        Vector2 direction = JoystickHandle.position - (Vector3)joystickCenter;
        float distance = Mathf.Min(direction.magnitude, MAX_JOYSTICK_DISTANCE);
        Vector2 normalizedDirection = direction.normalized;

        InputDirection = new Vector2(normalizedDirection.x * (distance / MAX_JOYSTICK_DISTANCE),
                                     normalizedDirection.y * (distance / MAX_JOYSTICK_DISTANCE));
    }

    void updateJoystickLine(Vector2 currentPos)
    {
        JoystickLine.positionCount = 2;
        screenSizeMultiplier = Screen.width / JoystickLineCanvas.sizeDelta.x;
        screenCenter = new Vector2(Screen.width, Screen.height) / 2;
        JoystickLine.SetPosition(0, (joystickCenter - screenCenter) / screenSizeMultiplier);
        JoystickLine.SetPosition(1, (currentPos - screenCenter) / screenSizeMultiplier);
    }
}