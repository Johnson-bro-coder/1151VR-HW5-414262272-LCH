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
        Material foliageMat = CreateColorMaterial("Mat_Foliage", new Color(0.18f, 0.52f, 0.22f));

        // 3. 調整定向光源 (Directional Light)
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight != null)
        {
            dirLight.name = "Directional Light";
            dirLight.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            dirLight.color = new Color(1f, 0.96f, 0.88f);
            dirLight.intensity = 1.35f;
            dirLight.shadows = LightShadows.Soft;
        }

        // 4. 建立與生成 Terrain (依據簡報 6-3 節，有連綿起伏山脈與鮮明草地岩石紋理)
        GameObject terrainObj = CreateCustomTerrain();

        // 5. 建立音效管理器 (AudioManager)
        GameObject audioObj = new GameObject("AudioManager");
        AudioManager audioMgr = audioObj.AddComponent<AudioManager>();
        audioMgr.throwClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Throw.wav");
        audioMgr.hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Hit.wav");
        audioMgr.captureClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Capture.wav");
        audioMgr.successClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pokeball_Success.wav");

        // 6. 載入並配置 3D 訓練家模型 (左側)
        GameObject trainerObj = SetupTrainer(redMat, whiteMat, blackMat, blueMat, skinMat, greenMat);

        // 7. 載入並配置 3D 寶可夢模型 (右側)
        GameObject pokemonObj = SetupPokemon(yellowMat, redMat, blackMat);

        // 8. 建立精靈球 Prefab (PokeballPrefab) 包含 Rigidbody, Collider, 發光粒子特效
        GameObject pokeballPrefab = CreatePokeballPrefab(redMat, whiteMat, blackMat);

        // 9. 建立拋球產生器 (PokeballGenerator)
        GameObject genObj = new GameObject("PokeballGenerator");
        PokeballGenerator generator = genObj.AddComponent<PokeballGenerator>();
        generator.pokeballPrefab = pokeballPrefab;
        Transform throwHand = trainerObj.transform.Find("RightShoulderPivot/ThrowOrigin");
        generator.throwOrigin = throwHand != null ? throwHand : trainerObj.transform;
        generator.targetPokemon = pokemonObj.transform;
        generator.trainerController = trainerObj.GetComponent<TrainerController>();
        generator.autoThrowOnStart = true;
        generator.autoThrowDelay = 1.2f;

        // 10. 建立遊戲導演 (GameDirector)
        GameObject directorObj = new GameObject("GameDirector");
        GameDirector director = directorObj.AddComponent<GameDirector>();
        director.generator = generator;
        director.pokemon = pokemonObj.GetComponent<PokemonTarget>();

        // 11. 在山腳與視野兩側種植自然美觀的 3D 樹木 (遵循簡報 49-50 頁)
        PlantSceneTrees(woodMat, foliageMat);

        // 12. 調整主攝影機位置 (遵循簡報 6-3-5 節，稍微俯瞰展示開闊草坪與後方壯麗起伏山嶺)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.name = "Main Camera";
            mainCam.transform.position = new Vector3(0f, 3.2f, -4.5f);
            mainCam.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
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

    private static Material GetOrCreateParticleMaterial()
    {
        string path = "Assets/Materials/Mat_ParticleGlow.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Texture2D glowTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/ParticleGlow.png");

        Shader s = Shader.Find("Mobile/Particles/Additive");
        if (s == null) s = Shader.Find("Particles/Standard Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        if (s == null) s = Shader.Find("Legacy Shaders/Particles/Additive");

        if (mat == null)
        {
            mat = new Material(s);
            if (glowTex != null) mat.mainTexture = glowTex;
            mat.color = new Color(1f, 0.92f, 0.35f, 1f);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = s;
            if (glowTex != null) mat.mainTexture = glowTex;
            mat.color = new Color(1f, 0.92f, 0.35f, 1f);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static GameObject CreateCustomTerrain()
    {
        int width = 160;
        int length = 160;
        int height = 48; // 最高山峰 48 米，高聳壯麗
        int resolution = 129;

        string terrainDataPath = "Assets/Materials/NewTerrainData.asset";
        TerrainData tData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainDataPath);
        if (tData == null)
        {
            tData = new TerrainData();
            AssetDatabase.CreateAsset(tData, terrainDataPath);
        }

        tData.heightmapResolution = resolution;
        tData.size = new Vector3(width, height, length);

        // 生成山脈高度圖：
        // 地形在 Z 軸位置從 -20 開始，長度 160。
        // 訓練家與寶可夢在 Z = 5 (normY ≈ 0.156)。
        // 從 normY = 0.20 (Z = 12) 開始拔地而起，後方形成起伏連綿的群峰！
        float[,] heights = new float[resolution, resolution];
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float normX = (float)x / (resolution - 1);
                float normY = (float)y / (resolution - 1); // 對應 Z 深度方向

                if (normY > 0.19f)
                {
                    float mountainFactor = Mathf.SmoothStep(0f, 1f, (normY - 0.19f) / 0.55f);
                    // 連綿起伏山嶺
                    float peak1 = Mathf.Sin(normX * Mathf.PI * 3f) * 0.38f + 0.42f;
                    float peak2 = Mathf.Cos(normX * Mathf.PI * 6f + normY * 4f) * 0.18f;
                    float noise = Mathf.PerlinNoise(normX * 6f, normY * 6f) * 0.35f;

                    // 左右山脊環抱，視野中央略低以呈現層疊遠山
                    float valleyShape = Mathf.Abs(normX - 0.5f) * 0.5f + 0.65f;
                    heights[y, x] = Mathf.Clamp01((peak1 + peak2 + noise) * mountainFactor * valleyShape);
                }
                else
                {
                    heights[y, x] = 0f;
                }
            }
        }
        tData.SetHeights(0, 0, heights);

        // 建立並儲存實體 TerrainLayer 資產（徹底消滅 Missing Layer 的白灰西洋棋盤！）
        Texture2D grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/GrassTexture.png");
        Texture2D rockTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/RockTexture.png");

        string grassLayerPath = "Assets/Materials/GrassLayer.terrainlayer";
        TerrainLayer grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(grassLayerPath);
        if (grassLayer == null)
        {
            grassLayer = new TerrainLayer();
            grassLayer.diffuseTexture = grassTex;
            grassLayer.tileSize = new Vector2(8, 8);
            AssetDatabase.CreateAsset(grassLayer, grassLayerPath);
        }
        else
        {
            grassLayer.diffuseTexture = grassTex;
            grassLayer.tileSize = new Vector2(8, 8);
            EditorUtility.SetDirty(grassLayer);
        }

        string rockLayerPath = "Assets/Materials/RockLayer.terrainlayer";
        TerrainLayer rockLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(rockLayerPath);
        if (rockLayer == null)
        {
            rockLayer = new TerrainLayer();
            rockLayer.diffuseTexture = rockTex;
            rockLayer.tileSize = new Vector2(10, 10);
            AssetDatabase.CreateAsset(rockLayer, rockLayerPath);
        }
        else
        {
            rockLayer.diffuseTexture = rockTex;
            rockLayer.tileSize = new Vector2(10, 10);
            EditorUtility.SetDirty(rockLayer);
        }

        tData.terrainLayers = new TerrainLayer[] { grassLayer, rockLayer };

        // 刷上山體岩石紋理 (Alphamap)：平原與山腳滿鋪翠綠草地，山嶺與山峰刷上岩石
        int alphaRes = tData.alphamapResolution;
        float[,,] alphaMaps = new float[alphaRes, alphaRes, 2];
        for (int y = 0; y < alphaRes; y++)
        {
            for (int x = 0; x < alphaRes; x++)
            {
                float normY = (float)y / alphaRes;
                if (normY > 0.35f)
                {
                    float rockBlend = Mathf.Clamp01((normY - 0.35f) / 0.30f);
                    alphaMaps[y, x, 0] = 1f - rockBlend; // Grass
                    alphaMaps[y, x, 1] = rockBlend;      // Rock
                }
                else
                {
                    alphaMaps[y, x, 0] = 1f; // 100% 翠綠草地
                    alphaMaps[y, x, 1] = 0f;
                }
            }
        }
        tData.SetAlphamaps(0, 0, alphaMaps);
        EditorUtility.SetDirty(tData);

        GameObject terrainGo = Terrain.CreateTerrainGameObject(tData);
        terrainGo.name = "Terrain";
        terrainGo.transform.position = new Vector3(-width / 2f, 0, -20);

        Terrain terrain = terrainGo.GetComponent<Terrain>();
        Material defaultTerrainMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Standard.mat");
        if (defaultTerrainMat != null)
        {
            terrain.materialTemplate = defaultTerrainMat;
        }
        else
        {
            Shader s = Shader.Find("Nature/Terrain/Standard");
            if (s == null) s = Shader.Find("Nature/Terrain/Diffuse");
            if (s != null) terrain.materialTemplate = new Material(s);
        }
        terrain.Flush();

        return terrainGo;
    }

    private static GameObject SetupTrainer(Material redMat, Material whiteMat, Material blackMat, Material blueMat, Material skinMat, Material greenMat)
    {
        GameObject trainerRoot = new GameObject("Trainer_Ash");
        trainerRoot.transform.position = new Vector3(-3.8f, 0f, 5.0f);
        trainerRoot.transform.rotation = Quaternion.Euler(0, 35f, 0); // 面向右前方寶可夢

        // 1. 身體與頭腿軀幹
        GameObject bodyObj = new GameObject("BodyVisual");
        bodyObj.transform.SetParent(trainerRoot.transform, false);

        GameObject bodyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Trainer_Body.obj");
        if (bodyPrefab == null) bodyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Trainer.obj");

        if (bodyPrefab != null)
        {
            GameObject bodyVisual = Object.Instantiate(bodyPrefab, bodyObj.transform);
            bodyVisual.name = "BodyMesh";
            bodyVisual.transform.localPosition = Vector3.zero;
            bodyVisual.transform.localRotation = Quaternion.identity;

            Renderer[] rends = bodyVisual.GetComponentsInChildren<Renderer>();
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

        // 2. 右肩膀樞紐 (RightShoulderPivot)
        GameObject shoulderPivot = new GameObject("RightShoulderPivot");
        shoulderPivot.transform.SetParent(trainerRoot.transform, false);
        shoulderPivot.transform.localPosition = new Vector3(0.42f, 1.45f, 0.0f);
        shoulderPivot.transform.localRotation = Quaternion.Euler(15f, 10f, 15f);

        // 3. 右手臂與手套
        GameObject armPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Trainer_RightArm.obj");
        if (armPrefab != null)
        {
            GameObject armVisual = Object.Instantiate(armPrefab, shoulderPivot.transform);
            armVisual.name = "ArmMesh";
            armVisual.transform.localPosition = Vector3.zero;
            armVisual.transform.localRotation = Quaternion.identity;

            Renderer[] rends = armVisual.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                string n = r.gameObject.name;
                if (n.Contains("Glove")) r.material = greenMat;
                else r.material = blueMat;
            }
        }

        // 4. 手中持有的展示用小精靈球 (掌心大小 0.25)
        GameObject ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Pokeball.obj");
        GameObject handBall = null;
        if (ballPrefab != null)
        {
            handBall = Object.Instantiate(ballPrefab, shoulderPivot.transform);
            handBall.name = "HandBall";
            handBall.transform.localPosition = new Vector3(0f, -0.60f, 0.22f);
            handBall.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            handBall.transform.localRotation = Quaternion.identity;

            Renderer[] rends = handBall.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                string n = r.gameObject.name;
                if (n.Contains("UpperRed")) r.material = redMat;
                else if (n.Contains("LowerWhite") || n.Contains("ButtonCenter")) r.material = whiteMat;
                else r.material = blackMat;
            }
        }

        // 5. 標記拋球起點 (ThrowOrigin)
        GameObject throwOrigin = new GameObject("ThrowOrigin");
        throwOrigin.transform.SetParent(shoulderPivot.transform, false);
        throwOrigin.transform.localPosition = new Vector3(0f, -0.60f, 0.35f);

        // 6. 掛載動作控制器
        TrainerController trainerController = trainerRoot.AddComponent<TrainerController>();
        trainerController.bodyTransform = bodyObj.transform;
        trainerController.rightShoulderPivot = shoulderPivot.transform;
        if (handBall != null) trainerController.handBall = handBall.transform;

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
        pokemonRoot.transform.rotation = Quaternion.Euler(0, -45f, 0);

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
            visual.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f); // 縮小為掌心標準精靈球尺寸

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
        rb.mass = 0.25f;
        rb.linearDamping = 0.02f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // SphereCollider
        SphereCollider sc = ball.AddComponent<SphereCollider>();
        sc.center = Vector3.zero;
        sc.radius = 0.12f;

        // 取得專屬發光粒子材質 (徹底解決洋紅色 Missing Material 破圖！)
        Material particleMat = GetOrCreateParticleMaterial();

        // 粒子特效 (ParticleSystem 飛濺特效，對應簡報 6-5-2)
        ParticleSystem ps = ball.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = ball.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null) psRenderer.material = particleMat;

        var main = ps.main;
        main.duration = 1.0f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 4.0f;
        main.startSize = 0.22f;
        main.startColor = new Color(1f, 0.95f, 0.3f, 1f);
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 45) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f;

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
        ParticleSystemRenderer starsRenderer = starsObj.GetComponent<ParticleSystemRenderer>();
        if (starsRenderer != null) starsRenderer.material = particleMat;

        var sMain = starsPs.main;
        sMain.duration = 1.2f;
        sMain.loop = false;
        sMain.startLifetime = 1.0f;
        sMain.startSpeed = 3.5f;
        sMain.startSize = 0.30f;
        sMain.startColor = new Color(0.3f, 1f, 0.6f, 1f);
        sMain.playOnAwake = false;
        var sEmission = starsPs.emission;
        sEmission.rateOverTime = 0;
        sEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) });

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
        Terrain terrain = Object.FindFirstObjectByType<Terrain>();

        // 遵循簡報 49-50 頁：樹木分佈在兩側山坡以及後方山腳，中央完全開闊露出山脈與精靈球拋物線！
        Vector3[] treePositions = new Vector3[]
        {
            // 左側山林
            new Vector3(-9.5f, 0f, 10f),
            new Vector3(-13.0f, 0f, 14f),
            new Vector3(-7.5f, 0f, 16f),
            new Vector3(-11.0f, 0f, 20f),
            new Vector3(-14.5f, 0f, 24f),
            // 右側山林
            new Vector3(9.5f, 0f, 10f),
            new Vector3(13.0f, 0f, 14f),
            new Vector3(7.5f, 0f, 16f),
            new Vector3(11.0f, 0f, 20f),
            new Vector3(14.5f, 0f, 24f),
            // 後方山腳景深 (兩側)
            new Vector3(-5.0f, 0f, 22f),
            new Vector3(5.0f, 0f, 22f),
            new Vector3(-3.0f, 0f, 27f),
            new Vector3(3.0f, 0f, 27f),
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            Vector3 pos = treePositions[i];
            if (terrain != null)
            {
                pos.y = terrain.SampleHeight(pos); // 自動貼合地表高度，絕不浮空
            }

            GameObject tree = Object.Instantiate(treePrefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), treesParent.transform);
            tree.name = $"Tree_{i + 1}";
            float s = Random.Range(0.9f, 1.3f);
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
