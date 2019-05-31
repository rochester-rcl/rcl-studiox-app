using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

// Button that raises onDown event when OnPointerDown is called.
public class AeButton : Button
{
    // Event delegate triggered on mouse or touch down.
    [SerializeField]
    ButtonDownEvent _onDown = new ButtonDownEvent();

    protected AeButton() { }

	void Start()
	{
        _onDown.AddListener(Ping);
	}

	public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        Debug.Log("before if");

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Debug.Log("after if");

        _onDown.Invoke();
    }

    public ButtonDownEvent onDown
    {
        get { return _onDown; }
        set { _onDown = value; }
    }

    void Ping()
    {
        Debug.Log("Ping");
    }

    [Serializable]
    public class ButtonDownEvent : UnityEvent { }
}