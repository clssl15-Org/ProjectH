using System;
using UI;
using UnityEngine;

namespace Dialogue
{
    public class BubbleContainer : BubbleDialogueUI.IContainer
    {
        // Front
        public string Text { get; set; }
        public Func<Vector2> GetPosition { get; set; }
        public Vector2 Offset { get; set; } = new Vector2(0, 100);

        // Internal
        private RectTransform _canvasTransform;
        private Camera _camera;
        private Vector2 _lastWorldPosition;


        // Content
        public BubbleContainer(RectTransform canvasTransform)
        {
            _canvasTransform = canvasTransform;
            _camera = Camera.main;
        }
        public BubbleContainer With(string text, Transform worldTransform, Vector2? offset = null)
        {
            With(
                text: text,
                getWorldPosition: () =>
                {
                    if (!worldTransform) return _lastWorldPosition;

                    _lastWorldPosition = worldTransform.position;
                    return _lastWorldPosition;
                },
                offset: offset);

            _lastWorldPosition = worldTransform.position;
            return this;
        }
        public BubbleContainer With(string text, Func<Vector2> getWorldPosition, Vector2? offset = null)
        {
            Text = text;
            GetPosition = () => WorldToCanvas(getWorldPosition()) + Offset;
            if (offset.HasValue) Offset = offset.Value;

            return this;
        }

        private Vector2 WorldToCanvas(Vector2 worldPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasTransform,
                _camera.WorldToScreenPoint(worldPosition),
                null,
                out var localPos);

            return localPos;
        }
    }
}
