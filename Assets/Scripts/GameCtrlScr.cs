using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Encargado de gestionar todas las variables del gameplay y el HUD.
/// </summary>
public class GameCtrlScr : MonoBehaviour {
    public static GameCtrlScr gameCtrl;
    public Vector2 PosicionInicioTetrimino;

    [Header("Velocidad Juego")]
    public int FramesIniciales;
    public int MaxFrames;    

    [Header("Musica")]
    public AudioClip[] MusicasJuego;
    public MusicScr ControladorDeMusica;    
    public int NumeroNivelesCambiarMusica;
    private int cancionActual = 0;

    [Header("Arrays de Tetriminos")]
    public GameObject[] Tetriminos;
    public GameObject[] TetriminosSiguiente;
    public GameObject[] TetriminosGuardado;

    [Header("Textos")]
    public TMP_Text Texto_Puntuacion;
    public TMP_Text Texto_PuntuacionMaxima;
    public TMP_Text Texto_Lineas;
    public TMP_Text Texto_LineasNecesariasSiguienteNivel;
    public TMP_Text Texto_Nivel;

    [Header("Componentes del Game Controller")]
    public LineManagerScr ControladorDeLineas;        
    public GameOver_Scr gameOverPanel;

    private bool[] sacoDeTetriminos;
    private int siguienteTetrimino;

    /// <summary>
    /// Index del siguiente tetrimino del array TetriminosSiguiente.                 
    /// Tambien gestiona el sacoDeTetriminos y los siguientes tetriminos que salen en la partida.
    /// </summary> 
    /// <remarks>Este index es el mismo para el resto de arrays</remarks>
    public int SiguienteTetrimino {
        get => siguienteTetrimino;

        set {            
            TetriminosSiguiente[siguienteTetrimino].SetActive(false);
            ComprobarSaco();
            siguienteTetrimino = SacarTetriminoDeSaco(value);            
            TetriminosSiguiente[siguienteTetrimino].SetActive(true);
        }
    }

    private int tetriminoGuardado;
    /// <summary>
    /// Index del tetrimino guardado dentro del array TetriminosGuardado. 
    /// Tambien gestiona el intercambio entre el tetrimino activo y el tetrimino guardado.
    /// </summary>
    public int TetriminoGuardado {
        get => tetriminoGuardado;

        set {
            if (tetriminoGuardado != -1) {                
                TetriminosGuardado[tetriminoGuardado].SetActive(false);
                GenerarTetriminoGuardado();
            } else {
                TetriminoScr tetrimino = GenerarTetriminoNuevo();
                tetrimino.InstanciadoDesdeGuardado = true;
            }

            tetriminoGuardado = value;
            TetriminosGuardado[tetriminoGuardado].SetActive(true);
        }
    }

    private const long LIMITEPUNTUACION = 9999999999;
    private int puntuacion;
    public int Puntuacion {
        get => puntuacion;

        set {
            puntuacion = (int)Mathf.Min(value, LIMITEPUNTUACION);
            Texto_Puntuacion.text = puntuacion.ToString();
        }
    }
    
    private int lineas;
    public int Lineas {
        get => lineas;

        set {
            lineas = value;
            Texto_Lineas.text = lineas.ToString();
        }
    }

    private int nivel;
    public int Nivel {
        get => nivel;

        set {
            nivel = value;

            if (nivel % NumeroNivelesCambiarMusica == 0) SiguienteCancion();

            Texto_Nivel.text = nivel.ToString();
        }
    }

    private const string CLAVEPUNTUACIONMAXIMA = "Puntuacion_Maxima";
    private int puntuacionMaxima;
    public int PuntuacionMaxima {
        get => puntuacionMaxima;
        set {
            if (PlayerPrefs.HasKey(CLAVEPUNTUACIONMAXIMA)) {
                if (puntuacionMaxima < value) puntuacionMaxima = value;
                else puntuacionMaxima = PlayerPrefs.GetInt(CLAVEPUNTUACIONMAXIMA);
            } else puntuacionMaxima = value;

            PlayerPrefs.SetInt(CLAVEPUNTUACIONMAXIMA, puntuacionMaxima);
            Texto_PuntuacionMaxima.text = puntuacionMaxima.ToString();
        }
    }

