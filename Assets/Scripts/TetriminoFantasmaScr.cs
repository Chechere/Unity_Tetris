using System;
using UnityEngine;

/// <summary>
/// Script que gestiona el Tetrimino fantasma de cada Tetrimino.
/// </summary>
public class TetriminoFantasmaScr : MonoBehaviour {
    MinosScr[] minosFantasma;

    void Start() {
        minosFantasma = GetComponentsInChildren<MinosScr>();
        ActualizarPosicion();
    }

    /// <summary>
    /// Actualiza su posición, según la posicion del tetrimino, 
    /// buscando la posición libre mas baja posible en el tablero
    /// </summary>
    public void ActualizarPosicion() {        
        transform.position = transform.parent.position;
        while (ComprobarEspacioVacio(Vector3.down)) transform.position += Vector3.down;
    }

    /// <summary>
    /// Actualiza su rotación según la rotación del tetrimino.
    /// </summary>
    /// <param name="minos">Minos del Tetrimino</param>
    public void ActualizarRotacion(MinosScr[] minos) {
        for (int i = 0; i < minos.Length; i++) {
            minosFantasma[i].transform.localPosition = minos[i].transform.localPosition;
            minosFantasma[i].NuevaRotacion(Quaternion.Euler(Vector3.zero));
        }

        ActualizarPosicion();
    }
    
    /// <summary>
    /// Comprueba si hay un espacio libre dada una dirección.
    /// </summary>
    /// <param name="dir">dirección a comprobar</param>
    /// <returns>true si hay espacio libre o false en caso contrario</returns>
    private bool ComprobarEspacioVacio(Vector3 dir) => !Array.Exists(minosFantasma, mino => !mino.ComprobarColision(dir));

    /// <summary>
    /// Evita que el tetrimino fantasma cambie su altura cuando el Tetrimino baja una posición.
    /// </summary>
    public void MantenerAltura() => transform.position += Vector3.up;
}