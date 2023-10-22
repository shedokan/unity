using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class BallPool : LinkedPool<BallController> {
    private const int MaxSize = 1000;

    // TODO: Try to avoid collection check
    public BallPool(GameObject prefab, GameObject parent)
        : base(
            PooledItemCreator(prefab, parent), OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject, true, MaxSize) {
    }

    private static Func<BallController> PooledItemCreator(GameObject prefab, GameObject parent) {
        return () => {
            var newObject = Object.Instantiate(prefab, parent.transform);
            return newObject.GetComponent<BallController>();
        };
    }

    // Called when an item is returned to the pool using Release
    private static void OnReturnedToPool(BallController ball) {
        ball.gameObject.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    private static void OnTakeFromPool(BallController ball) {
        ball.gameObject.SetActive(true);
        ball.Recycle();
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    private static void OnDestroyPoolObject(BallController ball) {
        Object.Destroy(ball.gameObject);
    }

    /// <summary>See <see cref="LinkedPool[T].Release" /></summary>
    public void Release(GameObject gameObject) {
        Release(gameObject.GetComponent<BallController>());
    }
}