    private const int BASELINEASNECESARIAS = 10;
    private const int EXTRALINEASNECESARIAS = 2;
    private int lineasNecesariasSiguienteNivel;
    public int LineasNecesariasSiguienteNivel {
        get => lineasNecesariasSiguienteNivel;

        set {
            lineasNecesariasSiguienteNivel = value;

            if (lineasNecesariasSiguienteNivel <= 0) {
                Nivel++;
                lineasNecesariasSiguienteNivel += BASELINEASNECESARIAS + EXTRALINEASNECESARIAS * Nivel;
            }

            Texto_LineasNecesariasSiguienteNivel.text = lineasNecesariasSiguienteNivel.ToString();
        }
    }

    private bool gameOver;    
    public void Awake() {        
        Singleton();
        IniciarJuego();    
    }

    /// <summary>
    /// Evita que se generen multiples instancias del GameCtrl 
    /// durante la partida destruyendo los siguientes GameCtrl.
    /// </summary>
    private void Singleton() {
        if (EsElPrimero()) gameCtrl = this;
        else Destroy(this);
    }

    /// <summary>
    /// Detecta si es el primer GameCtrl creado de la partida.
    /// </summary>
    /// <returns>True si es el primero o si aun no ha habido un primero o False en caso contrario</returns>
    private bool EsElPrimero() => gameCtrl == this || gameCtrl == null;

    /// <summary>
    /// Inicia el juego inicializando las variables del hud y generando el primer tetrimino.
    /// </summary>
    private void IniciarJuego() {
        InicializarVariables();
        StartCoroutine(Inputs());
        GenerarTetriminoNuevo();
    }

    private void InicializarVariables() {
        Nivel = 0;
        Puntuacion = Lineas = 0;        
        LineasNecesariasSiguienteNivel = BASELINEASNECESARIAS;
        tetriminoGuardado = -1;
        PuntuacionMaxima = 0;       

        cancionActual = 0;
        ControladorDeMusica.CambiarSonido(MusicasJuego[0]);

        Array.ForEach(TetriminosGuardado, n => n.SetActive(false));
        sacoDeTetriminos = new bool[Tetriminos.Length];
        SiguienteTetrimino = Random.Range(0, Tetriminos.Length);
    }

    /// <summary>
    /// Genera el siguiente tetrimino del array TetriminosSiguiente.
    /// </summary>
    /// <returns>Devuelve el script del tetrimino generado</returns>
    private TetriminoScr GenerarTetriminoNuevo() {
        TetriminoScr tetrimino = Instantiate(Tetriminos[siguienteTetrimino], PosicionInicioTetrimino, Quaternion.identity).GetComponent<TetriminoScr>();
        SiguienteTetrimino = Random.Range(0, Tetriminos.Length);

        return tetrimino;
    }
    
    /// <summary>
    /// Saca al tablero el tetrimino que estaba guardado.
    /// </summary>
    private void GenerarTetriminoGuardado() {
        TetriminoScr tetrimino = Instantiate(Tetriminos[tetriminoGuardado], PosicionInicioTetrimino, Quaternion.identity).GetComponent<TetriminoScr>();
        tetrimino.InstanciadoDesdeGuardado = true;
    }

    /// <summary>
    /// Comprueba el estado actual del saco de tetriminos.
    /// </summary>
    private void ComprobarSaco() {
        if (SacoDeTetriminosVacio()) RellenarSaco();
    }
    
    /// <summary>
    /// Comprueba si han salido todos los tetriminos del saco.
    /// </summary>
    /// <returns>true esta vacio o false en caso contrario.</returns>
    private bool SacoDeTetriminosVacio() => !Array.Exists(sacoDeTetriminos, tetrimino => !tetrimino);

    /// <summary>
    /// Rellena el saco con nuevos tetriminos.
    /// </summary>
    /// <remarks>Rellena con un tetrimino de cada tipo.</remarks>
    private void RellenarSaco() => sacoDeTetriminos = new bool[Tetriminos.Length];

    /// <summary>
    /// Saca un tetrimino aleatorio del saco de tetriminos.
    /// </summary>   
    /// <param name="valorInicial">
    /// Primer valor que se comprueba del saco. 
    /// Por defecto vale 0.
    /// </param>      
    /// <returns>El index del tetrimino a sacar.</returns>     
    private int SacarTetriminoDeSaco(int valorInicial) {
        int tetrimino = ElegirTetriminoDeSaco(valorInicial);
        sacoDeTetriminos[tetrimino] = true;
        return tetrimino;
    }

