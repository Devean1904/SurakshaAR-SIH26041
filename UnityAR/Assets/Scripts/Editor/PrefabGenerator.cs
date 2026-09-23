using UnityEngine;
using UnityEditor;

namespace SurakshaAR.Editor
{
    public static class PrefabGenerator
    {
        private static readonly string PrefabPath = "Assets/Prefabs/Scenarios";

        [MenuItem("SurakshaAR/Generate Scenario Prefabs")]
        public static void GenerateAllPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(PrefabPath))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Scenarios");

            CreateFirePrefab();
            CreateChemicalSpillPrefab();
            CreateStructuralDamagePrefab();
            CreateEvacuationRoutePrefab();
            CreateSafetyEquipmentPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PrefabGenerator] All scenario prefabs generated!");
        }

        static void CreateFirePrefab()
        {
            var go = new GameObject("FireScenario");
            var fireLight = new GameObject("FireLight");
            fireLight.transform.SetParent(go.transform);
            var light = fireLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.5f, 0f);
            light.range = 5f;
            light.intensity = 2f;
            fireLight.transform.localPosition = Vector3.up * 0.5f;

            var fireVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fireVisual.transform.SetParent(go.transform);
            fireVisual.transform.localPosition = Vector3.up * 0.3f;
            fireVisual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            var fireRenderer = fireVisual.GetComponent<Renderer>();
            fireRenderer.material = new Material(Shader.Find("Standard"));
            fireRenderer.material.color = new Color(1f, 0.3f, 0f);
            fireRenderer.material.EnableKeyword("_EMISSION");
            fireRenderer.material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 2f);
            Object.DestroyImmediate(fireVisual.GetComponent<Collider>());

            var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.transform.SetParent(go.transform);
            smoke.transform.localPosition = Vector3.up * 1f;
            smoke.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            var smokeRenderer = smoke.GetComponent<Renderer>();
            smokeRenderer.material = new Material(Shader.Find("Standard"));
            smokeRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
            Object.DestroyImmediate(smoke.GetComponent<Collider>());

            SavePrefab(go, "FirePrefab");
        }

        static void CreateChemicalSpillPrefab()
        {
            var go = new GameObject("ChemicalSpillScenario");

            var puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puddle.transform.SetParent(go.transform);
            puddle.transform.localPosition = Vector3.zero;
            puddle.transform.localScale = new Vector3(1.2f, 0.02f, 0.8f);
            var puddleRenderer = puddle.GetComponent<Renderer>();
            puddleRenderer.material = new Material(Shader.Find("Standard"));
            puddleRenderer.material.color = new Color(0.2f, 0.8f, 0.2f);
            Object.DestroyImmediate(puddle.GetComponent<Collider>());

            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.transform.SetParent(go.transform);
            barrel.transform.localPosition = new Vector3(0.8f, 0.3f, 0);
            barrel.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            var barrelRenderer = barrel.GetComponent<Renderer>();
            barrelRenderer.material = new Material(Shader.Find("Standard"));
            barrelRenderer.material.color = new Color(0.8f, 0.2f, 0.2f);

            SavePrefab(go, "ChemicalSpillPrefab");
        }

        static void CreateStructuralDamagePrefab()
        {
            var go = new GameObject("StructuralDamageScenario");

            var crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crack.transform.SetParent(go.transform);
            crack.transform.localPosition = new Vector3(0, 1f, 0);
            crack.transform.localScale = new Vector3(0.05f, 1f, 0.1f);
            crack.transform.localRotation = Quaternion.Euler(0, 0, 15f);
            var crackRenderer = crack.GetComponent<Renderer>();
            crackRenderer.material = new Material(Shader.Find("Standard"));
            crackRenderer.material.color = Color.black;
            Object.DestroyImmediate(crack.GetComponent<Collider>());

            var debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.transform.SetParent(go.transform);
            debris.transform.localPosition = new Vector3(0.3f, 0.05f, 0.2f);
            debris.transform.localScale = new Vector3(0.2f, 0.1f, 0.15f);
            debris.transform.localRotation = Quaternion.Euler(10f, 45f, 0f);
            var debrisRenderer = debris.GetComponent<Renderer>();
            debrisRenderer.material = new Material(Shader.Find("Standard"));
            debrisRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);

            SavePrefab(go, "StructuralDamagePrefab");
        }

        static void CreateEvacuationRoutePrefab()
        {
            var go = new GameObject("EvacuationRouteScenario");

            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.transform.SetParent(go.transform);
            arrow.transform.localPosition = Vector3.zero;
            arrow.transform.localScale = new Vector3(0.15f, 0.02f, 0.8f);
            var arrowRenderer = arrow.GetComponent<Renderer>();
            arrowRenderer.material = new Material(Shader.Find("Standard"));
            arrowRenderer.material.color = Color.green;
            arrowRenderer.material.EnableKeyword("_EMISSION");
            arrowRenderer.material.SetColor("_EmissionColor", Color.green * 1.5f);
            Object.DestroyImmediate(arrow.GetComponent<Collider>());

            var arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowHead.transform.SetParent(go.transform);
            arrowHead.transform.localPosition = new Vector3(0, 0.02f, 0.5f);
            arrowHead.transform.localScale = new Vector3(0.3f, 0.02f, 0.15f);
            arrowHead.transform.localRotation = Quaternion.Euler(0, 45f, 0);
            var headRenderer = arrowHead.GetComponent<Renderer>();
            headRenderer.material = new Material(Shader.Find("Standard"));
            headRenderer.material.color = Color.green;
            headRenderer.material.EnableKeyword("_EMISSION");
            headRenderer.material.SetColor("_EmissionColor", Color.green * 1.5f);
            Object.DestroyImmediate(arrowHead.GetComponent<Collider>());

            SavePrefab(go, "EvacuationRoutePrefab");
        }

        static void CreateSafetyEquipmentPrefab()
        {
            var go = new GameObject("SafetyEquipmentScenario");

            var helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.transform.SetParent(go.transform);
            helmet.transform.localPosition = new Vector3(0, 0.8f, 0);
            helmet.transform.localScale = new Vector3(0.4f, 0.25f, 0.4f);
            var helmetRenderer = helmet.GetComponent<Renderer>();
            helmetRenderer.material = new Material(Shader.Find("Standard"));
            helmetRenderer.material.color = Color.yellow;
            Object.DestroyImmediate(helmet.GetComponent<Collider>());

            var stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stand.transform.SetParent(go.transform);
            stand.transform.localPosition = new Vector3(0, 0.3f, 0);
            stand.transform.localScale = new Vector3(0.08f, 0.3f, 0.08f);
            var standRenderer = stand.GetComponent<Renderer>();
            standRenderer.material = new Material(Shader.Find("Standard"));
            standRenderer.material.color = Color.gray;
            Object.DestroyImmediate(stand.GetComponent<Collider>());

            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.transform.SetParent(go.transform);
            sign.transform.localPosition = new Vector3(0, 1.2f, 0);
            sign.transform.localScale = new Vector3(0.5f, 0.3f, 0.02f);
            var signRenderer = sign.GetComponent<Renderer>();
            signRenderer.material = new Material(Shader.Find("Standard"));
            signRenderer.material.color = new Color(0f, 0.6f, 0f);
            signRenderer.material.EnableKeyword("_EMISSION");
            signRenderer.material.SetColor("_EmissionColor", Color.green);
            Object.DestroyImmediate(sign.GetComponent<Collider>());

            SavePrefab(go, "SafetyEquipmentPrefab");
        }

        static void SavePrefab(GameObject go, string name)
        {
            string path = $"{PrefabPath}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[PrefabGenerator] Created: {path}");
        }
    }
}
