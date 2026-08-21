using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hospital404
{
    public static class HospitalUIFactory
    {
        public static readonly Color Ink = new Color(0.12f, 0.16f, 0.22f, 1f);
        public static readonly Color Paper = new Color(0.96f, 0.93f, 0.84f, 0.98f);
        public static readonly Color Accent = new Color(0.15f, 0.52f, 0.63f, 1f);
        public static readonly Color AccentDark = new Color(0.06f, 0.24f, 0.31f, 1f);
        public static readonly Color Warning = new Color(0.78f, 0.30f, 0.23f, 1f);
        public static readonly Color Overlay = new Color(0.03f, 0.06f, 0.10f, 0.82f);

        public static Canvas CreateCanvas(string name, int sortingOrder = 100)
        {
            EnsureEventSystem();
            GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return panel;
        }

        public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            button.colors = colors;

            Text text = CreateText(buttonObject.transform, "Label", label, 28, Color.white, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
            return button;
        }

        public static InputField CreateInputField(Transform parent, string name, string placeholderText, bool multiLine)
        {
            GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            Image image = inputObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.96f);

            InputField input = inputObject.GetComponent<InputField>();
            input.lineType = multiLine ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            input.transition = Selectable.Transition.ColorTint;

            Text placeholder = CreateText(inputObject.transform, "Placeholder", placeholderText, 24, new Color(0.35f, 0.40f, 0.45f, 0.78f), TextAnchor.UpperLeft);
            Stretch(placeholder.rectTransform, new Vector2(18f, 12f), new Vector2(-18f, -12f));
            placeholder.fontStyle = FontStyle.Italic;

            Text text = CreateText(inputObject.transform, "Text", string.Empty, 24, Ink, TextAnchor.UpperLeft);
            Stretch(text.rectTransform, new Vector2(18f, 12f), new Vector2(-18f, -12f));
            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        public static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        public static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static void DestroyIfPresent(string objectName)
        {
            GameObject found = GameObject.Find(objectName);
            if (found != null)
            {
                Object.Destroy(found);
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.name = "EventSystem";
        }
    }
}