    /// <summary>
    /// Elige un tetrimino aleatorio del saco que aun no haya salido
    /// </summary>
    /// <param name="index">indice inicial del tetrimino a comprobar</param>
    /// <returns>El indice del primer tetrimino aleatorio que aun no haya salido del saco</returns>
    private int ElegirTetriminoDeSaco(int index) {
        while (sacoDeTetriminos[index]) index = Random.Range(0, Tetriminos.Length);
        return index;
    }

    /// <summary>
    /// Empieza el flujo para generar el siguiente tetrimino.
    /// </summary>
    /// <remarks>Este flujo no se inicia si se ha perdido</remarks>
    public void FinTetrimino() {
        if (gameOver) return;
        else DestruirLineasCompletas();
    }

    /// <summary>
    /// Comprueba si hay lineas completas y las destruye.
    /// </summary>
    private void DestruirLineasCompletas() => ControladorDeLineas.DestruirLineas();

    /// <summary>
    /// Funcion que recibe las lineas destruidas por el <see cref="ControladorDeLineas"/>.
    /// </summary>
    /// <param name="lineasDestruidas">Numero de lineas destruidas</param>
    public void ObtenerLineasDestruidas(int lineasDestruidas) { 
        Lineas += lineasDestruidas;
        LineasNecesariasSiguienteNivel -= lineasDestruidas;

        int puntos = (lineasDestruidas + Mathf.Max(lineasDestruidas - 1, 0)) * Nivel * 100;
        if (lineasDestruidas == 4) puntos += 100;

        SumarPuntos(puntos);

        ControladorDeMusica.ActualizarPitchMusica();
        GenerarTetriminoNuevo();
    }

    /// <summary>
    /// Indica al <see cref="MusicScr"/> que prepare la siguiente canción.
    /// </summary>
    private void SiguienteCancion() {
        cancionActual = (cancionActual == MusicasJuego.Length - 1) ? 0 : cancionActual + 1;
        ControladorDeMusica.CambiarSonido(MusicasJuego[cancionActual]);
    }

    public void SumarPuntos(int puntos) => Puntuacion += puntos;

    /// <summary>
    /// Transforma el número de frames por bajada (Según el nivel) a segundos.
    /// </summary>
    /// <returns>El tiempo entre una bajada y otra</returns>
    public float ObtenerTiempoMovimientoBajar() =>  ObtenerFramesDelNivel(FramesIniciales) / MaxFrames - Time.deltaTime;

    /// <summary>
    /// Frames entre una acción de bajar y otra.
    /// </summary>
    /// <param name="frames">Frames minimos (Nivel 0)</param>
    /// <returns>El numero de frames del nivel actual</returns>
    private float ObtenerFramesDelNivel(int frames) {
        //La formula para calcular el tiempo cambia dependiendo del nivel.
        if (Nivel <= 8) frames -= 5 * Nivel;
        else if (Nivel == 9) frames = 6;
        else if (Nivel >= 10 && Nivel <= 18) frames = 6 - (Nivel - 10) / 3 + 1;
        else frames = 2;

        return frames;
    }

    /// <summary>
    /// Guarda el tetrimino en pantalla y saca un nuevo tetrimino o el que estaba ya guardado.    
    /// </summary>
    /// <param name="tetrimino">Tetrimino a guardar</param>
    public void GuardarTetrimino(TetriminoScr tetrimino) {
        TetriminoGuardado = (int)tetrimino.Tetrimino;
        Destroy(tetrimino.gameObject);
    }

    public void GameOver() {
        gameOver = true;
        PararCorrutinas();

        ControladorDeMusica.ResetPitchMusica();
        ControladorDeMusica.PararMusica();

        PuntuacionMaxima = Puntuacion;

        gameOverPanel.GameOver();
    }

    /// <summary>
    /// Una vez el juego se acaba todas las corrutinas en funcionamiento son paradas para que no den fallas.
    /// </summary>
    private void PararCorrutinas() {
        ControladorDeLineas.StopAllCoroutines();
        ControladorDeMusica.StopAllCoroutines();
    }

    public void ReiniciarPartida() {
        gameOver = false;
        IniciarJuego();
    }

    public void SalirDelJuego() {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Comprueba inputs extra del juego
    /// </summary>    
    private IEnumerator Inputs() {
        while (!gameOver) {
            if (Input.GetKey(KeyCode.Escape)) SalirDelJuego();            
            yield return new WaitForEndOfFrame();
        }
    }

}