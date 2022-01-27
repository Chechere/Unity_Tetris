using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script encargado de gestionar al tetrimino activo.
/// Para mas info tambien mirar: <seealso cref="MinosScr"/>
/// </summary>
/// <remarks> Un tetrimino es cada una de las fichas que el jugador controla y usa para hacer lineas</remarks>
public class TetriminoScr : MonoBehaviour {
    private MinosScr[] minos;
    private AudioSource ControladorAudio;
    private WallKickScr wallKickScr; //Objeto a llamar si al rotar choca con algo.

    public enum TiposTetriminos {
        O = 0,
        I = 1,
        Z = 2,
        S = 3,
        T = 4,
        L = 5,
        J = 6
    };
    public TiposTetriminos Tetrimino;

    public LayerMask CapaSuelo;

    public TetriminoFantasmaScr tetriminoFantasma;

    public bool InstanciadoDesdeGuardado;

    [Header("Rotaciones")]
    public int[] Rotaciones;
    private int rotacionActual;
    private int direccionRotacion = 1;

    [Header("Inputs")]
    private int inputMovHor;
    private int inputCaidaInstantanea = 1, inputCaidaRapida = 1;
    private int inputRot;

    [Header("Tiempos")]
    public float TiempoBloqueo;
    private float tiempoBloqueoTranscurrido = 0;

    private float TiempoMovBajarInicial;
    private float TiempoMovBajar;
    private float UltimoMovBajar;

    public float TiempoEntreMovsHorizontales;
    private float tiempoTranscurridoMovHorizontal;

    [Header("Velocidades")]
    public int VelocidadCaidaInstantanea;
    public int VelocidadCaidaRapida;
    private int velocidadCaidaNormal = 1;

    private int velocidadHorizontal = 1;

    [Header("Efectos de Sonido")]
    public AudioClip RotacionFX;
    public AudioClip MovimientoHorizontalFX;
    public AudioClip ColocadoFX;
    public AudioClip GameOverFX;

    [Header("Puntuaciones Extra")]
    public int PuntuacionCaidaRapida;
    public int PuntuacionCaidaInstantanea;

    #region Inicio
    private void Start() {
        ObtenerComponentes();
        InicializarVariables();

        if (ComprobacionGameOver()) GameOver();
    }

    void ObtenerComponentes() {
        minos = Array.FindAll(GetComponentsInChildren<MinosScr>(), mino => !mino.name.Contains("Shadow"));
        ControladorAudio = GetComponent<AudioSource>();
        wallKickScr = gameObject.AddComponent<WallKickScr>();
    }

    void InicializarVariables() {
        UltimoMovBajar = Time.time;
        tiempoTranscurridoMovHorizontal = TiempoEntreMovsHorizontales;
        TiempoMovBajarInicial = ObtenerTiempoBajar();
    }

    private float ObtenerTiempoBajar() => GameCtrlScr.gameCtrl.ObtenerTiempoMovimientoBajar();

    private bool ComprobacionGameOver() => !ComprobarEspacioVacio(Vector3.back);

    private void GameOver() {
        GameCtrlScr.gameCtrl.GameOver();
        Destroy(gameObject);
    }
    #endregion

    #region Inputs   
    private void ObtenerInputs() {
        ReiniciarInputs();
        InputsHorizontales();
        InputsRotaciones();
        InputsCaidas();
    }
    
    private void ReiniciarInputs() {
        inputMovHor = 0;
        inputRot = 0;
        inputCaidaRapida = 1;
        //inputCaidaInstantanea no se reinicia entre frames.
    }

    private void InputsHorizontales() {
        inputMovHor = ComprobarTeclaMantenida(KeyCode.D, velocidadHorizontal, 0)
                    + ComprobarTeclaMantenida(KeyCode.A, -velocidadHorizontal, 0);
    }

    private void InputsRotaciones() {
        if (!HayRotaciones()) return;

        inputRot = ComprobarTeclaPulsada(KeyCode.RightArrow, direccionRotacion, 0)
                 + ComprobarTeclaPulsada(KeyCode.LeftArrow, -direccionRotacion, 0);
    }

