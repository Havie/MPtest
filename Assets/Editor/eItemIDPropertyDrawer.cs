using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(ObjectManager.eItemID))]
public class eItemIDPropertyDrawer : PropertyDrawer
{
    int lh1 = 18; // line height

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ImplementationONE(position, property, label);
        //ImplementationTWO(position, property, label);
    }

    private void ImplementationONE(Rect position, SerializedProperty property, GUIContent label)
    {
        GenericMenu gm = new GenericMenu();

        foreach (var item in System.Enum.GetValues(typeof(ObjectManager.eItemID)).Cast<ObjectManager.eItemID>())
        {
            gm.AddItem(new GUIContent($"{(int)item}: {item}"), false, () => { CallBack(item, property); });
        }

        var enumId = (ObjectManager.eItemID)property.enumValueIndex;
        string val = $"{(int)enumId}: {enumId}";

        //EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var amountRect = new Rect(position.x, position.y, position.width, position.height);

        var skin = Resources.Load<GUISkin>("Editor/Tex/button");
        var style = new GUIStyle(skin.button);
      
        if (GUI.Button(position, val, style))
        {
            gm.ShowAsContext();
        }


        //EditorGUI.EndProperty();

    }
    private void ImplementationTWO(Rect position, SerializedProperty property, GUIContent label)
    {
        GenericMenu gm = new GenericMenu();

        foreach (var item in System.Enum.GetValues(typeof(ObjectManager.eItemID)).Cast<ObjectManager.eItemID>())
        {
            gm.AddItem(new GUIContent($"{(int)item}: {item}"), false, () => { CallBack(item, property); });
        }

        var enumId = (ObjectManager.eItemID)property.enumValueIndex;
        string val = $"{(int)enumId}: {enumId}";

        //if(val.Length<90)
        //     val += NormalizeSpacing(val, 20);

        EditorGUILayout.BeginHorizontal();
      
        EditorGUI.indentLevel = 3; ///doesnt work with rects, only works with GUILAYOUT


        Rect textR = new Rect(position.x, position.y, 100f, lh1);
        Rect ctrlAdd = new Rect(position.x + label.text.Length + 270, position.y , 150f, position.height);


        EditorGUI.LabelField(textR, label.text, EditorStyles.label);
        //EditorGUILayout.LabelField( label.text, EditorStyles.label);
        //if (GUILayout.Button(new GUIContent(val), EditorStyles.popup))
        if (GUI.Button(ctrlAdd, val))
        {
            gm.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();

    }


    private string[] GetEnumList()
    {
        var arrList = System.Enum.GetValues(typeof(ObjectManager.eItemID));
        string[] list = new string[arrList.Length];
        int index = 0;
        foreach (var item in arrList)
        {
            list[index++] = $"{index}: {item}";
        }


        return list;
    }

    private void CallBack(ObjectManager.eItemID eItem, SerializedProperty property)
    {
        property.serializedObject.Update();
        property.enumValueIndex = (int)eItem;
        property.serializedObject.ApplyModifiedProperties();

    }


    
}
