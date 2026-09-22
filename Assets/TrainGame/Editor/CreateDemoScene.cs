using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TrainGame.Editor
{
    public static class CreateDemoScene
    {
        private const string DataFolder = "Assets/TrainGame/Data";
        private const string SceneFolder = "Assets/TrainGame/Scenes";
        private const string ScenePath = SceneFolder + "/TrainPrototype.unity";

        [MenuItem("Tools/Train Game/Create Demo Scene")]
        public static void Create()
        {
            EnsureFolder("Assets/TrainGame", "Data");
            EnsureFolder("Assets/TrainGame", "Scenes");

            StationData stationA = CreateOrLoadStation(DataFolder + "/Station_Departure.asset", "departure", "Departure Station");
            StationData stationB = CreateOrLoadStation(DataFolder + "/Station_Crossroads.asset", "crossroads", "Crossroads Station");
            StationData stationC = CreateOrLoadStation(DataFolder + "/Station_Terminal.asset", "terminal", "Terminal Station");

            RouteData route = AssetDatabase.LoadAssetAtPath<RouteData>(DataFolder + "/DemoRoute.asset");
            if (route == null)
            {
                route = ScriptableObject.CreateInstance<RouteData>();
                AssetDatabase.CreateAsset(route, DataFolder + "/DemoRoute.asset");

                SerializedObject serializedRoute = new SerializedObject(route);
                SerializedProperty stops = serializedRoute.FindProperty("stops");
                stops.arraySize = 3;
                ConfigureStop(stops.GetArrayElementAtIndex(0), stationA, new Vector3(-6f, 0f, 0f));
                ConfigureStop(stops.GetArrayElementAtIndex(1), stationB, Vector3.zero);
                ConfigureStop(stops.GetArrayElementAtIndex(2), stationC, new Vector3(6f, 0f, 0f));
                serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            }

            JourneyEventData journeyEvent = AssetDatabase.LoadAssetAtPath<JourneyEventData>(DataFolder + "/DemoEvent.asset");
            if (journeyEvent == null)
            {
                journeyEvent = ScriptableObject.CreateInstance<JourneyEventData>();
                AssetDatabase.CreateAsset(journeyEvent, DataFolder + "/DemoEvent.asset");

                SerializedObject serializedEvent = new SerializedObject(journeyEvent);
                serializedEvent.FindProperty("title").stringValue = "A signal in the fog";
                serializedEvent.FindProperty("description").stringValue =
                    "A weak signal appears beside the track. The prototype only logs choices for now.";

                SerializedProperty choices = serializedEvent.FindProperty("choices");
                choices.arraySize = 2;
                choices.GetArrayElementAtIndex(0).FindPropertyRelative("label").stringValue = "Slow down and inspect";
                choices.GetArrayElementAtIndex(0).FindPropertyRelative("resultText").stringValue = "The train slows.";
                choices.GetArrayElementAtIndex(1).FindPropertyRelative("label").stringValue = "Keep moving";
                choices.GetArrayElementAtIndex(1).FindPropertyRelative("resultText").stringValue = "The signal disappears behind you.";
                serializedEvent.ApplyModifiedPropertiesWithoutUndo();
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 10f, -12f);
            camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "Prototype Track";
            track.transform.position = Vector3.zero;
            track.transform.localScale = new Vector3(14f, 0.15f, 1f);

            GameObject train = GameObject.CreatePrimitive(PrimitiveType.Cube);
            train.name = "Prototype Train";
            train.transform.localScale = new Vector3(1.8f, 1f, 1f);
            TrainMovementController movement = train.AddComponent<TrainMovementController>();

            CreateStationMarker("Departure Station", new Vector3(-6f, 0.75f, 0f));
            CreateStationMarker("Crossroads Station", new Vector3(0f, 0.75f, 0f));
            CreateStationMarker("Terminal Station", new Vector3(6f, 0.75f, 0f));

            GameObject systems = new GameObject("Game Systems");
            GameFlowController flow = systems.AddComponent<GameFlowController>();

            SerializedObject serializedFlow = new SerializedObject(flow);
            serializedFlow.FindProperty("route").objectReferenceValue = route;
            serializedFlow.FindProperty("train").objectReferenceValue = movement;
            SerializedProperty events = serializedFlow.FindProperty("journeyEvents");
            events.arraySize = 1;
            events.GetArrayElementAtIndex(0).objectReferenceValue = journeyEvent;
            serializedFlow.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
            Debug.Log($"[TrainGame] Demo scene created: {ScenePath}");
        }

        private static StationData CreateOrLoadStation(string path, string id, string displayName)
        {
            StationData station = AssetDatabase.LoadAssetAtPath<StationData>(path);
            if (station != null)
                return station;

            station = ScriptableObject.CreateInstance<StationData>();
            AssetDatabase.CreateAsset(station, path);

            SerializedObject serializedStation = new SerializedObject(station);
            serializedStation.FindProperty("stationId").stringValue = id;
            serializedStation.FindProperty("displayName").stringValue = displayName;
            serializedStation.ApplyModifiedPropertiesWithoutUndo();

            return station;
        }

        private static void ConfigureStop(SerializedProperty stop, StationData station, Vector3 position)
        {
            stop.FindPropertyRelative("station").objectReferenceValue = station;
            stop.FindPropertyRelative("worldPosition").vector3Value = position;
        }

        private static void CreateStationMarker(string name, Vector3 position)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name;
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(1.25f, 0.25f, 1.25f);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
