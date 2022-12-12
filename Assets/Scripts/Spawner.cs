using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using crass;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    private const int EnemySpawnAttemptsPerFrame = 10;

    [SerializeField] public List<WaveData> waves;

    [Header("References")] [SerializeField]
    private Mirror startMirror;

    [SerializeField] private IntVariable killCount;
    [SerializeField] private Vector2Variable playerPosition;
    [SerializeField] private BoolVariable playerDying;

    private int _waveIndex, _mirrorCount, _enemyCount;

    private void Start()
    {
        Mirror.ClearActiveMirrors();

        killCount.Value = 0;
        _mirrorCount = 1;
        startMirror.onDeath.AddListener(() => _mirrorCount--);
    }

    private void Update()
    {
        if (playerDying.Value) return;

        if (
            _waveIndex < waves.Count - 1 &&
            killCount.Value > waves[_waveIndex + 1].killThreshold
        ) _waveIndex++;

        var wave = waves[_waveIndex];

        var mirrorSpawnChance = wave.mirrorSpawnChance.Evaluate(_mirrorCount);
        if (RandomExtra.ChancePerSecond(mirrorSpawnChance)) SpawnMirror(wave);

        var enemySpawnChance = wave.enemySpawnChance.Evaluate(_enemyCount);
        if (RandomExtra.ChancePerSecond(enemySpawnChance)) SpawnEnemy(wave);
    }

    public void OnPlayerDyingChanged(bool value)
    {
        if (!value) return;

        StopAllCoroutines();
    }

    private void SpawnMirror(WaveData wave)
    {
        _mirrorCount++;

        var safe = Screen.safeArea;

        var pos = CameraCache.Main.ScreenToWorldPoint(new Vector2(
            Random.Range(safe.xMin, safe.xMax),
            Random.Range(safe.yMin, safe.yMax)
        ));
        pos.z = 0;

        var mirrorPrefab = wave.mirrorPrefabs.GetNext();

        var mirror = Instantiate(mirrorPrefab, pos, Quaternion.identity);
        mirror.onDeath.AddListener(() => _mirrorCount--);
    }

    private void SpawnEnemy(WaveData wave)
    {
        _enemyCount++;

        IEnumerator SpawnRoutine()
        {
            var enemyPrefab = wave.enemyPrefabs.GetNext();

            var spawned = false;
            while (!spawned)
            {
                for (var _ = 0; _ < EnemySpawnAttemptsPerFrame; _++)
                {
                    var spawnDist =
                        wave.enemyDistanceToPlayer.Evaluate(
                            Random.Range(0, wave.enemyDistanceToPlayer.keys.Last().time));
                    var spawnPosition = playerPosition.Value + Random.insideUnitCircle.normalized * spawnDist;

                    if (!Mirror.AnyContain(spawnPosition, enemyPrefab.ReflectionRadius)) continue;

                    var newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                    newEnemy.onDeath.AddListener(() => _enemyCount--);

                    spawned = true;
                    break;
                }

                yield return null;
            }
        }

        StartCoroutine(SpawnRoutine());
    }

    [Serializable]
    public struct WaveData
    {
        public int killThreshold;

        [Tooltip(
            "Time axis represents count of existing entities, value access is chance per second to spawn a new one."
        )]
        public AnimationCurve mirrorSpawnChance, enemySpawnChance;

        [Tooltip(
            "Every time an enemy attempts to spawn, its distance from the player will be randomly selected from this"
        )]
        // TODO: prevent cheese where player hangs out at edge of a mirror close to another mirror that doesn't overlap
        public AnimationCurve enemyDistanceToPlayer;

        public BagRandomizer<Enemy> enemyPrefabs;
        public BagRandomizer<Mirror> mirrorPrefabs;
    }
}