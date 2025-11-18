using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class InputHandler : MonoBehaviour
    {
        struct Vector2Action
        {
            public string x;
            public string y;

            public Vector2Action(string x, string y)
            {
                this.x = x;
                this.y = y;
            }
        }

        // Avoid duplicate string generation
        Dictionary<string, Vector2Action> vector2Actions = new Dictionary<string, Vector2Action>();

        public bool GetBool(string actionName)
        {
            bool output = false;
            try
            {
                output = Input.GetButton(actionName);
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"{actionName} action not found!");
            }

            return output;
        }

        public float GetFloat(string actionName)
        {
            float output = default(float);
            try
            {
                output = Input.GetAxis(actionName);
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"{actionName} action not found!");
            }
            return output;
        }

        public Vector2 GetVector2(string actionName)
        {
            Vector2Action vector2Action;

            bool found = vector2Actions.TryGetValue(actionName, out vector2Action);

            if (!found)
            {
                vector2Action = new Vector2Action(
                    string.Concat(actionName, " X"),
                    string.Concat(actionName, " Y")
                );

                vector2Actions.Add(actionName, vector2Action);
            }

            Vector2 output = default(Vector2);

            try
            {
                output = new Vector2(Input.GetAxis(vector2Action.x), Input.GetAxis(vector2Action.y));
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"{vector2Action.x} and/or {vector2Action.y} actions not found!");
            }

            return output;
        }
    }
}
