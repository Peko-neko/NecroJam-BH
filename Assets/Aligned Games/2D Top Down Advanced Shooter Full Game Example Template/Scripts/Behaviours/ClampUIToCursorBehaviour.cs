using UnityEngine;

namespace AlignedGames
{
    public class ClampUIToCursorBehaviour : MonoBehaviour
    {
        public RectTransform uiElement; // The UI element (e.g., crosshair) that follows the cursor
        public Vector2 offset = Vector2.zero; // Optional position offset from the cursor
        public Canvas canvas; // The canvas the UI element is on

        void Update()
        {
            // Try to find the GameManager in the scene (you can optimize by caching this)
            var gameManager = FindObjectOfType<GameManager>();

            // If any menu is open, hide the UI element
            if (gameManager != null && gameManager.IsAnyMenuOpen)
            {
                if (uiElement.gameObject.activeSelf)
                    uiElement.gameObject.SetActive(false);
                return;
            }
            else
            {
                // If no menu is open, make sure the UI element is visible
                if (!uiElement.gameObject.activeSelf)
                    uiElement.gameObject.SetActive(true);
            }

            // Determine which camera to use depending on canvas render mode
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            // Convert screen point to local point within the canvas
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                Input.mousePosition,
                cam,
                out pos
            );

            // Get the canvas rect and calculate half the UI element's size (taking scale into account)
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 halfSize = uiElement.rect.size * uiElement.lossyScale * 0.5f;

            // Calculate clamped min/max limits based on canvas bounds and UI size
            Vector2 min = canvasRect.rect.min + halfSize;
            Vector2 max = canvasRect.rect.max - halfSize;

            // Apply offset and clamp the position so the UI stays within the screen
            pos += offset;
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);

            // Set the new position of the UI element
            uiElement.anchoredPosition = pos;
        }
    }
}
