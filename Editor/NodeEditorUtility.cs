using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FedoraDev.NodeEditor.Editor
{
    public static class NodeEditorUtility
    {
		public static void CacheResults<T>()
		{
			_ = GetAllThatImplement<T>(true);
			_ = GetAllScriptableObjectsThatImplement<T>(true);
		}

		static Dictionary<Type, Type[]> _implements = new Dictionary<Type, Type[]>();
		public static Type[] GetAllThatImplement<T>() => GetAllThatImplement<T>(false);
		static Type[] GetAllThatImplement<T>(bool forceReload)
		{
			Type interfaceType = typeof(T);
			if (_implements.ContainsKey(interfaceType) && !forceReload)
				return _implements[interfaceType];

			Type[] classes = AppDomain.CurrentDomain.GetAssemblies()
						  .SelectMany(assembly => assembly.GetTypes())
						  .Where(cls => interfaceType.IsAssignableFrom(cls) && cls.IsClass && !cls.IsAbstract)
						  .ToArray();

			if (_implements.ContainsKey(interfaceType))
				_implements[interfaceType] = classes;
			else
				_implements.Add(interfaceType, classes);
			return classes;
		}

		static Dictionary<Type, ScriptableObject[]> _scriptables = new Dictionary<Type, ScriptableObject[]>();
		public static ScriptableObject[] GetAllScriptableObjectsThatImplement<T>() => GetAllScriptableObjectsThatImplement<T>(false);
		static ScriptableObject[] GetAllScriptableObjectsThatImplement<T>(bool forceReload)
		{
			Type interfaceType = typeof(T);

			if (_scriptables.ContainsKey(interfaceType) && !forceReload)
				return _scriptables[interfaceType];

			Type[] types = GetAllThatImplement<T>(forceReload);
			List<ScriptableObject> scriptables = new List<ScriptableObject>();
			for (int i = 0; i < types.Length; i++)
			{
				string[] guids = AssetDatabase.FindAssets($"t:{types[i].Name}");

				for (int j = 0; j < guids.Length; j++)
				{
					string path = AssetDatabase.GUIDToAssetPath(guids[j]);
					scriptables.Add(AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as ScriptableObject);
				}
			}

			if (_scriptables.ContainsKey(interfaceType))
				_scriptables[interfaceType] = scriptables.ToArray();
			else
				_scriptables.Add(interfaceType, scriptables.ToArray());
			return scriptables.ToArray();
		}
	}
}
