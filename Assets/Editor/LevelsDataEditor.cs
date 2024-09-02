using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MatchUp.Data
{
    [CustomEditor(typeof(LevelsData))]
    [CanEditMultipleObjects]
    public class LevelsDataEditor : Editor
    {
        private static Dictionary<LevelsData.LevelDataItem, bool> foldouts = new Dictionary<LevelsData.LevelDataItem, bool>();
        private LevelsData _levelsData;
        
        private const int ButtonWidth = 20;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.BeginVertical();
            
            _levelsData = target as LevelsData;

            if (_levelsData.levels.Any())
            {
                for (var i = 0; i < _levelsData.levels.Count; i++)
                {
                    var l = _levelsData.levels[i];
                    Draw(l, i);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField("No Elements");
                
                if (Button("+", Color.green, true, GUILayout.Width(ButtonWidth)))
                {
                    Undo.RecordObject(_levelsData, "Added Level Data");
                    var newItem = new LevelsData.LevelDataItem();
                    _levelsData.levels.Add(newItem);
                }
                
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(LevelsData.LevelDataItem item, int i)
        {
            EditorGUILayout.BeginHorizontal();

            if (!foldouts.TryGetValue(item, out bool f))
            {
                f = true;
                foldouts.Add(item, true);
            }

            var c = GUI.color;

            int cardCount = item.boardWidth * item.boardHeight;
                
            if (_levelsData.levels.Count(l => l.id == item.id) > 1 || string.IsNullOrEmpty(item.name) ||
                cardCount < 4 || item.matchingCount < 2 ||
                cardCount <= item.matchingCount || cardCount % item.matchingCount != 0)
                GUI.color = Color.red;

            bool foldout = EditorGUILayout.Foldout(f, $"Element {i}");
            
            GUI.color = c;
            
            if (Button("^", Color.white, i > 0, GUILayout.Width(ButtonWidth)))
            {
                Undo.RecordObject(_levelsData, "Moved Level Data");
                _levelsData.levels.RemoveAt(i);
                _levelsData.levels.Insert(i - 1, item);
            }
            
            if (Button("v", Color.white, i < _levelsData.levels.Count - 1, GUILayout.Width(ButtonWidth)))
            {
                Undo.RecordObject(_levelsData, "Moved Level Data");
                _levelsData.levels.RemoveAt(i);
                _levelsData.levels.Insert(i + 1, item);
            }

            if (Button("-", Color.red, true, GUILayout.Width(ButtonWidth)))
            {
                Undo.RecordObject(_levelsData, "Removed Level Data");
                _levelsData.levels.RemoveAt(i);
            }

            if (Button("+", Color.green, true, GUILayout.Width(ButtonWidth)))
            {
                Undo.RecordObject(_levelsData, "Added Level Data");
                var newItem = new LevelsData.LevelDataItem();
                newItem.id = _levelsData.levels.Max(l => l.id) + 1;
                _levelsData.levels.Insert(i + 1, newItem);
            }

            EditorGUILayout.EndHorizontal();
            
            if (f != foldout)
                foldouts[item] = foldout;

            if (foldout)
            {
                EditorGUI.BeginChangeCheck();
                
                if (_levelsData.levels.Count(l => l.id == item.id) > 1)
                    GUI.color = Color.red;
                int id = Mathf.Max(0, EditorGUILayout.IntField("Id", item.id));
                GUI.color = c;
                
                if (string.IsNullOrEmpty(item.name))
                    GUI.color = Color.red;
                string name = EditorGUILayout.TextField("Name", item.name);
                GUI.color = c;
                
                int boardWidth = Mathf.Max(1, EditorGUILayout.IntField("Board Width", item.boardWidth));
                int boardHeight = Mathf.Max(1, EditorGUILayout.IntField("Board Height", item.boardHeight));
                int timeLimit = Mathf.Max(1, EditorGUILayout.IntField("Time Limit", item.timeLimit));
                int matchingCount = Mathf.Max(2, EditorGUILayout.IntField("Matching Count", item.matchingCount));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_levelsData, "Changed Level Data");
                    item.id = id;
                    item.name = name;
                    item.boardWidth = boardWidth;
                    item.boardHeight = boardHeight;
                    item.timeLimit = timeLimit;
                    item.matchingCount = matchingCount;
                }
            }
        }

        public static bool Button(string buttonName, Color buttonColor, bool buttonEnabled, params GUILayoutOption[] options)
        {
            GUI.backgroundColor = buttonColor;
            GUI.enabled = buttonEnabled;
            bool button = GUILayout.Button(buttonName, options);
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            return button;
        }
    }
}