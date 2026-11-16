#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HW5SceneBuilder
{
    [MenuItem("Tools/Build Pokemon Capture Scene (HW5)")]
    public static void BuildScene()
    {
        Debug.Log("Starting automated build for HW5 Pokemon Capture Scene...");

        // 確保目錄存在
        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Scenes");
        EnsureDirectory("Assets/Materials");

        // 1. 建立全新空白場景
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 2. 建立專案材質庫
        Material redMat = CreateColorMaterial("Mat_Red", new Color(0.92f, 0.15f, 0.15f));
        Material whiteMat = CreateColorMaterial("Mat_White", new Color(0.95f, 0.95f, 0.95f));
        Material blackMat = CreateColorMaterial("Mat_Black", new Color(0.12f, 0.12f, 0.12f));
        Material yellowMat = CreateColorMaterial("Mat_Yellow", new Color(0.98f, 0.85f, 0.12f));
        Material blueMat = CreateColorMaterial("Mat_Blue", new Color(0.15f, 0.45f, 0.88f));
        Material skinMat = CreateColorMaterial("Mat_Skin", new Color(0.98f, 0.82f, 0.70f));
        Material greenMat = CreateColorMaterial("Mat_Green", new Color(0.2f, 0.65f, 0.25f));
        Material woodMat = CreateColorMaterial("Mat_Wood", new Color(0.45f, 0.28f, 0.15f));
        Material foliageMat = CreateColorMaterial("Mat_Foliage", new Color(0.22f, 0.55f, 0.25f));

        // 3. 調整定向光源 (Directional Light)
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight != null)
        {
            dirLight.name = "Directional Light";
            dirLight.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            dirLight.color = new Color(1f, 0.96f, 0.88f);
            dirLight.intensity = 1.3f;
            dirLight.shadows = LightShadows.Soft;
        }

        // 4. 建立與生成 Terrain (依據簡報 6-3 節)
        GameObject terrainObj = CreateCustomTerrain();

        // 5. 建立音效管理器 (AudioManager)
        GameObject audioObj = new GameObject("AudioManager");
        AudioManager audioMgr = audioObj.AddComponent<AudioManager>();
        audioMgr.throwClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Throw.wav");
        audioMgr.hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Hit.wav");
        audioMgr.captureClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Capture.wav");
        audioMgr.successClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Success.wav");

        // 6. 載入並配置 3D 訓練家模型 (左側)
        GameObject trainerObj = SetupTrainer(redMat, whiteMat, blueMat, skinMat, greenMat);

        // 7. 載入並配置 3D 寶可夢模型 (右側)
        GameObject pokemonObj = SetupPokemon(yellowMat, redMat, blackMat);

        // 8. 建立精靈球 Prefab (PokeballPrefab) 包含 Rigidbody, Collider, ParticleSystem
        GameObject pokeballPrefab = CreatePokeballPrefab(redMat, whiteMat, blackMat);

        // 9. 建立拋球產生器 (PokeballGenerator)
        GameObject genObj = new GameObject("PokeballGenerator");
        PokeballGenerator generator = genObj.AddComponent<PokeballGenerator>();
        generator.pokeballPrefab = pokeballPrefab;
        Transform throwHand = trainerObj.transform.Find("ThrowOrigin");
        generator.throwOrigin = throwHand != null ? throwHand : trainerObj.transform;
        generator.targetPokemon = pokemonObj.transform;
        generator.autoThrowOnStart = true;
        generator.autoThrowDelay = 1.2f;

        // 10. 建立遊戲導演 (GameDirector)
        GameObject directorObj = new GameObject("GameDirector");
        GameDirector director = directorObj.AddComponent<GameDirector>();
        director.generator = generator;
        director.pokemon = pokemonObj.GetComponent<PokemonTarget>();

        // 11. 在場景兩側及背景種植美麗樹木
        PlantSceneTrees(woodMat, foliageMat);

        // 12. 調整主攝影機位置 (正面視角清楚展示訓練家、精靈球由左至右軌跡、寶可夢與山脈背景)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.name = "Main Camera";
            mainCam.transform.position = new Vector3(0f, 2.2f, -1.5f);
            mainCam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            mainCam.fieldOfView = 60f;
        }

        // 13. 儲存場景並更新 Build Settings
        string scenePath = "Assets/Scenes/MainCaptureScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"Scene successfully built and saved to: {scenePath}");

        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("HW5 Automated Scene Build Completed Perfectly!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path);
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static Material CreateColorMaterial(string name, Color color, float smoothness = 0.35f)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Glossiness", smoothness);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static GameObject CreateCustomTerrain()
    {
        int width = 250;
        int length = 250;
        int height = 35;
        int resolution = 129;

        TerrainData tData = new TerrainData();
        tData.heightmapResolution = resolution;
        tData.size = new Vector3(width, height, length);

        // 生成山脈起伏高度圖 (背景 +Z 方向隆起山峰，中央平原平坦)
        float[,] heights = new float[resolution, resolution];
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float normX = (float)x / resolution;
                float normY = (float)y / resolution; // Y 對應 Z 方向

                // 在 Z > 0.45 區域形成雄偉起伏山脈
                if (normY > 0.40f)
                {
                    float mountainFactor = Mathf.SmoothStep(0f, 1f, (normY - 0.40f) / 0.60f);
                    float wave1 = Mathf.Sin(normX * 12f) * 0.4f + 0.4f;
                    float wave2 = Mathf.Cos(normX * 24f + normY * 10f) * 0.2f;
                    float wave3 = Mathf.PerlinNoise(normX * 6f, normY * 6f) * 0.5f;
                    heights[y, x] = (wave1 + wave2 + wave3) * mountainFactor * 0.75f;
                }
                else
                {
                    // 平原帶有些微自然起伏 (0.01)
                    heights[y, x] = Mathf.PerlinNoise(normX * 10f, normY * 10f) * 0.008f;
                }
            }
        }
        tData.SetHeights(0, 0, heights);

        // 設定地表紋理 Layer (Grass & Rock)
        Texture2D grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/GrassTexture.png");
        Texture2D rockTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/RockTexture.png");

        TerrainLayer grassLayer = new TerrainLayer();
        grassLayer.diffuseTexture = grassTex;
        grassLayer.tileSize = new Vector2(10, 10);

        TerrainLayer rockLayer = new TerrainLayer();
        rockLayer.diffuseTexture = rockTex;
        rockLayer.tileSize = new Vector2(15, 15);

        tData.terrainLayers = new TerrainLayer[] { grassLayer, rockLayer };

        // 刷上山體岩石紋理 (Alphamap)
        int alphaRes = tData.alphamapResolution;
        float[,,] alphaMaps = new float[alphaRes, alphaRes, 2];
        for (int y = 0; y < alphaRes; y++)
        {
            for (int x = 0; x < alphaRes; x++)
            {
                float normY = (float)y / alphaRes;
                if (normY > 0.60f)
                {
                    float rockBlend = Mathf.Clamp01((normY - 0.60f) / 0.35f);
                    alphaMaps[y, x, 0] = 1f - rockBlend; // Grass
                    alphaMaps[y, x, 1] = rockBlend;      // Rock
                }
                else
                {
                    alphaMaps[y, x, 0] = 1f;
                    alphaMaps[y, x, 1] = 0f;
                }
            }
        }
        tData.SetAlphamaps(0, 0, alphaMaps);

        AssetDatabase.CreateAsset(tData, "Assets/Materials/NewTerrainData.asset");

        GameObject terrainGo = Terrain.CreateTerrainGameObject(tData);
        terrainGo.name = "Terrain";
        // 居中對齊 (平原中心位於 (0, 0, 0))
        terrainGo.transform.position = new Vector3(-width / 2f, 0, -20);
        return terrainGo;
    }

    private static GameObject SetupTrainer(Material redMat, Material whiteMat, Material blueMat, Material skinMat, Material greenMat)
    {
        GameObject trainerRoot = new GameObject("Trainer_Ash");
        trainerRoot.transform.position = new Vector3(-3.8f, 0f, 5.0f);
        trainerRoot.transform.rotation = Quaternion.Euler(0, 35f, 0); // 面向右前方寶可夢

        GameObject trainerModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Trainer.obj");
        if (trainerModelPrefab != null)
        {
            GameObject visual = Object.Instantiate(trainerModelPrefab, trainerRoot.transform);
            visual.name = "VisualModel";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            // 依據子網格指派材質
            Renderer[] rends = visual.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                string n = r.gameObject.name;
                if (n.Contains("Cap")) r.material = redMat;
                else if (n.Contains("Skin") || n.Contains("Head")) r.material = skinMat;
                else if (n.Contains("Jacket") || n.Contains("Arm") || n.Contains("Pants")) r.material = blueMat;
                else if (n.Contains("Glove")) r.material = greenMat;
                else if (n.Contains("Shoe")) r.material = redMat;
                else r.material = whiteMat;
            }
        }

        // 標記右手投球點
        GameObject throwOrigin = new GameObject("ThrowOrigin");
        throwOrigin.transform.SetParent(trainerRoot.transform);
        throwOrigin.transform.localPosition = new Vector3(0.65f, 1.5f, 0.45f);

        CapsuleCollider col = trainerRoot.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1.0f, 0);
        col.height = 2.0f;
        col.radius = 0.45f;

        return trainerRoot;
    }

    private static GameObject SetupPokemon(Material yellowMat, Material redMat, Material blackMat)
    {
        GameObject pokemonRoot = new GameObject("Pokemon_Pikachu");
        pokemonRoot.transform.position = new Vector3(3.8f, 0f, 5.0f);
        pokemonRoot.transform.rotation = Quaternion.Euler(0, -45f, 0); // 面向左側訓練家

        GameObject pokeModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Pokemon.obj");
        if (pokeModelPrefab != null)
        {
            GameObject visual = Object.Instantiate(pokeModelPrefab, pokemonRoot.transform);
            visual.name = "VisualModel";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            Renderer[] rends = visual.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                string n = r.gameObject.name;
                if (n.Contains("Cheek")) r.material = redMat;
                else if (n.Contains("Black") || n.Contains("Eye") || n.Contains("Tip")) r.material = blackMat;
                else r.material = yellowMat;
            }
        }

        SphereCollider col = pokemonRoot.AddComponent<SphereCollider>();
        col.center = new Vector3(0, 0.8f, 0);
        col.radius = 0.85f;

        pokemonRoot.AddComponent<PokemonTarget>();
        return pokemonRoot;
    }

    private static GameObject CreatePokeballPrefab(Material redMat, Material whiteMat, Material blackMat)
    {
        GameObject ball = new GameObject("Pokeball");
        GameObject ballModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Pokeball.obj");
        if (ballModelPrefab != null)
        {
            GameObject visual = Object.Instantiate(ballModelPrefab, ball.transform);
            visual.name = "VisualModel";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            Renderer[] rends = visual.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                string n = r.gameObject.name;
                if (n.Contains("UpperRed")) r.material = redMat;
                else if (n.Contains("LowerWhite") || n.Contains("ButtonCenter")) r.material = whiteMat;
                else r.material = blackMat;
            }
        }

        // Rigidbody 物理
        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.35f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // SphereCollider
        SphereCollider sc = ball.AddComponent<SphereCollider>();
        sc.center = Vector3.zero;
        sc.radius = 0.4f;

        // 粒子特效 (ParticleSystem 飛濺特效，對應簡報 6-5-2)
        ParticleSystem ps = ball.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.0f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 4.5f;
        main.startSize = 0.25f;
        main.startColor = new Color(1f, 0.9f, 0.3f);
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 45) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        // 控制器腳本
        PokeballController controller = ball.AddComponent<PokeballController>();

        // 成功星芒特效子物件
        GameObject starsObj = new GameObject("SuccessStarsParticle");
        starsObj.transform.SetParent(ball.transform);
        starsObj.transform.localPosition = Vector3.zero;
        ParticleSystem starsPs = starsObj.AddComponent<ParticleSystem>();
        var sMain = starsPs.main;
        sMain.duration = 1.2f;
        sMain.loop = false;
        sMain.startLifetime = 1.0f;
        sMain.startSpeed = 3.0f;
        sMain.startSize = 0.35f;
        sMain.startColor = new Color(0.2f, 1f, 0.5f);
        sMain.playOnAwake = false;
        var sEmission = starsPs.emission;
        sEmission.rateOverTime = 0;
        sEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });

        controller.successStarsParticle = starsPs;

        // 儲存為 Prefab
        string prefabPath = "Assets/Prefabs/PokeballPrefab.prefab";
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(ball, prefabPath);
        Object.DestroyImmediate(ball);
        Debug.Log($"PokeballPrefab created at: {prefabPath}");
        return prefabAsset;
    }

    private static void PlantSceneTrees(Material trunkMat, Material foliageMat)
    {
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Tree.obj");
        if (treePrefab == null) return;

        GameObject treesParent = new GameObject("TreesContainer");

        // 在山腳、左側與右側後方自然錯落種植樹木
        Vector3[] treePositions = new Vector3[]
        {
            new Vector3(-8.5f, 0f, 8.5f),
            new Vector3(-6.2f, 0f, 12f),
            new Vector3(-10.5f, 0f, 15f),
            new Vector3(-3.5f, 0f, 14f),
            new Vector3(0f, 0f, 15f),
            new Vector3(3.5f, 0f, 14f),
            new Vector3(6.5f, 0f, 11f),
            new Vector3(8.8f, 0f, 8.0f),
            new Vector3(10.5f, 0f, 14f),
            new Vector3(-1.8f, 0f, 17f),
            new Vector3(2.0f, 0f, 18f),
            new Vector3(-5.0f, 0f, 19f),
            new Vector3(5.0f, 0f, 19f),
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            GameObject tree = Object.Instantiate(treePrefab, treePositions[i], Quaternion.Euler(0, Random.Range(0, 360), 0), treesParent.transform);
            tree.name = $"Tree_{i + 1}";
            float s = Random.Range(0.85f, 1.25f);
            tree.transform.localScale = new Vector3(s, s, s);

            Renderer[] rends = tree.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                if (r.gameObject.name.Contains("Trunk")) r.material = trunkMat;
                else r.material = foliageMat;
            }
        }
    }
}
#endif
