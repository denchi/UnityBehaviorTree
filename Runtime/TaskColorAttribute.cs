using System;
using UnityEngine;

namespace Behaviours.BT
{
    public class TaskColorAttribute: Attribute
    {
        public TaskColorAttribute(Color color)
        {
            Color = color;
        }
        
        public TaskColorAttribute(string hexString)
        {
            ColorUtility.TryParseHtmlString(hexString, out var color);
            Color = color;
        }

        public Color Color { get; private set; }
    }
}