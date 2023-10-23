using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DottedLine {
    public class DottedLine : MonoBehaviour {
        //Static Property with backing field
        private static DottedLine _instance;

        // Inspector fields
        public GameObject dotPrefab;

        [Range(0.01f, 3f)] public float Size;

        [Range(0.1f, 10f)] public float Offset;

        [Range(1, 1000)] public int MaxPoints = 1000;

        private readonly List<GameObject> _dots = new();
        private ObjectPool<GameObject> _dotPool;

        public static DottedLine Instance => _instance ? _instance : _instance = FindObjectOfType<DottedLine>();

        public void Awake() {
            _dotPool = new ObjectPool<GameObject>(
                () => Instantiate(dotPrefab, transform), go => { go.SetActive(true); },
                go => { go.SetActive(false); }, null, true, 100, MaxPoints);
        }

#if UNITY_EDITOR
        public void OnValidate() {
            var sizeVector = Vector3.one * Size;
            dotPrefab.transform.localScale = sizeVector;
            _dots.ForEach(d => d.transform.localScale = sizeVector);
            _dotPool?.Clear(); // FIXME: Don't clear on size change
        }
#endif

        public void DrawDottedLine(Vector2 start, Vector2 end) {
            var point = start;
            var direction = (end - start).normalized;

            using(ListPool<Vector3>.Get(out var positions)) {
                var i = 0;

                while((end - start).magnitude > (point - start).magnitude) {
                    if(i >= MaxPoints) {
                        break;
                    }

                    positions.Add(point);

                    point += direction * Offset;
                    i++;
                }

                Render(positions);
            }
        }

        private void Render(List<Vector3> positions) {
            for(var i = 0; i < positions.Count; i++) {
                GameObject g;
                if(_dots.Count == i) {
                    g = _dotPool.Get();
                    _dots.Add(g);
                }
                else {
                    g = _dots[i];
                }

                g.transform.localPosition = positions[i];
            }

            if(_dots.Count <= positions.Count) return;

            // Release unused
            for(var i = positions.Count; i < _dots.Count; i++) {
                _dotPool.Release(_dots[i]);
            }

            _dots.RemoveRange(positions.Count, _dots.Count - positions.Count);
        }
    }
}