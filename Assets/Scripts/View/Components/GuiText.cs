using TMPro;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class GuiText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void SetText(string text)
        {
            if (_text != null) { _text.text = text; }
        }
    }
}
