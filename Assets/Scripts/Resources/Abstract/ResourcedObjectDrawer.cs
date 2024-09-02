#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Helpers
{
    public abstract class ResourcedObjectDrawer<TObject, TValue> : PropertyDrawer
        where TObject : InstanceObjectBase<TValue>
        where TValue : UnityEngine.Object
    {
        private const float Tab = 2f;

        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent content)
        {
            float h = EditorGUI.GetPropertyHeight(property, content, true);
            if (property.isExpanded)
                h += Tab + GUI.skin.label.CalcSize(content).y;

            return h;
        }

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent content)
        {
            float h = GUI.skin.label.CalcSize(content).y;
            var p = new Rect(position.x, position.y, position.width, h);

            EditorGUI.BeginProperty(position, content, property);

            Type t = property.serializedObject.targetObject.GetType();
            FieldInfo fi = null;
            PropertyInfo pi = null;
            object o = property.serializedObject.targetObject;
            var pp = property.propertyPath.Replace("Array", "_items")
                .Replace("data[", string.Empty)
                .Replace("]", string.Empty)
                .Split('.');

            foreach (var prop in pp)
            {
                t = o.GetType();

                if (fi != null && fi.FieldType.IsArray || pi != null && pi.PropertyType.IsArray)
                {
                    o = ((System.Collections.IList) o)[Convert.ToInt32(prop)];

                    fi = null;
                    pi = null;

                    if (o == null)
                        break;

                    continue;
                }

                fi = t.GetField(prop,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                    BindingFlags.Instance);
                if (fi == null)
                    pi = t.GetProperty(prop,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                        BindingFlags.Instance);

                if (fi != null)
                    o = fi.GetValue(o);
                else if (pi != null)
                    o = pi.GetValue(o);
                else
                {
                    o = null;
                    break;
                }
            }

            var target = o as TObject;

            var c = GUI.color;
            if (string.IsNullOrEmpty(target.GUID) || string.IsNullOrEmpty(target.Path))
                GUI.color = Color.red;
            
            EditorGUI.PropertyField(p, property, content, false);

            GUI.color = c;

            if (property.isExpanded)
            {
                bool e = GUI.enabled;
                GUI.enabled = false;

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = indent + 1;

                p.y = p.y + h + Tab;
                EditorGUI.PropertyField(p, property.FindPropertyRelative("assetGUID"), new GUIContent("GUID"), true);

                p.y = p.y + h + Tab;
                EditorGUI.PropertyField(p, property.FindPropertyRelative("assetPath"), new GUIContent("Path"), true);

                GUI.enabled = e;

                p.y = p.y + h + Tab;
                p.width = p.width - 20;
                
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.ObjectField(p, "Asset", target.Value, typeof(TValue), false) as TValue;
                if (EditorGUI.EndChangeCheck())
                    target.Value = v;

                GUI.enabled = e;

                EditorGUI.indentLevel = indent;
            }

            EditorGUI.EndProperty();
        }
    }
}

#endif