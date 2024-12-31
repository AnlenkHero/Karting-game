
using Kart.Controls;
using Unity.Cinemachine;
using UnityEngine;
using UnityUtils;
using Utilities;

namespace Kart
{
    public class KartSpawner : MonoBehaviour
    {
        [SerializeField] Circuit circuit;
        [SerializeField] AIDriverData aiDriverData;
        [SerializeField] GameObject[] aiKartPrefabs;
        [SerializeField] GameObject playerKartPrefab;
        [SerializeField] CinemachineCamera playerCamera;

        void Start()
        {
            /*var playerKart = Instantiate(playerKartPrefab, circuit.spawnPoints[0].position,
                circuit.spawnPoints[0].rotation);
            playerCamera.Follow = playerKart.transform;
            playerCamera.LookAt = playerKart.transform;*/

            for (int i = 0; i < circuit.spawnPoints.Length; i++)
            {
                new AIKartBuilder(aiKartPrefabs[i % aiKartPrefabs.Length])
                    .withCircuit(circuit)
                    .withDriverData(aiDriverData)
                    .withSpawnPoint(circuit.spawnPoints[i])
                    .build();
            }
        }

        class AIKartBuilder
        {
            GameObject prefab;
            AIDriverData data;
            Circuit circuit;
            Transform spawnPoint;

            public AIKartBuilder(GameObject prefab)
            {
                this.prefab = prefab;
            }

            public AIKartBuilder withDriverData(AIDriverData data)
            {
                this.data = data;
                return this;
            }

            public AIKartBuilder withCircuit(Circuit circuit)
            {
                this.circuit = circuit;
                return this;
            }

            public AIKartBuilder withSpawnPoint(Transform spawnPoint)
            {
                this.spawnPoint = spawnPoint;
                return this;
            }

            public GameObject build()
            {
                var instance = Object.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                var aiInput = instance.GetOrAdd<AIInput>();
                aiInput.AddCircuit(circuit);
                aiInput.AddDriverData(data);
                instance.GetComponent<KartController>().SetInput(aiInput);

                return instance;
            }
        }
    }
}