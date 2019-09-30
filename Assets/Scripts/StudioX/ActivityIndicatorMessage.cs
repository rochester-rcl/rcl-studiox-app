using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
namespace StudioX
{
    /// <summary>
    /// Renders a simple fading message to indicate some type of activity status.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class ActivityIndicatorMessage : MonoBehaviour
    {

        [SerializeField]
        private string activityMessage;
        /// <summary>
        /// The message to be displayed.
        /// </summary>
        public string ActivityMessage { get { return activityMessage; } set { activityMessage = value; } }
        /// <summary>
        /// Sets animated fading to enabled or disabled state.
        /// </summary>
        public bool FadeEnabled { get; set; }
        /// <summary> The <see cref="UnityEngine.UI.Text"/> Component attached to the GameObject </summary>
        private Text textComponent;
        /// <summary> The color of the <see cref="UnityEngine.UI.Text"/> Component used in <see cref="Fade" />. </summary>
        private Color textColor;

        /// <summary> 
        /// Initializes internal properties. Note, any additional options can be set on the Text component attached to the GameObject.
        /// </summary>
        public void Awake()
        {
            textComponent = gameObject.GetComponent<Text>();
            textComponent.text = activityMessage;
            textColor = textComponent.color;
        }

        /// <summary>
        /// Toggles <see cref="ActivityMessage"/>  on or off. Optional parameter timeout will delay toggle for x seconds
        /// </summary>
        /// <param name="val">The state to set <see cref="ActivityMessage" /> to.</param>
        /// <param name="timeout">The duration of the timeout in seconds.</param>
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
        /// <summary>
        /// Toggles <see cref="ActivityMessage"/> on or off after timeout
        /// <summary>
        /// <param name="val">The state to set <see cref="ActivityMessage" /> to.</param>
        /// <param name="timeout">The duration of the timeout in seconds.</param>
        /// <returns>Task<void></returns> 
        public async Task ToggleTextAsync(bool val, int timeout)
        {
            await Task.Delay(timeout * 1000);
            textComponent.enabled = val;
        }
        /// <summary> Asynchronously toggles <see cref="ActivityMessage"/> after duration.</summary>
        /// <param name="val">The state to set <see cref="ActivityMessage" /> to.</param>
        /// <param name="duration">The duration of the timeout in seconds.</param>
        /// <returns>IEnumerator to be used with <see cref="UnityEngine.StartCoroutine"/></returns> 
        private IEnumerator ToggleTextTimeout(bool val, int duration)
        {
            yield return new WaitForSeconds(duration);
            textComponent.enabled = val;
        }
        /// <summary> 
        /// Updates <see cref="activityMessage"/> based on changes in attached <see cref="UnityEngine.UI.Text"/> Component. 
        /// </summary>
        private void UpdateText()
        {
            if (activityMessage != textComponent.text)
            {
                textComponent.text = activityMessage;
            }
        }
        /// <summary> 
        /// Fades attached <see cref="UnityEngine.UI.Text"/> Component's color value if <see cref="FadeEnabled"/> is true. 
        /// </summary>
        private void Fade()
        {
            textComponent.color = Color.Lerp(textColor, Color.clear, Mathf.PingPong(Time.time, 1));
        }

        /// <summary> 
        /// Checks for changes on the Text component and executes fading animation if <see cref="FadeEnabled" /> is set to true. 
        /// </summary>
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

