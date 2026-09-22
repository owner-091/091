using NUnit.Framework;
using UnityEngine;

namespace TrainGame.Tests
{
    public sealed class RouteDataTests
    {
        [Test]
        public void NewRoute_HasZeroStops()
        {
            RouteData route = ScriptableObject.CreateInstance<RouteData>();
            Assert.AreEqual(0, route.StopCount);
            Object.DestroyImmediate(route);
        }
    }
}
