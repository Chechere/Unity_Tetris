using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Script encargado de gestionar la musica del juego.
/// </summary>
public class MusicScr : MonoBehaviour {
    [Range(0,20)]
    public int[] AlturasPeligrosas;

    [Header("Pitch")]
    public float TiempoCambioPitch;
    public float ExtraPitch;
    private const int PITCHBASE = 1;

    AudioSource altavozTetris;

    private AudioClip siguienteCancion;
    private bool altavozParado;
   
    void Start() {
        altavozTetris = GetComponent<AudioSource>();
    }

    private void Update() {
        if (!altavozTetris.isPlaying && !altavozParado) {
            if (siguienteCancion != null) {                
                altavozTetris.clip = siguienteCancion;                
                siguienteCancion = null;
            }

            altavozTetris.Play();
        }
    }

    public void PararMusica() {
        altavozParado = true;
        altavozTetris.Stop();
    }

    /// <summary>
    /// Aumenta o disminuye el pitch de la musica segun la altura de los tetriminos colocados por el jugador.
    /// </summary>
    public void ActualizarPitchMusica() {        
        int multiplicadorPitch = ObtenerMultiplicadorPitch();
        float nuevoPitch = PITCHBASE + multiplicadorPitch * ExtraPitch / AlturasPeligrosas.Length;
        StartCoroutine(CambiarPitch(nuevoPitch));
    }

    public void ResetPitchMusica() => altavozTetris.pitch = 1;  
    
    /// <summary>
    /// Genera un multiplicador dependiendo de la altura de los tetriminos en el tablero.
    /// </summary>
    /// <returns>Multiplicador para el pitch de la musica.</returns>
    private int ObtenerMultiplicadorPitch() {        
        int alturaMaxima = ObtenerAlturaMaximaMinos();
        return Array.FindAll(AlturasPeligrosas, h => alturaMaxima >= h).Length;
    }    
    
    /// <summary>
    /// Busca el tetrimino mas alto.
    /// </summary>
    /// <returns>La posicion Y del tetrimino mas alto.</returns>
    private int ObtenerAlturaMaximaMinos() {        
        float MaxAltura = GameObject.FindGameObjectsWithTag("Minos")
                                    .ToList()
                                    .Select(mino => mino.transform.position.y)
                                    .Max();
        return (int)MaxAltura + 1;
    }

    /// <summary>
    /// Cambia la cancion que se esta reproduciendo.
    /// </summary>
    /// <param name="clip">Nueva cancion a reproducir</param>
    /// <remarks>
    /// Solo la cambia cuando la anterior a terminado. 
    /// Para mas informacion ver: <seealso cref="Update"/>
    /// </remarks>
    public void CambiarSonido(AudioClip clip) {
        altavozParado = false;
        siguienteCancion = clip;
    }

    /// <summary>
    /// Cambia el pitch de manera progresiva.
    /// </summary>
    /// <param name="nuevoPitch">Nuevo pitch de la cancion</param>    
    IEnumerator CambiarPitch(float nuevoPitch) {
        while (altavozTetris.pitch != nuevoPitch) {
            altavozTetris.pitch = Mathf.Lerp(altavozTetris.pitch, nuevoPitch, TiempoCambioPitch);
            yield return new WaitForEndOfFrame();
        }
    }
}