using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace DungeonGeneration
{
    public class DungeonStateController : MonoBehaviour
    {
        private AbstractDungeonGenerator dungeonGenerator;
        private NavMeshController navMeshController;

        // Start is called before the first frame update
        void Start()
        {
            dungeonGenerator = GetComponentInChildren<AbstractDungeonGenerator>();
            navMeshController = GetComponentInChildren<NavMeshController>();
            StartCoroutine(Initialize());
        }

        IEnumerator Initialize()
        {
            dungeonGenerator.GenerateDungeon(this);
            yield return new WaitUntil(() => dungeonGenerator.isDone);
            StartCoroutine(navMeshController.FixNavMeshAgent());
            yield return new WaitUntil(() => navMeshController.isDone);
            var rooms = dungeonGenerator.tree.nodes.OfType<RoomNode>().ToHashSet();
            var alienSpawnRoom = rooms.FirstOrDefault(r => r is EndRoom).bounds;
            NavMesh.SamplePosition(
                alienSpawnRoom.center,
                out NavMeshHit alienHit,
                alienSpawnRoom.size.magnitude,
                NavMesh.AllAreas
            );
            var startRoom = rooms.FirstOrDefault(r => r is StartRoom).bounds;

            NavMesh.SamplePosition(
                startRoom.center,
                out NavMeshHit playerHit,
                startRoom.size.magnitude,
                NavMesh.AllAreas
            );
            GameStateController.instance.SpawnPlayer(playerHit.position);
            GameStateController.instance.SpawnAlien(alienHit.position);
            GameStateController.instance.Toggle(IsEnabled.CAMERA, true);
            StartCoroutine(
                GameStateController.instance.cameraController.InitializeConfiner(
                    dungeonGenerator.parameters.dungeon.bounds
                )
            );
            GameStateController.instance.Toggle(IsEnabled.INPUT, true);
        }
    }
}
