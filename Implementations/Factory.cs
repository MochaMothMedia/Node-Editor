using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.NodeEditor.Implementations
{
	[CreateAssetMenu(fileName = "Node Editor Factory", menuName = "Node Editor/Factory")]
	public class Factory : SerializedScriptableObject, IFactory
	{
		[SerializeField] IConnection _connectionFab;

		public IConnection ProduceConnection() => _connectionFab.Produce(this);
	}
}
