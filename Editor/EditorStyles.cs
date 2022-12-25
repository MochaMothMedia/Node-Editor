using UnityEditor;
using UnityEngine;

namespace MochaMoth.NodeEditor.Editor
{
    public static class EditorStyles
    {
        static GUIStyle _nodeStyle;
        public static GUIStyle NodeStyle
		{
            get
			{
                if (_nodeStyle == null)
				{
                    _nodeStyle = new GUIStyle();
                    _nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                    _nodeStyle.border = new RectOffset(12, 12, 12, 12);
                    _nodeStyle.alignment = TextAnchor.UpperCenter;
                    _nodeStyle.normal.textColor = Color.white;
                    _nodeStyle.fontStyle = FontStyle.Bold;
                    _nodeStyle.contentOffset = new Vector2(0, 15);
				}

                return _nodeStyle;
			}
		}

        static GUIStyle _headerStyle;
        public static GUIStyle HeaderStyle
	    {
            get
		    {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle();
                    _headerStyle.fontSize = 24;
                    _headerStyle.normal.textColor = Color.white;
			    }

                return _headerStyle;
		    }
	    }

        static GUIStyle _pathStyle;
        public static GUIStyle PathStyle
		{
            get
			{
                if (_pathStyle == null)
				{
                    _pathStyle = new GUIStyle();
                    _pathStyle.normal.background = new Texture2D(1, 1);
                    _pathStyle.normal.background.wrapMode = TextureWrapMode.Repeat;
				}

                return _pathStyle;
			}
		}
    }
}
