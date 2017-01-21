using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AI
{
    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : Editor
    {
        public Inventory Target { get; private set; }

        public override void OnInspectorGUI()
        {
            Target = (Inventory)target;
            base.OnInspectorGUI();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine(Target.MoneyString);
            builder.AppendLine(string.Format("Liquid: {0} L", Target.Liquid.ToString("F2")));
            builder.AppendLine(string.Format("Food: {0} KG", Target.Food.ToString("F2")));
            builder.AppendLine(string.Format("Boarding Pass: {0}", Target.BoardingPass));
            EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info, true);
        }
    }
}
