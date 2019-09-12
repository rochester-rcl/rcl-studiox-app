using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace StudioX
{
    public class ExternalLinkButtonHandler : MonoBehaviour
    {
        public Button LinkButton { get; set; }

        public string url;

        public void Awake()
        {
            LinkButton = GetComponent<Button>() as Button;
            LinkButton.onClick.AddListener(ClickHandler);
            url = Sanitize(url);
        }

        public static string Sanitize(string val)
        {
            string pattern = "[^ -~]+";
            Regex regEx = new Regex(pattern);
            return regEx.Replace(val, "");
        }

        public void ClickHandler()
        {
            Application.OpenURL(url);
        }
    }
}

