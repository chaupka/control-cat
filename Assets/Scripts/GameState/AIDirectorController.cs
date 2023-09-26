using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIDirectorController : MonoBehaviour
{
    public GameObject player;
    public List<GameObject> aliens;

    [SerializeField]
    private float hearAlienDistance;
    private bool isCloseToAlien;
    Coroutine trackAliens;

    public void Initialize()
    {
        aliens = new();
        trackAliens = StartCoroutine(CheckDistanceToAliens());
    }

    private IEnumerator CheckDistanceToAliens()
    {
        while (1 < 2)
        {
            yield return new WaitForSeconds(0.5f);
            var wasCloseToAlien = isCloseToAlien;
            isCloseToAlien = aliens.Any(a =>
            {
                return Vector2.Distance(player.transform.position, a.transform.position)
                    < hearAlienDistance;
            });
            if (!wasCloseToAlien && isCloseToAlien)
            {
                GameStateController.singleton.audioState.PlaySound(Sound.CatClose);
            }
            if (wasCloseToAlien && !isCloseToAlien)
            {
                GameStateController.singleton.audioState.StopSound(Sound.CatClose);
            }
        }
    }

    public void Disable()
    {
        StopCoroutine(trackAliens);
        player = null;
        aliens.Clear();
    }
}
