using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    [RequireComponent(typeof(Text))]
    public class ActivityIndicatorMessage : MonoBehaviour
    {
        [SerializeField]
        private string activityMessage;
        public string ActivityMessage { get { return activityMessage; } set { activityMessage = value; } }
        public bool FadeEnabled { get; set; }
        private Text textComponent;
        private Color textColor;
        public void Awake()
        {
            textComponent = gameObject.GetComponent<Text>();
            textComponent.text = activityMessage;
            textColor = textComponent.color;
        }

        public void ToggleText(bool val, int timeout = 0)
        {
            if (timeout > 0)
            {
                StartCoroutine(ToggleTextTimeout(val, timeout));
            }
            else
            {
                textComponent.enabled = val;
            }
        }

        private IEnumerator ToggleTextTimeout(bool val, int duration)
        {
            yield return new WaitForSeconds(duration);
            textComponent.enabled = val;
        }

        private void UpdateText()
        {
            if (activityMessage != textComponent.text)
            {
                textComponent.text = activityMessage;
            }
        }

        private void Fade()
        {
            textComponent.color = Color.Lerp(textColor, Color.clear, Mathf.PingPong(Time.time, 1));
        }

        public void Update()
        {
            UpdateText();
            if (FadeEnabled)
            {
                Fade();
            }
        }
    }
}