    /// <summary>
    /// Comprueba si el tetrimino puede rotar
    /// </summary>
    /// <returns>true si tiene rotaciones o false en caso contrario</returns>
    /// <example>
    /// El tetrimino O o cuadrado daria falso por que no tiene rotaciones 
    /// (Rota sobre si mismo que en la practica es como no rotar).
    /// </example>
    private bool HayRotaciones() => Rotaciones.Length > 0;

    private void InputsCaidas() {
        inputCaidaInstantanea *= ComprobarTeclaPulsada(KeyCode.W, VelocidadCaidaInstantanea, velocidadCaidaNormal);
        inputCaidaRapida = ComprobarTeclaMantenida(KeyCode.S, VelocidadCaidaRapida, velocidadCaidaNormal);
    }

    /// <param name="key">Tecla a comprobar</param>
    /// <param name="teclaPulsada">Valor que devuelve si la tecla es pulsada.</param>
    /// <param name="teclaNoPulsada">Valor que devuelve si la tecla no es pulsada</param>    
    private int ComprobarTeclaPulsada(KeyCode key, int teclaPulsada, int teclaNoPulsada) {
        if (Input.GetKeyDown(key)) return teclaPulsada;
        else return teclaNoPulsada;
    }

    /// <param name="key">Tecla a comprobar</param>
    /// <param name="teclaPulsada">Valor que devuelve si la tecla es pulsada y mantenida.</param>
    /// <param name="teclaNoPulsada">Valor que devuelve si la tecla no es pulsada y mantenida</param>    
    private int ComprobarTeclaMantenida(KeyCode key, int teclaPulsada, int teclaNoPulsada) {
        if (Input.GetKey(key)) return teclaPulsada;
        else return teclaNoPulsada;
    }

    /// <summary>
    /// Comprueba si el jugador quiere guardar el tetrimino y si este no ha sido generado ya desde el guardado.
    /// </summary>
    /// <returns>true si quiere y puede o false en cada uno de los casos contrarios</returns>
    private bool QuererGuardarTetrimino() => ComprobarTeclaPulsada(KeyCode.Space, 1, 0) == 1 && !InstanciadoDesdeGuardado;
    #endregion

    #region Movimiento Horizontal

    /// <summary>
    /// Comprueba si quiere moverse el jugador, 
    /// si ha pasado el tiempo necesario para 
    /// moverse y si el espacio esta libre.
    /// </summary>
    /// <returns>true si quiere y puede o false en caso contrario</returns>
    /// <remarks>Si el jugador no quiere moverse el tiempo se resetea.</remarks>
    private bool QuererMoverse() {
        if (inputMovHor != 0) {
            return TiempoMovHorTranscurrido() && ComprobarEspacioVacio(Vector2.right * inputMovHor);
        } else {
            tiempoTranscurridoMovHorizontal = TiempoEntreMovsHorizontales;
            return false;
        }
    }

    /// <summary>
    /// Comprueba si ha pasado el tiempo necesario para poder moverse.
    /// </summary>
    /// <returns> true si ha pasado el tiempo o false en caso contrario.</returns>
    private bool TiempoMovHorTranscurrido() {
        tiempoTranscurridoMovHorizontal += Time.deltaTime;
        return tiempoTranscurridoMovHorizontal >= TiempoEntreMovsHorizontales;
    }

    private void Moverse() {
        tiempoTranscurridoMovHorizontal = 0;
        transform.position += Vector3.right * inputMovHor;

        ReproducirFX(MovimientoHorizontalFX);
        tetriminoFantasma.ActualizarPosicion();
    }
    #endregion

    #region Rotaciones

    /// <summary>
    /// Comprueba si el jugador si quiere rotar y ha podido rotar.
    /// </summary>
    /// <returns>true ha podido rotar o false en caso contrario.</returns>
    /// <remarks>
    /// Ha diferencia de los movimientos horizontal y vertical, 
    /// la rotacion primero la ejecutamos y luego comprobamos 
    /// si hay alguna posicion valida.
    /// </remarks>
    public bool HaRotado() => inputRot != 0 && PuedeRotar();

