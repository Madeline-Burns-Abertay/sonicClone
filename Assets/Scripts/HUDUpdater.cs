using TMPro;
using UnityEngine;
[RequireComponent (typeof(Player))]
public class HUDUpdater : MonoBehaviour
{
    public TMP_Text scoreDisplay, timer, ringCount, lifeCount;
    Player player;
    private void Start()
    {
        player = GetComponent<Player>();
    }
    void Update()
    {
        ringCount.text = player.getRings();
    }
}
