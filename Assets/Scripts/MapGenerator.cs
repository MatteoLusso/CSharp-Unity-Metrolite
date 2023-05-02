using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum LineDir{
        North,
        South,
        East,
        West,
        Random,
    }
    public enum CellSide{
        Up,
        Right,
        Down,
        Left,
    }

    public enum EndedBy{
        OutOfBoundsLeft,
        OutOfBoundsRight,
        OutOfBoundsUp,
        OutOfBoundsDown,
        OtherLine,
        Completed,
    }

    public float cellSize = 300.0f; 
    public int mapSize = 50;
    public float mainDirPercent = 0.7f; 
    public int linesNumber = 5;
    public int noGenNearBorders = 10;
    public int noGenNearLines = 2;
    public int lineLength = 25;
    public Cell[ , ] map;
    public Dictionary<string, Cell> lines = new Dictionary<string, Cell>();

    void Start()
    {
        StartGeneration();
    }

    public bool StartGeneration() {
        Debug.ClearDeveloperConsole();

        InitializeMap();

        for( int i = 0; i < linesNumber; i++ ) {
            GenerateLine( CalculateNextLineDir(), null );
        }

        return true;
    }

    private LineDir CalculateNextLineDir() {

        int proibitedCellUp = 0, proibitedCellDown = 0, proibitedCellLeft = 0, proibitedCellRight = 0;

        for( int i = 0; i < mapSize; i++ ) {
            if( map[ 0, i ].content == Cell.Content.Proibited ) {
                proibitedCellLeft++;
            }

            if( map[ i, 0 ].content == Cell.Content.Proibited ) {
                proibitedCellDown++;
            }

            if( map[ mapSize - 1, i ].content == Cell.Content.Proibited ) {
                proibitedCellRight++;
            }

            if( map[ i, mapSize - 1 ].content == Cell.Content.Proibited ) {
                proibitedCellUp++;
            }
        }

        Dictionary<int, LineDir> availableDir = new Dictionary<int, LineDir>();
        int min = Mathf.Min( new int[ 4 ]{ proibitedCellUp, proibitedCellDown, proibitedCellLeft, proibitedCellRight } );
        int k = 0;

        //LineDir dir = LineDir.Random;

        if( proibitedCellUp == min ) {
            availableDir.Add( k, LineDir.South );
            k++;

            //dir = LineDir.South;
        }
        if( proibitedCellLeft == min ) {
            availableDir.Add( k, LineDir.East );
            k++;

            //dir = LineDir.East;
        }
        if( proibitedCellDown == min ) {
            availableDir.Add( k, LineDir.North );
            k++;

            //dir = LineDir.North;
        }
        if( proibitedCellRight == min ) {
            availableDir.Add( k, LineDir.West );
            k++;

            //dir = LineDir.West;
        } 

        return availableDir[ Random.Range( 0, k ) ];
        //return dir;
    } 

    private void InitializeMap() {
        Cell[ , ] matrix = new Cell[ mapSize, mapSize ];
            
        // Inizializzazione celle vuote
        for( int i = 0; i < mapSize; i++ ) {
            for( int j = 0; j < mapSize; j++ ) {
                Cell cell = new Cell();

                cell.content = Cell.Content.Empty;
                if( i == 0 || i == mapSize - 1 ) {
                    if( j < noGenNearBorders || j > mapSize - 1 - noGenNearBorders ) {
                        cell.content = Cell.Content.Proibited;
                    }
                }
                if( j == 0 || j == mapSize - 1) {
                    if( i < noGenNearBorders || i > mapSize - 1 - noGenNearBorders ) {
                        cell.content = Cell.Content.Proibited;
                    }
                }

                cell.coords = new Vector2( i, j );
                cell.spatialCoords = new Vector3( ( i * cellSize ) + ( cellSize / 2 ), ( j * cellSize ) + ( cellSize / 2 ), 0.0f );

                matrix[ i, j ] = cell;
            }    
        }

        this.map = matrix;
    }

    private void GenerateLine( LineDir lineDir, Vector2? startDir ) {

        EndedBy endedBy = EndedBy.Completed;
        
        if( lineDir == LineDir.Random ) {
            lineDir = ( LineDir )Random.Range( 0, 4 );
        }
        Debug.Log( "GenerateMap >>> lineDir: " + lineDir );
        Vector2Int actualCoords = Vector2Int.zero;
        Vector2Int startingCoords = Vector2Int.zero;
        
        int genStartingPointCounter = 0;
    GenStartingPoint:
        if( genStartingPointCounter > 10 ) {
            return;
        }

        if( startDir == null ) {
            // In questo caso inizio la generazione dai bordi della mappa, in base alla lineDir
            HashSet<Cell> proibitedStarts = new HashSet<Cell>();
            switch( lineDir ) {
                    case LineDir.East:  actualCoords = new Vector2Int( 0, Random.Range( noGenNearBorders, mapSize - noGenNearBorders ) );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.y + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y + i ] );
                                            }
                                            if( actualCoords.y - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y - i ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x + 1, actualCoords.y );
                                        break;

                    case LineDir.North: actualCoords = new Vector2Int( Random.Range( noGenNearBorders, mapSize - noGenNearBorders ), 0 );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.x + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x + i, actualCoords.y ] );
                                            }
                                            if( actualCoords.x - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x - i, actualCoords.y ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x, actualCoords.y + 1 );
                                        break;

                    case LineDir.South: actualCoords = new Vector2Int( Random.Range( noGenNearBorders, mapSize - noGenNearBorders ), mapSize - 1 );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.x + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x + i, actualCoords.y ] );
                                            }
                                            if( actualCoords.x - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x - i, actualCoords.y ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x, actualCoords.y - 1 );
                                        break;

                    case LineDir.West:  actualCoords = new Vector2Int( mapSize - 1, Random.Range( noGenNearBorders, mapSize - noGenNearBorders ) );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.y + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y + i ] );
                                            }
                                            if( actualCoords.y - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y - i ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x - 1, actualCoords.y );
                                        break;
            }
            if( this.map[ actualCoords.x, actualCoords.y ].content != Cell.Content.Empty || this.map[ actualCoords.x, actualCoords.y ].content == Cell.Content.Proibited ) {
                genStartingPointCounter++;
                goto GenStartingPoint;
            }
            foreach( Cell proibitedStart in proibitedStarts ) {
                if( proibitedStart.content == Cell.Content.Empty ) {
                    proibitedStart.content = Cell.Content.Proibited;
                }
            }
        }

        this.map[ startingCoords.x, startingCoords.y ].content = Cell.Content.Tunnel;
        actualCoords = startingCoords;

        int cellsCounter = 1;
        bool stopGeneration = false;
        HashSet<CellSide> proibitedSides = new HashSet<CellSide>();

        while( cellsCounter < lineLength - 1 && !stopGeneration ) {


            float randomSide = Random.Range( 0.0f, 1.0f );
            bool ignoreCell = false;
            Vector2Int nextCoords = Vector2Int.zero;
            
            switch( lineDir ) {
                case LineDir.East:  if( randomSide <= ( 1.0f - mainDirPercent ) / 2 ) {
                                        Debug.Log( "GenerateMap >>> MOVE UP" );

                                        ignoreCell = true;
                                        if( actualCoords.y + 2 >= mapSize ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsUp;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Up ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x, actualCoords.y + 1 );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Down );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Up );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > ( 1.0f - mainDirPercent ) / 2 && randomSide <= 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE RIGHT" );

                                        ignoreCell = true;
                                        if( actualCoords.x + 2 >= mapSize ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsRight;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Right ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x + 1, actualCoords.y );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Left );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Right );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE DOWN" );

                                        ignoreCell = true;
                                        if( actualCoords.y - 2 < 0 ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsDown;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Down ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x, actualCoords.y - 1 );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Up );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Down );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }

                                    break;

                case LineDir.North: if( randomSide <= ( 1.0f - mainDirPercent ) / 2 ) {
                                        Debug.Log( "GenerateMap >>> MOVE LEFT" );

                                        ignoreCell = true;
                                        if( actualCoords.x - 2 < 0 ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsLeft;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Left ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x - 1, actualCoords.y );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Right );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Left );

                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > ( 1.0f - mainDirPercent ) / 2 && randomSide <= 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE UP" );

                                        ignoreCell = true;
                                        if( actualCoords.y + 2 >= mapSize ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsUp;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Up ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x, actualCoords.y + 1 );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Down );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Up );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE RIGHT" );

                                        ignoreCell = true;
                                        if( actualCoords.x + 2 >= mapSize ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsRight;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Right ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x + 1, actualCoords.y );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Left );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Right );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }

                                    break;

                case LineDir.South: if( randomSide <= ( 1.0f - mainDirPercent ) / 2 ) {
                                        Debug.Log( "GenerateMap >>> MOVE LEFT" );

                                        ignoreCell = true;
                                        if( actualCoords.x - 2 < 0 ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsLeft;
                                        }
                                        else {
                                            ignoreCell = true;
                                            if( !proibitedSides.Contains( CellSide.Left ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x - 1, actualCoords.y );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Right );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Left );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > ( 1.0f - mainDirPercent ) / 2 && randomSide <= 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE DOWN" );

                                        ignoreCell = true;
                                        if( actualCoords.y - 2 < 0 ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsDown;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Down ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x, actualCoords.y - 1 );

                                            if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {    
                                                proibitedSides = new HashSet<CellSide>();
                                                proibitedSides.Add( CellSide.Up );

                                                ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Down );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE RIGHT" );

                                        ignoreCell = true;
                                        if( actualCoords.x + 2 >= mapSize ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsRight;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Right ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x + 1, actualCoords.y );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Left );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Right );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }

                                    break;

                case LineDir.West:  if( randomSide <= ( 1.0f - mainDirPercent ) / 2 ) {
                                        Debug.Log( "GenerateMap >>> MOVE UP" );

                                        ignoreCell = true;
                                        if( actualCoords.y + 2 >= mapSize ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsUp;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Up ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x, actualCoords.y + 1 );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Down );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Up );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > ( 1.0f - mainDirPercent ) / 2 && randomSide <= 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE LEFT" );

                                        ignoreCell = true;
                                        if( actualCoords.x - 2 < 0 ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsLeft;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Left ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x - 1, actualCoords.y );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Right );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Left );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }
                                    else if( randomSide > 1.0f - ( ( 1.0f - mainDirPercent ) / 2 ) ) {
                                        Debug.Log( "GenerateMap >>> MOVE DOWN" );

                                        ignoreCell = true;
                                        if( actualCoords.y - 2 < 0 ) {
                                            Debug.Log( "GenerateMap >>> Fuori limiti" );
                                            stopGeneration = true;
                                            endedBy = EndedBy.OutOfBoundsDown;
                                        }
                                        else {
                                            if( !proibitedSides.Contains( CellSide.Down ) ) {
                                                nextCoords = new Vector2Int( actualCoords.x, actualCoords.y - 1 );

                                                if( this.map[ nextCoords.x, nextCoords.y ].content == Cell.Content.Empty ) {
                                                    proibitedSides = new HashSet<CellSide>();
                                                    proibitedSides.Add( CellSide.Up );

                                                    ignoreCell = false;
                                                }
                                                else {
                                                    //proibitedSides.Add( CellSide.Down );
                                                    this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.OutsideSwitch;

                                                    if( this.map[ nextCoords.x, nextCoords.y ].newLineCells == null ) {
                                                        this.map[ nextCoords.x, nextCoords.y ].newLineCells = new List<Cell>();
                                                    }
                                                    this.map[ nextCoords.x, nextCoords.y ].newLineCells.Add( this.map[ actualCoords.x, actualCoords.y ] );

                                                    stopGeneration = true;
                                                    endedBy = EndedBy.OtherLine;
                                                }
                                            }
                                        }
                                    }

                                    break;
            }

            if( !ignoreCell ) {
                //Debug.Log( "nextCoords: " + nextCoords );
                this.map[ nextCoords.x, nextCoords.y ].content = Cell.Content.Tunnel;
                this.map[ nextCoords.x, nextCoords.y ].previousCell = this.map[ actualCoords.x, actualCoords.y ];
                this.map[ actualCoords.x, actualCoords.y ].nextCell = this.map[ nextCoords.x, nextCoords.y ];
                actualCoords = nextCoords;
                cellsCounter++;
            }
            else {
                Debug.Log( "GenerateMap >>> Direzione ignorata" );
            }

            HashSet<Cell> proibitedEnds = new HashSet<Cell>();
            switch( endedBy ) {
                case EndedBy.OutOfBoundsRight:  for( int i = 0; i < noGenNearLines; i++ ) {
                                                    if( actualCoords.y + i < mapSize ) {        
                                                        proibitedEnds.Add( this.map[ actualCoords.x + 1, actualCoords.y + i ] );
                                                    }
                                                    if( actualCoords.y - i >= 0 ) {
                                                        proibitedEnds.Add( this.map[ actualCoords.x + 1, actualCoords.y - i ] );
                                                    }
                                                }
                                                break;

                case EndedBy.OutOfBoundsUp:     for( int i = 0; i < noGenNearLines; i++ ) {
                                                    if( actualCoords.x + i < mapSize ) {        
                                                        proibitedEnds.Add( this.map[ actualCoords.x + i, actualCoords.y + 1 ] );
                                                    }
                                                    if( actualCoords.x - i >= 0 ) {
                                                        proibitedEnds.Add( this.map[ actualCoords.x - i, actualCoords.y + 1 ] );
                                                    }
                                                }
                                                break;

                case EndedBy.OutOfBoundsDown:   for( int i = 0; i < noGenNearLines; i++ ) {
                                                    if( actualCoords.x + i < mapSize ) {        
                                                        proibitedEnds.Add( this.map[ actualCoords.x + i, actualCoords.y - 1 ] );
                                                    }
                                                    if( actualCoords.x - i >= 0 ) {
                                                        proibitedEnds.Add( this.map[ actualCoords.x - i, actualCoords.y - 1 ] );
                                                    }
                                                }
                                                break;

                case EndedBy.OutOfBoundsLeft:   for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.y + i < mapSize ) {        
                                                proibitedEnds.Add( this.map[ actualCoords.x - 1, actualCoords.y + i ] );
                                            }
                                            if( actualCoords.y - i >= 0 ) {
                                                proibitedEnds.Add( this.map[ actualCoords.x - 1, actualCoords.y - i ] );
                                            }
                                        }
                                        break;
            }

            foreach( Cell proibitedEnd in proibitedEnds ) {
                if( proibitedEnd.content == Cell.Content.Empty ) {
                    proibitedEnd.content = Cell.Content.Proibited;
                }
            }
        }

    }

    private void OnDrawGizmos()
    { 
        if( map != null ) {

            for( int i = 0; i < mapSize; i++ ) {
                for( int j = 0; j < mapSize; j++ ) {
                    Cell cell = this.map[ i, j ];
                    Vector3 center = new Vector3( cell.spatialCoords.x, cell.spatialCoords.y, 0.0f );
                    Vector3 a, b, c, d;
                    a = center + ( Vector3.up * cellSize / 2 ) - ( Vector3.right * cellSize / 2 );
                    b = a + ( Vector3.right * cellSize );
                    c = b - ( Vector3.up * cellSize );
                    d = c - ( Vector3.right * cellSize );

                    Gizmos.color = Color.grey;
                    Gizmos.DrawLine( a, b );
                    Gizmos.DrawLine( b, c );
                    Gizmos.DrawLine( c, d );
                    Gizmos.DrawLine( d, a );

                    if( cell.previousCell != null ) {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine( center, new Vector3( cell.previousCell.spatialCoords.x, cell.previousCell.spatialCoords.y, 0.0f ) );
                    }

                    if( cell.content == Cell.Content.OutsideSwitch ) {
                        Gizmos.color = Color.blue;
                        foreach( Cell newLineCell in cell.newLineCells ) {
                            Gizmos.DrawLine( center, new Vector3( newLineCell.spatialCoords.x, newLineCell.spatialCoords.y, 0.0f ) );
                        }
                    }

                    if( cell.content == Cell.Content.Proibited ) {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine( a, c );
                        Gizmos.DrawLine( b, d );
                    }
                }    
            }
        }
    }
}
