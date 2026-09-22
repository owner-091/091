using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TrainGame.Editor
{
    public static class CreateDemoScene
    {
        private const string DataFolder = "Assets/TrainGame/Data";
        private const string SceneFolder = "Assets/TrainGame/Scenes";
        private const string PrototypeArtFolder = "Assets/TrainGame/Art/Prototype";
        private const string ScenePath = SceneFolder + "/TrainInteriorPrototype.unity";
        private const string PixelPath = PrototypeArtFolder + "/PrototypePixel.png";

        [MenuItem("Tools/Train Game/Create Top-Down Demo Scene")]
        public static void Create()
        {
            EnsureFolder("Assets/TrainGame", "Data");
            EnsureFolder("Assets/TrainGame", "Scenes");
            EnsureFolder("Assets/TrainGame", "Art");
            EnsureFolder("Assets/TrainGame/Art", "Prototype");

            Sprite pixel = CreateOrLoadPrototypeSprite();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateTrainInterior(pixel);
            CreateConductor(pixel);
            CreatePassenger(pixel, new Vector2(-2f, 0.75f), "Prototype Passenger A");
            CreatePassenger(pixel, new Vector2(2f, -0.75f), "Prototype Passenger B");

            GameObject systems = new GameObject("Game Systems");
            systems.transform.position = Vector3.zero;

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

            Debug.Log($"[TrainGame] Top-down pixel prototype scene created: {ScenePath}");
        }

        private static void CreateCamera()
        {
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
        }

        private static void CreateTrainInterior(Sprite pixel)
        {
            GameObject root = new GameObject("Train Interior");

            CreateSpriteObject(
                root.transform,
                "Floor",
                pixel,
                Vector2.zero,
                new Vector2(11f, 5f),
                new Color(0.28f, 0.24f, 0.2f),
                0,
                false);

            CreateWall(root.transform, pixel, "Wall Top", new Vector2(0f, 2.65f), new Vector2(11.5f, 0.3f));
            CreateWall(root.transform, pixel, "Wall Bottom", new Vector2(0f, -2.65f), new Vector2(11.5f, 0.3f));
            CreateWall(root.transform, pixel, "Wall Left", new Vector2(-5.65f, 0f), new Vector2(0.3f, 5.6f));
            CreateWall(root.transform, pixel, "Wall Right", new Vector2(5.65f, 0f), new Vector2(0.3f, 5.6f));

            CreateSpriteObject(
                root.transform,
                "Dining Counter",
                pixel,
                new Vector2(0f, 1.6f),
                new Vector2(3.5f, 0.65f),
                new Color(0.4f, 0.28f, 0.18f),
                1,
                true);

            CreateSpriteObject(
                root.transform,
                "Cargo Stack",
                pixel,
                new Vector2(3.8f, 1.5f),
                new Vector2(1.2f, 1.2f),
                new Color(0.35f, 0.3f, 0.22f),
                1,
                true);

            CreateSpriteObject(
                root.transform,
                "Crew Desk",
                pixel,
                new Vector2(-3.8f, -1.5f),
                new Vector2(1.4f, 0.8f),
                new Color(0.32f, 0.24f, 0.18f),
                1,
                true);
        }

        private static void CreateConductor(Sprite pixel)
        {
            GameObject conductor = CreateSpriteObject(
                null,
                "Conductor",
                pixel,
                Vector2.zero,
                new Vector2(0.65f, 0.65f),
                new Color(0.85f, 0.82f, 0.52f),
                10,
                false);

            Rigidbody2D body = conductor.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.None;

            BoxCollider2D collider = conductor.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 0.8f);

            conductor.AddComponent<ConductorTopDownController>();
        }

        private static void CreatePassenger(Sprite pixel, Vector2 position, string name)
        {
            GameObject passenger = CreateSpriteObject(
                null,
                name,
                pixel,
                position,
                new Vector2(0.6f, 0.6f),
                new Color(0.55f, 0.65f, 0.78f),
                8,
                false);

            BoxCollider2D collider = passenger.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.85f, 0.85f);
        }

        private static void CreateWall(Transform parent, Sprite pixel, string name, Vector2 position, Vector2 scale)
        {
            CreateSpriteObject(
                parent,
                name,
                pixel,
                position,
                scale,
                new Color(0.15f, 0.13f, 0.12f),
                2,
                true);
        }

        private static GameObject CreateSpriteObject(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 scale,
            Color color,
            int sortingOrder,
            bool addCollider)
        {
            GameObject obj = new GameObject(name);
            if (parent != null)
                obj.transform.SetParent(parent);

            obj.transform.position = new Vector3(position.x, position.y, 0f);
            obj.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            if (addCollider)
                obj.AddComponent<BoxCollider2D>();

            return obj;
        }

        private static Sprite CreateOrLoadPrototypeSprite()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(PixelPath);
            if (existing != null)
                return existing;

            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(PixelPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(PixelPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PixelPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(PixelPath);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
