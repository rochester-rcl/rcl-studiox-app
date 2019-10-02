using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace StudioX
{
    /// <summary>Component that opens an external link on a button click.</summary>
    [RequireComponent(typeof(Button))]
    public class ExternalLinkButtonHandler : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="UnityEngine.UI.Button" /> Component attached to <see cref="gameObject"/>.
        /// </summary>
        public Button LinkButton { get; set; }
        /// <summary>
        /// The URL of the external resource to open when <see cref="LinkButton"/> is clicked.
        /// </summary> 
        public string url;
        /// <summary>
        /// Adds <see cref="ClickHandler"/> to <see cref="LinkButton"/>, sanitizes <see cref="url"/>.
        /// </summary>
        public void Awake()
        {
            LinkButton = gameObject.GetComponent<Button>();
            LinkButton.onClick.AddListener(ClickHandler);
            url = Sanitize(url);
        }
        /// <summary>Sanitzes the input URL.</summary>
        /// <param name="val">The input URL to sanitize.</param>
        /// <returns>The sanitized URL.</returns> 
        public static string Sanitize(string val)
        {
            string pattern = "[^ -~]+";
            Regex regEx = new Regex(pattern);
            return regEx.Replace(val, "");
        }
        /// <summary>
        /// Opens <see cref="url"/> using <see cref="UnityEngine.Application.OpenURL"/>
        /// when <see cref="LinkButton"/> is clicked.
        /// </summary>
        public void ClickHandler()
        {
            Application.OpenURL(url);
        }
    }
}

