using System;
using UnityEngine;

namespace FedoraDev.NodeEditor.Abstract
{
	public delegate void MoveCallback(Vector2 delta);

    public class NodeDragHelper
    {
		public static bool ProcessDrag(Event currentEvent, MoveCallback move, Action place, Rect correctedPosition, ref bool isDragged)
		{
			switch (currentEvent.type)
			{
				case EventType.MouseDown:
					if (currentEvent.button == 0)
					{
						if (correctedPosition.Contains(currentEvent.mousePosition))
							isDragged = true;
						GUI.changed = true;
					}
					break;

				case EventType.MouseUp:
					isDragged = false;
					place.Invoke();
					break;

				case EventType.MouseDrag:
					if (currentEvent.button == 0 && isDragged)
					{
						move(currentEvent.delta);
						currentEvent.Use();
						return true;
					}
					break;
			}

			return false;
		}
	}
}