    /// <summary>
    /// Rota la figura y comprueba si no esta chocando con nada.
    /// </summary>
    /// <returns>
    /// true si no choca contra nada o se puede mover 
    /// para no chocar o false en caso contrario
    /// </returns>
    private bool PuedeRotar() {
        int rotacionOriginal = rotacionActual;
        InicioRotar();

        if (!ComprobarEspacioVacio(Vector3.back)) return WallKick(rotacionOriginal);
        else return true;
    }

    /// <summary>
    /// Prepara el tetrimino para rotar la pieza.
    /// </summary>
    private void InicioRotar() {
        rotacionActual += inputRot;

        if (rotacionActual < 0) rotacionActual = Rotaciones.Length - 1;
        else if (rotacionActual == Rotaciones.Length) rotacionActual = 0;

        Vector3 centro = minos[0].transform.position;
        Vector3 eje = Vector3.forward;
        int angulo = 90 * inputRot;
        int rotacionBuscada = Rotaciones[rotacionActual];

        Rotar(centro, eje, angulo, rotacionBuscada);
        RestaurarRotacionMinos();
    }

    /// <summary>
    /// Rota el tetrimino hasta que llega a la rotacion buscada.
    /// </summary>
    /// <param name="centro">Centro en el que rota el tetrimino.</param>
    /// <param name="eje">Eje en el que rota el tetrimino.</param>
    /// <param name="angulo">Angulo que se suma a la rotacion del tetrimino para llegar a la rotacion buscada.</param>
    /// <param name="rotacionBuscada">Rotacion Buscada para el tetrimino.</param>
    private void Rotar(Vector3 centro, Vector3 eje, int angulo, int rotacionBuscada) {
        while (transform.rotation.eulerAngles.z != rotacionBuscada) {
            transform.RotateAround(centro, eje, angulo);
        }
    }


    /// <summary>
    /// Rota los minos para que mantengan su rotacion de 0º.
    /// </summary>
    private void RestaurarRotacionMinos() => Array.ForEach(minos, mino => mino.NuevaRotacion(Quaternion.Euler(Vector3.zero)));

    /// <summary>
    /// Comprueba las posibles posiciones validas donde el tetrimino rotado no choque.
    /// </summary>
    /// <param name="posiciones">Posiciones a comprobar.</param>
    /// <param name="origen">Posicion inicial del tetrimino.</param>
    /// <returns>true si encuentra una posicion en la que no choque o false en caso contrario.</returns>
    private bool TesteaPosiciones(Vector2[] posiciones, Vector3 origen) {
        int i = 0;

        do {
            transform.position = origen;
            transform.position += (Vector3)posiciones[i] * inputRot;
            i++;
        } while (i < posiciones.Length && !ComprobarEspacioVacio(Vector3.back));

        return i != posiciones.Length;
    }

    /// <summary>
    /// Devuelve el tetrimino a su estado antes de rotar.
    /// </summary>
    /// <param name="origen">Posicion original del tetrimino.</param>
    private void RestauraRotacion(Vector3 origen) {
        transform.position = origen;
        inputRot = -inputRot;
        InicioRotar();
    }
    #endregion

    #region Colisiones

    /// <summary>
    /// Comprueba las posiciones dadas por <see cref="WallKickScr"/> 
    /// para ver si en alguna puede realizar la rotacion correctamente.
    /// </summary>
    /// <param name="rotacionOriginal"> antigua posición del tetrimino</param>
    private bool WallKick(int rotacionOriginal) {
        Vector3 origen = transform.position;
        Vector2[] posiciones = wallKickScr.ObtenerPosiciones(rotacionOriginal, rotacionActual, Tetrimino);

        if (TesteaPosiciones(posiciones, origen)) {
            return true;
        } else {
            RestauraRotacion(origen);
            return false;
        }
    }

    /// <summary>
    /// Comprueba si los minos pueden chocar dada una dirección.
    /// </summary>
    /// <param name="dir">Dirección a la que los minos miran.</param>
    /// <returns>true si no chocan contra nada o false en caso contrario.</returns>
    private bool ComprobarEspacioVacio(Vector3 dir) => !Array.Exists(minos, mino => !mino.ComprobarColision(dir));
    #endregion

