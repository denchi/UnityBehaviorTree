using UnityEngine;

namespace Behaviours.BT
{
    public class BTVarAttribute : PropertyAttribute
    {
        public ValueType type;

        public BTVarAttribute(ValueType type)
        {
            this.type = type;
        }
    }
}