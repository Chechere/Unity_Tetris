using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script encargado de gestionar las lineas completadas por el jugador.
/// </summary>
public class LineManagerScr : MonoBehaviour {
    public int AlturaCampo = 20, AnchuraCampo = 10;
    public float MultiplicadorDelay = 1.25f;
    public LayerMask CapaSuelo;
    public AudioClip lineaFX;
    public AnimationClip lineaCompleta;    

    private AudioSource audioSource;
    private Vector3 origen;
    private int lineasDestruidas = 0;
    private float duracionAnimacion;

    public void Awake() {
        origen = transform.position;
        audioSource = GetComponent<AudioSource>();
        duracionAnimacion = lineaCompleta.averageDuration;
    }

    /// <summary>
    /// Busca lineas completas y las destruye con su animacion y sonido pertinentes.
    /// </summary>    
    public void DestruirLineas() {
        for (int i = 1; i <= AlturaCampo; i++) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right, AnchuraCampo, CapaSuelo);
            hits = EliminarBackGroundDelArray(hits);

            if (hits.Length == 10) {
                lineasDestruidas++;
                StartCoroutine(IluminarLineas(hits));
            } else {                
                StartCoroutine(MoverHaciaAbajoLineas(hits, lineasDestruidas));
            }

            transform.position += Vector3.up;
        }

        Invoke("MandarLineas", duracionAnimacion * MultiplicadorDelay);        
    }

    /// <summary>
    /// Inicia la animación de linea completa y luego los destruye.
    /// </summary>
    /// <param name="minos">Minos dentro de la linea completa.</param>   
    private IEnumerator IluminarLineas(RaycastHit2D[] minos) {
        if (audioSource.clip == null) {
            audioSource.clip = lineaFX;
            audioSource.Play();
        }

        Array.ForEach(minos, mino => mino.collider.GetComponent<Animator>().SetTrigger("Borrar"));
        yield return new WaitForSeconds(duracionAnimacion);
        Array.ForEach(minos, mino => Destroy(mino.collider.gameObject));
    }

    /// <summary>
    /// Manda mover la linea hacia abajo tantas lineas borradas por debajo de esta.
    /// </summary>
    /// <param name="minos">minos que se mueven hacia abajo</param>
    /// <param name="numeroLineasHaciaAbajo">numero de lineas que tienen que moverse</param>
    private IEnumerator MoverHaciaAbajoLineas(RaycastHit2D[] minos, int numeroLineasHaciaAbajo) {
        yield return new WaitForSeconds(duracionAnimacion);
        Array.ForEach(minos, mino => mino.transform.position += Vector3.down * numeroLineasHaciaAbajo);
    }

    private RaycastHit2D[] EliminarBackGroundDelArray(RaycastHit2D[] array) {        
        Array.Reverse(array);
        Array.Resize(ref array, array.Length - 1);

        return array;
    }

    /// <summary>
    /// Manda las lineas al <see cref="GameCtrlScr"/> para gestionarlo
    /// y resetea el controlador.
    /// </summary>
    private void MandarLineas() {
        int lineas = lineasDestruidas;

        ResetControlador();
        GameCtrlScr.gameCtrl.ObtenerLineasDestruidas(lineas);
        
    }

    private void ResetControlador() {
        transform.position = origen;
        lineasDestruidas = 0;
        audioSource.clip = null;
    }
}