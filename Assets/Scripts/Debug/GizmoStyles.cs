using UnityEngine;

namespace Match3Game
{
    public static class GizmoStyles {
        private static GUIStyle _label;
        public static GUIStyle Label {
            get {
                if (_label == null) {
                    _label = new GUIStyle(UnityEditor.EditorStyles.whiteLargeLabel)
                    {
                        alignment = TextAnchor.UpperRight,
                        fontStyle = FontStyle.Bold,
                        contentOffset = Vector2.zero
                    };
                }
                return _label;
            }
        }
    }
}
