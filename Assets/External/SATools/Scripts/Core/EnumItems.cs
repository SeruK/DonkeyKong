using UnityEngine;
using UE = UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Serialization;

public class EnumItems<E, V> where E: struct, IConvertible
{
	[SerializeField, FormerlySerializedAs("items")]
	V[] _items = new V[EnumHelper<E>.values.Length];

	protected virtual IEqualityComparer<E> comparer
	{
		get { return null; }
	}

	public V[] items
	{
		get
		{
			int newSize = EnumHelper<E>.values.Length;
			if(_items.Length != newSize)
			{
				Array.Resize(ref _items, newSize);
			}
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	public V this[E key]
	{
		get
		{
			return items[EnumHelper<E>.IndexOf(key, comparer)];
		}
	}

	public Dictionary<E, V> CreateDictionary()
	{
		// Assumes a correct (0..last) enum
		var vals = EnumHelper<E>.values;
		Dictionary<E, V> dict = new Dictionary<E, V>();
		var cachedItems = this.items;
		for(int i = 0; i < vals.Length; ++i)
		{
			dict.Add(vals[i], cachedItems[i]);
		}
		return dict;
	}

	public Dictionary<int, V> CreateIntDictionary()
	{
		// Assumes a correct (0..last) enum
		var vals = EnumHelper<E>.values;
		Dictionary<int, V> dict = new Dictionary<int, V>();
		var cachedItems = this.items;
		for(int i = 0; i < vals.Length; ++i)
		{
			dict.Add(i, cachedItems[i]);
		}
		return dict;
	}

	#if UNITY_EDITOR
	public class Drawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var enumType = typeof(E);
			if(!enumType.IsEnum)
			{
				return EditorGUIUtility.singleLineHeight * (property.isExpanded ? 2f : 1f);
			}

			float h = EditorGUIUtility.singleLineHeight;

			if(property.isExpanded)
			{
				var items = property.FindPropertyRelative("_items");

				for(int i = 0; i < items.arraySize; ++i)
				{
					var item = items.GetArrayElementAtIndex(i);
					h += EditorGUI.GetPropertyHeight(item);
				}
			}

			return h;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = EditorHelper.GetTooltipFromAttribute(fieldInfo, defaultValue: null);

			Rect foldPosition = position;
			foldPosition.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = EditorGUI.Foldout(foldPosition, property.isExpanded, label);
			if(!property.isExpanded)
			{
				return;
			}

			var enumType = typeof(E);
			if(!enumType.IsEnum)
			{
				position.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.LabelField(position, new GUIContent("Non Enum Type used"));
				return;
			}
			var names = Enum.GetNames(enumType);

			var items = property.FindPropertyRelative("_items");

			while(items.arraySize < names.Length)
			{
				items.InsertArrayElementAtIndex(Mathf.Max(0, items.arraySize - 1));
				var newEle = items.GetArrayElementAtIndex(items.arraySize - 1);
				EditorHelper.ResetPropertyValue(newEle);
			}


			position.y += EditorGUIUtility.singleLineHeight;
			position.yMax = position.y + EditorGUIUtility.singleLineHeight;

			EditorGUI.indentLevel += 1;

			for(int i = 0; i < names.Length; ++i)
			{
				var item = items.GetArrayElementAtIndex(i);
				var name = names[i];
				EditorGUI.PropertyField(position, item, new GUIContent(name), includeChildren: true);

				float h = EditorGUI.GetPropertyHeight(item);
				position.y += h;
				position.yMax = position.y + h;
			}

			EditorGUI.indentLevel -= 1;
		}
	}
	#endif
}