    #region Movimiento Vertical

    /// <summary>
    /// Gestiona lo que tiene que hacer el tetrimino al bajar segun si puede o no bajar.
    /// </summary>
    private void GestionarMovimientoBajar() {
        if (ComprobarEspacioVacio(Vector3.down)) {
            if (ComprobarPuedeBajar()) MoverseHaciaAbajo();
        } else if (ComprobarBloquearTetrimino()) {
            BloquearTetrimino();
        }
    }

    /// <summary>
    /// Comprueba si ha pasado el tiempo para bajar una posicion.
    /// </summary>
    private bool ComprobarPuedeBajar() {
        TiempoMovBajarInicial = ObtenerTiempoBajar();
        TiempoMovBajar = inputCaidaInstantanea * TiempoMovBajarInicial / inputCaidaRapida;

        return Time.time - UltimoMovBajar > TiempoMovBajar;
    }

    /// <summary>
    /// Comprueba si se puede bloquear el tetrimino y si no se esta ya bloqueando.
    /// </summary>
    private bool ComprobarBloquearTetrimino() {
        tiempoBloqueoTranscurrido += Time.deltaTime;

        return tiempoBloqueoTranscurrido >= TiempoBloqueo && !TetriminoBloqueandose();
    }

    /// <summary>
    /// Bloquea el tetrimino en la posicion en la que esta.
    /// </summary>
    private void BloquearTetrimino() {
        ReproducirFX(ColocadoFX);
        StartCoroutine(DestruyeTetrimino());
    }

    /// <summary>
    /// Comprueba si el tetrimino se esta bloqueando
    /// </summary>
    /// <returns>true si acaba de bloquearse o false en caso contrario</returns>
    private bool TetriminoBloqueandose() => ControladorAudio.clip == ColocadoFX;

    /// <summary>
    /// Mueve una posición hacia abajo el tetrimino y suma puntos.
    /// </summary>
    private void MoverseHaciaAbajo() {
        tiempoBloqueoTranscurrido = 0;
        transform.position += Vector3.down;
        UltimoMovBajar = Time.time;

        tetriminoFantasma.MantenerAltura();
        SumarPuntos();
    }
    #endregion

    /// <summary>
    /// Contiene todo el loop jugable de los tetriminos.
    /// </summary>
    private void Update() {
        if (TetriminoBloqueandose()) return;

        ObtenerInputs();

        if (QuererGuardarTetrimino()) GameCtrlScr.gameCtrl.GuardarTetrimino(this);

        if (QuererMoverse()) Moverse();

        if (HaRotado()) {
            ReproducirFX(RotacionFX);
            tetriminoFantasma.ActualizarRotacion(minos);
        }

        GestionarMovimientoBajar();
    }

    /// <summary>
    /// Suma puntos al <see cref="GameCtrlScr.Puntuacion"/> 
    /// si esta bajando rapido o de manera instantanea
    /// </summary>
    private void SumarPuntos() {
        if (inputCaidaInstantanea == VelocidadCaidaInstantanea) GameCtrlScr.gameCtrl.SumarPuntos(PuntuacionCaidaInstantanea);
        else if (inputCaidaRapida == VelocidadCaidaRapida) GameCtrlScr.gameCtrl.SumarPuntos(PuntuacionCaidaRapida);
    }
        
    private void ReproducirFX(AudioClip fx) {
        ControladorAudio.clip = fx;
        ControladorAudio.Play();
    }

    /// <summary>
    /// Destruye el tetrimino liberando espacio de memoria e inicia el flujo para crear otro.
    /// </summary>
    /// <remarks>No destruye los minos pero los fija al mapa</remarks>    
    IEnumerator DestruyeTetrimino() {
        while (ControladorAudio.isPlaying) yield return new WaitForEndOfFrame();

        foreach (MinosScr mino in minos) {
            mino.gameObject.layer = (int)Mathf.Log(CapaSuelo.value, 2);
            mino.transform.parent = null;
            Destroy(mino); //Destruye el script no el objeto.        
        }

        Destroy(gameObject);
        GameCtrlScr.gameCtrl.FinTetrimino();
    }
}