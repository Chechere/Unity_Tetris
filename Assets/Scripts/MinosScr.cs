using UnityEngine;

/// <summary>
/// Encargado de gestionar los minos del tetrimino activo. Para mas info mirar tambien: <seealso cref="TetriminoScr"/>
/// </summary>
/// <remarks>Un mino es cada uno de los cubos que conforman el tetrimino</remarks>
public class MinosScr : MonoBehaviour {
    public LayerMask CapaSuelo;

    /// <summary>
    /// Comprueba si choca contra algo dada una dirección
    /// </summary>
    /// <param name="dir">dirección a la que mira.</param>
    /// <returns>true si no choca con nada o falso en caso contrario.</returns>
    public bool ComprobarColision(Vector3 dir) {
        RaycastHit2D hitsInfo = Physics2D.Raycast(transform.position, dir, dir.magnitude, CapaSuelo);
        return hitsInfo.collider == null;
    }
    
    /// <summary>
    /// Cambia su rotacion dado un <see cref="Quaternion"/>
    /// </summary>
    /// <param name="rot">Rotacion a la que se coloca el mino</param>
    public void NuevaRotacion(Quaternion rot) => transform.rotation = rot;
}