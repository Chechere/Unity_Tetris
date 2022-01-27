using UnityEngine;

/// <summary>
/// Script encargado de guardar y enviar las posiciones a testear por la rotación del tetrimino.
/// </summary>
public class WallKickScr : MonoBehaviour {    
    private readonly Vector2[,] WALLKICKS_CUATRO_ROTS_IZQ = new Vector2[4, 6] {
        { new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, -1), new Vector2(0, -2), new Vector2(-1, -1), new Vector2(-1, -2)   },  //0 >> 1.
        { new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, 1), new Vector2(0, 2), new Vector2(1, 1), new Vector2(1, 2)          },  //1 >> 2.
        { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, -1), new Vector2(0, -2), new Vector2(1, -1), new Vector2(1, -2)       },  //2 >> 3.
        { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, -1), new Vector2(0, -2), new Vector2(-1, -1), new Vector2(-1, -2)     },  //3 >> 0.
    };

    private readonly Vector2[,] WALLKICKS_CUATRO_ROTS_DCH = new Vector2[4, 6] {
        { new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, 1), new Vector2(0, 2), new Vector2(1, 1), new Vector2(1, 2)        },  //0 >> 3.
        { new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, 1), new Vector2(0, 2), new Vector2(1, 1), new Vector2(1, 2)          },  //1 >> 0.
        { new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, -1), new Vector2(0, -2), new Vector2(-1, -1), new Vector2(-1, -2)   },  //2 >> 1.
        { new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, 1), new Vector2(0, 2), new Vector2(1, 1), new Vector2(1, 2)        },  //3 >> 2.
    };

    private readonly Vector2[] WALLKICKS_DOS_ROTS_IZQ = new Vector2[7] { // 0 >> 1. 
        new Vector2(1, 0),
        new Vector2(-1, 0), 
        new Vector2(-1, 1), 
        new Vector2(0, -1), 
        new Vector2(0, -2), 
        new Vector2(-1, -1), 
        new Vector2(-1, -2)
    };

    private readonly Vector2[] WALLKICKS_DOS_ROTS_DCH = new Vector2[7] { // 1 >> 0. 
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(1, -1),
        new Vector2(0, 1),
        new Vector2(0, 2),
        new Vector2(1, 1),
        new Vector2(1, 2)
    };
   
    private readonly Vector2[] WALLKICKS_TETRIMINO_I_IZQ = new Vector2[14] { // 0 >> 1. 
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(2, 0),
        new Vector2(-2, 0),
        new Vector2(-1, -1),
        new Vector2(-2, -1),
        new Vector2(1, 1),
        new Vector2(1, 2),
        new Vector2(1, -1),
        new Vector2(1, -2),
        new Vector2(2, -1),
        new Vector2(2, -2),        
        new Vector2(-1, -2),        
        new Vector2(-2, -2)
    };

    private readonly Vector2[] WALLKICKS_TETRIMINO_I_DCH = new Vector2[14] { // 1 >> 0.
        new Vector2(-1, 0),
        new Vector2(1, 0),
        new Vector2(-2, 0),
        new Vector2(2, 0),
        new Vector2(1, 1),
        new Vector2(2, 1),
        new Vector2(-1, -1),
        new Vector2(-1, -2),
        new Vector2(1, -1),
        new Vector2(1, -2),
        new Vector2(2, -1),
        new Vector2(2, -2),
        new Vector2(-1, -2),
        new Vector2(-2, -2)
    };

    /// <summary>
    /// Gestiona las posiciones que el tetrimino tiene comprobar para su rotacion.
    /// </summary>
    /// <param name="rotacionOriginal">Rotación anterior del tetrimino</param>
    /// <param name="rotacionActual">Rotación en la que se encuentra el tetrimino</param>
    /// <param name="tetrimino">Tipo de tetrimino que esta rotando</param>
    /// <returns>Array de posiciones para que el tetrimino compruebe su rotación.</returns>
    public Vector2[] ObtenerPosiciones(int rotacionOriginal, int rotacionActual, TetriminoScr.TiposTetriminos tetrimino) {        
        Vector2[,] wallKicks = null;
        Vector2[] posiciones = null;
        int test = 0;

        switch (tetrimino) {
            case TetriminoScr.TiposTetriminos.I:
                if (rotacionOriginal < rotacionActual) posiciones = WALLKICKS_TETRIMINO_I_DCH;
                else posiciones = WALLKICKS_TETRIMINO_I_IZQ;
            break;

            case TetriminoScr.TiposTetriminos.Z:
            case TetriminoScr.TiposTetriminos.S:
                if (rotacionOriginal < rotacionActual) posiciones = WALLKICKS_DOS_ROTS_DCH;
                else posiciones = WALLKICKS_DOS_ROTS_IZQ;
            break;

            case TetriminoScr.TiposTetriminos.T:
            case TetriminoScr.TiposTetriminos.L:
            case TetriminoScr.TiposTetriminos.J:
                if (rotacionOriginal < rotacionActual) {
                    wallKicks = WALLKICKS_CUATRO_ROTS_DCH;
                    test = rotacionOriginal;
                } else {
                    wallKicks = WALLKICKS_CUATRO_ROTS_IZQ;
                    test = rotacionActual;
                }
            break;
        }

        if (wallKicks != null) {
            posiciones = new Vector2[wallKicks.GetLength(1)];
            for (int i = 0; i < posiciones.GetLength(0); i++) posiciones[i] = wallKicks[test, i];
        }

        return posiciones;
    }
}