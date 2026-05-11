using UnityEngine;

public class RopeVisual : MonoBehaviour
{
    private GameObject player1;
    private GameObject player2;

    void Update()
    {
        // 1. Buscamos a los jugadores si aún no los tenemos
        if (player1 == null || player2 == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length >= 2)
            {
                player1 = players[0];
                player2 = players[1];
            }
            return; // Esperamos al siguiente frame si aún no están los dos
        }

        // 2. Posicionamos la soga justo en el medio de los dos
        Vector3 pos1 = player1.transform.position;
        Vector3 pos2 = player2.transform.position;
        transform.position = (pos1 + pos2) / 2;

        // 3. Calculamos la distancia para estirarla
        float distancia = Vector3.Distance(pos1, pos2);

        // 4. Aplicamos la escala (Ajusta el 0.1f si tu soga es muy gorda o flaca)
        transform.localScale = new Vector3(distancia, 0.1f, 1f);
    }
}