using System;
using UnityEngine;

/// <summary>
/// Script encargado de gestionar el game over.
/// </summary>
public class GameOver_Scr : MonoBehaviour {
    AudioSource ControladorFX;
    AudioSource ControladorMusica;    
    Animator anim;

    public void Start() {
        anim = GetComponent<Animator>();
        ControladorFX = GetComponents<AudioSource>()[0];
        ControladorMusica = GetComponents<AudioSource>()[1];
    }
    
    /// <summary>
    /// Reproduce un efecto de sonido cuando una animación lo requiere.
    /// </summary>
    /// <param name="clip">Efecto de sonido a reproducir</param>
    public void ReproducirFX(AnimationEvent clip) {
        AudioClip fx = (AudioClip)clip.objectReferenceParameter;

        if (ControladorMusica.isPlaying) ControladorMusica.Stop();
        ControladorFX.clip = fx;
        ControladorFX.Play();
    }

    /// <summary>
    /// Reproduce una musica cuando una animación lo requiere.
    /// </summary>
    /// <param name="clip">Musica a reproducir</param>
    public void ReproducirMusica(AnimationEvent clip) {
        AudioClip musica = (AudioClip)clip.objectReferenceParameter;

        if (ControladorFX.isPlaying) ControladorFX.Stop();
        ControladorMusica.clip = musica;
        ControladorMusica.Play();
    }

    public void PararControladoresAudio() {
        ControladorFX.Stop();
        ControladorMusica.Stop();
    }

    public void GameOver() => anim.SetTrigger("GameOver");

    public void BorrarMinosTablero() => Array.ForEach(GameObject.FindGameObjectsWithTag("Minos"), Destroy);

    public void ResetearPanel() => anim.SetTrigger("ResetGame");

    public void ReiniciarJuego() => GameCtrlScr.gameCtrl.ReiniciarPartida();
}