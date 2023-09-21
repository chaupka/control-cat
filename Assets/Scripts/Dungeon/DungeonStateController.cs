using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Utility;

namespace DungeonGeneration
{
    public class DungeonStateController : MonoBehaviour, IInteract
    {
        [HideInInspector]
        public AbstractDungeonGenerator dungeonGenerator;
        private NavMeshController navMeshController;
        public HashSet<RoomNode> rooms;
        public RoomNode playerRoom;
        public Queue<HashSet<Vector2Int>> platforms = new();
        public int maxPlatforms;
        public int platformCounter;

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
            StartCoroutine(navMeshController.BakeNavMesh());
            yield return new WaitUntil(() => navMeshController.isDone);
            rooms = dungeonGenerator.tree.nodes.OfType<RoomNode>().ToHashSet();
            var endRoom = rooms.FirstOrDefault(r => r is EndRoom).bounds;
            NavMesh.SamplePosition(
                endRoom.center,
                out NavMeshHit cheeseHit,
                endRoom.size.magnitude,
                NavMesh.AllAreas
            );
            GameStateController.singleton.SpawnCheese(cheeseHit.position);
            var startRoom = rooms.FirstOrDefault(r => r is StartRoom).bounds;

            NavMesh.SamplePosition(
                startRoom.center,
                out NavMeshHit playerHit,
                startRoom.size.magnitude,
                NavMesh.AllAreas
            );
            GameStateController.singleton.SpawnPlayer(playerHit.position);
            GameStateController.singleton.Toggle(IsEnabled.CAMERA, true);
            StartCoroutine(
                GameStateController.singleton.cameraController.InitializeConfiner(
                    dungeonGenerator.parameters.dungeon.bounds
                )
            );
            GameStateController.singleton.Toggle(IsEnabled.INPUT, true);
        }

        public bool Interact(Transform transform)
        {
            var vents = dungeonGenerator.tree.nodes.OfType<VentNode>();
            var onParentVent = vents.FirstOrDefault(
                v => v.parentVentPositions.Any(pv => Vector2.Distance(pv, transform.position) < 2)
            );
            var onChildVent = vents.FirstOrDefault(
                v => v.childVentPositions.Any(pv => Vector2.Distance(pv, transform.position) < 2)
            );
            if (onParentVent)
            {
                var destination = onParentVent.childVentPositions.ElementAt(0);
                transform.position = new Vector2(destination.x, destination.y + 1);
                return true;
            }
            else if (onChildVent)
            {
                var destination = onChildVent.parentVentPositions.ElementAt(0);
                transform.position = new Vector2(destination.x, destination.y + 1);
                return true;
            }
            return false;
        }

        public void CopyPlatform(Vector2Int position)
        {
            platformCounter += 1;
            var platformCenter = position + Vector2Int.down * 2;
            var platform = new HashSet<Vector2Int>(
                new[]
                {
                    platformCenter + Vector2Int.left,
                    platformCenter,
                    platformCenter + Vector2Int.right
                }
            );
            platforms.Enqueue(platform);
            dungeonGenerator.CreatePlatform(platform);
            if (platforms.Count > maxPlatforms)
            {
                dungeonGenerator.ClearPlatform(platforms.Dequeue());
            }
            if (platformCounter % maxPlatforms == 0)
            {
                StartCoroutine(CopyCat());
            }
            else
            {
                GameStateController.singleton.audioState.PlaySound(Sound.PlatformSpawn);
            }
            navMeshController.Rebake();
        }

        private IEnumerator CopyCat()
        {
            yield return new WaitUntil(() => navMeshController.isDone);
            GameStateController.singleton.audioState.PlaySound(Sound.CatSpawn);
            var potSpawnRooms = rooms.Where(r => !r.Equals(playerRoom)).ToHashSet();
            var alienSpawnRoom = potSpawnRooms
                .ElementAt(Random.Range(0, potSpawnRooms.Count))
                .bounds;
            NavMesh.SamplePosition(
                alienSpawnRoom.center,
                out NavMeshHit alienHit,
                alienSpawnRoom.size.magnitude,
                NavMesh.AllAreas
            );
            GameStateController.singleton.SpawnAlien(alienHit.position);
        }
    }
}
