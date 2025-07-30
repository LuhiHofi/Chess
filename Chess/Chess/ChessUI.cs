namespace Chess
{
    /**
     * @brief Enum describing the state of the game
     */
    public enum StateOfGame
    {
        Game,
        Check,
        PromotionPending,
        Draw,
        WhiteWins,
        BlackWins
    }

    /**
     * @brief Struct describing a position on the board
     */
    public struct Position
    {
        public Position(int x = 0, int y = 0)
        {
            this.x = x; this.y = y;
        }
        public int x; public int y;
    }

    /**
     * @brief Struct describing a move
     * Saves 2 positions - from and to which position the piece moves
     */
    public struct Move
    {
        public Move(Position from, Position to)
        {
            this.from = from;
            this.to = to;
        }
        public Position from;
        public Position to;
    }

    /**
     * @brief Class describing a cell on the board
     */
    public class Cell
    {
        public Piece? piece;
        public List<Piece> attackedByWhite = new List<Piece>();
        public List<Piece> attackedByBlack = new List<Piece>();
        public Cell(Piece? piece)
        {
            this.piece = piece;
        }
    }

    /**
     * @brief Class describing the game
     */
    public class ChessUI
    {
        internal List<Piece> whitePieces = new List<Piece>(); // list of all white pieces
        internal List<Piece> blackPieces = new List<Piece>(); // list of all black pieces
        
        internal List<Move> possibleMoves = new List<Move>(); // list of all possible moves at this time (helpfull for MinimaxActive and GUI)
        internal List<Position> stopKingCheck = new List<Position>();

        internal Cell[,] board = new Cell[8, 8]; // x is first, y second
        internal Piece[] kings = new Piece[2]; // 0 index is a cell of white king, 1 index is a cell of black king

        internal bool[] CastlingRights { get; set; } = { true, true, true, true }; // W queen, W king, B queen, B king

        internal Position EnPassant { get; set; } = new Position { x = -1, y = -1 }; // indexed from 0 
        internal StateOfGame State { get; set; } = StateOfGame.Game;
        internal int HalfMoves { get; set; } = 0;
        internal int FullMoves { get; set; } = 1;
        internal bool WhiteToMove { get; set; } = true; // current player is white or not

        internal bool MinimaxActive { get; set; } // true if the UI is for MinimaxActive, false otherwise


        /**
         * @brief Creates a brand new game
         */
        public ChessUI(bool MinimaxActive = false)
        {
            string startGame = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            this.board = CreateBoardFromFen(startGame);
            this.FillPieceAttacks();
            this.MinimaxActive = MinimaxActive;
        }
        
        /**
         * @brief Creates a new game from a file
         * @param sr StreamReader to read from
         * DOES NOT CHECK IF FEN IS CORRECT
         */
        public ChessUI(StreamReader sr, bool MinimaxActive = false)
        {
            string? line = sr.ReadLine();
            if (line is null)
                line = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            this.board = this.CreateBoardFromFen(line);
            string[] splitFen = line.Split(' ');
            if (splitFen[1][0] == 'w') this.WhiteToMove = true; // only 1 char should be there, that's why there is the [0] - to convert to char
            else this.WhiteToMove = false;

            for (int i = 0; i < CastlingRights.Length; ++i)
            {
                this.CastlingRights[i] = false;
            }
            for (int i = 0; i < splitFen[2].Length; ++i)
            {
                switch (splitFen[2][i])
                {
                    case 'K':
                        this.CastlingRights[0] = true;
                        break;
                    case 'Q':
                        this.CastlingRights[1] = true;
                        break;
                    case 'k':
                        this.CastlingRights[2] = true;
                        break;
                    case 'q':
                        this.CastlingRights[3] = true;
                        break;
                }
            }
            if (splitFen[3].Length == 2)
            {
                this.EnPassant = new Position { x = splitFen[3][0] - 'a', y = splitFen[3][1] - '1' };
            }
            else
            {
                this.EnPassant = new Position { x = -1, y = -1 };
            }
            this.HalfMoves = splitFen[4][0] - '0';
            this.FullMoves = splitFen[5][0] - '0';
            this.FillPieceAttacks();
            this.MinimaxActive = MinimaxActive;
        }

        /**
         * @brief Creates a new game from a file
         * @param FEN FEN to build the game from
         * DOES NOT CHECK IF FEN IS CORRECT
         */
        public ChessUI(string FEN, bool MinimaxActive = false)
        {
            this.board = this.CreateBoardFromFen(FEN);
            string[] splitFen = FEN.Split(' ');
            if (splitFen[1][0] == 'w') this.WhiteToMove = true; // only 1 char should be there, that's why there is the [0] - to convert to char
            else this.WhiteToMove = false;
            for (int i = 0; i < CastlingRights.Length; ++i)
            {
                this.CastlingRights[i] = false;
            }
            for (int i = 0; i < splitFen[2].Length; ++i)
            {
                switch (splitFen[2][i])
                {
                    case 'K':
                        this.CastlingRights[0] = true;
                        break;
                    case 'Q':
                        this.CastlingRights[1] = true;
                        break;
                    case 'k':
                        this.CastlingRights[2] = true;
                        break;
                    case 'q':
                        this.CastlingRights[3] = true;
                        break;
                }
            }
            if (splitFen[3].Length == 2)
            {
                this.EnPassant = new Position { x = splitFen[3][0] - 'a', y = splitFen[3][1] - '1' };
            }
            else
            {
                this.EnPassant = new Position { x = -1, y = -1 };
            }
            this.HalfMoves = splitFen[4][0] - '0';
            this.FullMoves = splitFen[5][0] - '0';
            this.FillPieceAttacks();
            this.MinimaxActive = MinimaxActive;
        }

        /**
         * @brief Creates a new game from a file
         * @param ui creates a deep copy from ui to build a new identical game
         */
        public ChessUI(ChessUI ui, bool MinimaxActive = true)
        {
            this.DeepCopyBoardToThisGame(ui.board);

            for (int i = 0; i < CastlingRights.Length; ++i)
            {
                this.CastlingRights[i] = ui.CastlingRights[i];
            }

            this.EnPassant = ui.EnPassant;
            this.State = ui.State;

            this.HalfMoves = ui.HalfMoves;
            this.FullMoves = ui.FullMoves;

            this.WhiteToMove = ui.WhiteToMove;
            this.MinimaxActive = MinimaxActive;

            this.stopKingCheck = new List<Position>(ui.stopKingCheck);
            this.possibleMoves = new List<Move>(ui.possibleMoves);

            this.UpdateMovesOfAllPieces();
        }

        /**
         * @brief Saves the game to a file
         * @param fileName Name of the file to save to
         */
        public void SaveGame(string fileName)
        {
            using (StreamWriter saveFile = new StreamWriter("SaveGames/" + fileName))
            saveFile.WriteLine(GetFEN());
        }
        
        /**
         * @brief Saves the game to a file
         * @param file StreamWriter to save to
         */
        public void SaveGame(StreamWriter file)
        {
            file.WriteLine(GetFEN());
        }
        
        /**
         * @brief Loads the game from a file
         * @param fileName Name of the file to load from
         * @return ChessUI object with the loaded game
         */
        public static ChessUI LoadGame(string fileName)
        {
            var saveGamesDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"../../../SaveGames"));
            if (!File.Exists(saveGamesDirectory + "\\\\" + fileName)) return new ChessUI();
            StreamReader sr = File.OpenText(saveGamesDirectory + "\\\\" + fileName);
            ChessUI loadedGame = new ChessUI(sr);
            return loadedGame;
        }
        
        private void UpdateMovesOfAllPieces()
        {
            foreach (Piece piece in blackPieces)
            {
                if (piece is not King)
                    piece.UpdatePosibleMoves();
            }
            foreach (Piece piece in whitePieces)
            {
                if (piece is not King)
                    piece.UpdatePosibleMoves();
            }
            // i need to update kings last, because for their moves i need to know which cells are attacked by the opponent
            foreach (King king in kings)
            {
                king.UpdatePosibleMoves();
            }
        }

        /**
         * @brief Fills the attackedByWhite and attackedByBlack lists in each cell
         * and the pieceBlocking list in each piece
         */
        private void FillPieceAttacks()
        {
            UpdateMovesOfAllPieces();
            // to set the state of the game
            Check();
            // to fill possibleMoves
            GameEnd();
        }

        /**
         * @brief Checks if the game is in check, 
         * and if it is, it sets the state of the game to Check
         */
        internal void Check()
        {
            this.stopKingCheck.Clear();
            bool check = false;
            if (WhiteToMove)
            {
                var attackers = board[kings[0].position.x, kings[0].position.y].attackedByBlack;
                check = (attackers.Count > 0);
                if (check)
                {
                    attackers[0].CellsStoppingCheck(this.stopKingCheck);
                    if (attackers.Count > 1)
                    {
                        for (int i = 1; i < attackers.Count; ++i)
                        {
                            List<Position> stoppingCheck = new List<Position>();
                            attackers[i].CellsStoppingCheck(stoppingCheck);
                            stopKingCheck.Intersect(stoppingCheck);
                        }
                    }
                }
            }
            else
            {
                var attackers = board[kings[1].position.x, kings[1].position.y].attackedByWhite;
                check = (attackers.Count > 0);
                if (check)
                {
                    attackers[0].CellsStoppingCheck(stopKingCheck);
                    if (attackers.Count > 1)
                    {
                        for (int i = 1; i < attackers.Count; ++i)
                        {
                            List<Position> stoppingCheck = new List<Position>();
                            attackers[i].CellsStoppingCheck(stoppingCheck);
                            stopKingCheck.Intersect(stoppingCheck);
                        }
                    }
                }
            }
            if (check)
                State = StateOfGame.Check;
            else State = StateOfGame.Game;
        }

        /**
         * @brief Checks if there are enough piece to win for either of the players, if there aren't, returns true
         */
        private bool NotEnoughPieces()
        {
            bool white = this.whitePieces.Count == 1;

            if (this.whitePieces.Count == 2)
            {
                if (this.whitePieces[0] is Bishop or King && this.whitePieces[1] is Bishop or King) white = true;
            }
            else if (this.whitePieces.Count == 3)
            {
                if (this.whitePieces[0] is Knight or King && this.whitePieces[1] is Knight or King && this.whitePieces[2] is Knight or King) white = true;
            }

            bool black = this.blackPieces.Count == 1;

            if (this.blackPieces.Count == 2)
            {
                if (this.blackPieces[0] is Bishop or King && this.blackPieces[1] is Bishop or King) black = true;
            }
            else if (this.blackPieces.Count == 3)
            {
                if (this.blackPieces[0] is Knight or King && this.blackPieces[1] is Knight or King && this.blackPieces[2] is Knight or King) black = true;
            }
            return white && black;
        }

        /**
         * @brief Checks if the game is over
         * Fills the possibleMoves list with all possible moves
         * If there are no available moves, it sets the state of the game to Draw, WhiteWins or BlackWins
         * depending on the player to move and on the state of the game
         */
        internal void GameEnd()
        {
            this.possibleMoves.Clear();
            bool gameEnd = true;
            if (this.WhiteToMove)
            {
                foreach (Piece piece in this.whitePieces)
                {
                    foreach (Position cell in piece.possibleMoves)
                    {
                        if (piece.CheckMove(cell))
                        {
                            gameEnd = false;
                            this.possibleMoves.Add(new Move(piece.position, cell));
                        }
                    }
                }
            }
            else
            {
                foreach (Piece piece in this.blackPieces)
                {
                    foreach (Position cell in piece.possibleMoves)
                    {
                        if (piece.CheckMove(cell))
                        {
                            gameEnd = false;
                            this.possibleMoves.Add(new Move(piece.position, cell));
                        }
                    }
                }
            }
            if (gameEnd)
            {
                if (this.State == StateOfGame.Check)
                {
                    if (this.WhiteToMove) this.State = StateOfGame.BlackWins;
                    else this.State = StateOfGame.WhiteWins;
                }
                else this.State = StateOfGame.Draw;
            }
            else if (this.NotEnoughPieces())
            {
                this.State = StateOfGame.Draw;
            }
        }

        /**
         * @brief Moves a piece from one position to another
         * @param from string representation of Position to move from
         * @param to string representation of Position to move to
         * @return True if the move was successful, false otherwise
         */
        public bool Move(string from, string to)
        {
            if (from.Length < 2 || to.Length < 2)
            {
                Console.WriteLine("Input not long enough!");
                return false;
            }
            else if ((from[0] - 'a' > 7 || from[0] - 'a' < 0)
                || (to[0] - 'a' > 7 || to[0] - 'a' < 0)
                || (from[1] - '1' > 7 || from[1] - '1' < 0)
                || (to[1] - '1' > 7 || to[1] - '1' < 0))
            {
                Console.WriteLine("Wrong coordinates");
                return false;
            } 
            else
                return Move(new Position(from[0] - 'a', from[1] - '1'), new Position(to[0] - 'a', to[1] - '1'));
        }

        /**
         * @brief Moves a piece from one position to another
         * @param from Position to move from
         * @param to Position to move to
         * Positions are correct, no need to check
         * @return True if the move was successful, false otherwise
         */
        public bool Move(Position from, Position to)
        {
            if (board[from.x, from.y].piece is not null)
            {
                #pragma warning disable
                Piece piece = board[from.x, from.y].piece;
                if (!piece.Move(to))
                {
                    Console.WriteLine("This is not a legal move");
                    return false;
                }
                else
                {
                    if (this.State == StateOfGame.Game || this.State == StateOfGame.Check)
                    {
                        Check();
                        GameEnd();
                    }
                    return true;
                }
                #pragma warning restore
            }
            else Console.WriteLine("No piece on this coordinate");
            return false;
        }

        /**
         * @brief Moves a piece from one position to another
         * @param move Move from where to where
         * Positions are correct, no need to check
         * @return True if the move was successful, false otherwise
         */
        public bool Move(Move move)
        {
            if (board[move.from.x, move.from.y].piece is not null)
            {
                // Warnings disabled cause there is no way board.piece is null - asking in the if statement above
                #pragma warning disable
                Piece piece = board[move.from.x, move.from.y].piece;
                if (!piece.Move(move.to))
                {
                    Console.WriteLine("This is not a legal move");
                    return false;
                }
                else
                {
                    if (this.State == StateOfGame.Game || this.State == StateOfGame.Check)
                    {
                        Check();
                        GameEnd();
                    }
                    return true;
                }
                #pragma warning restore
            }
            else Console.WriteLine("No piece on this coordinate");
            return false;
        }

        private void DeepCopyBoardToThisGame(Cell[,] board)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    #pragma warning disable
                    switch (board[i, j].piece)
                    {
                        case Pawn:
                            this.board[i, j] = new Cell(new Pawn(new Position { x = i, y = j }, board[i, j].piece.white, this));
                            if (this.board[i, j].piece.white) this.whitePieces.Add(this.board[i, j].piece);
                            else this.blackPieces.Add(this.board[i, j].piece);
                            break;
                        case King:
                            this.board[i, j] = new Cell(new King(new Position { x = i, y = j }, board[i, j].piece.white, this));
                            if (this.board[i, j].piece.white)
                            {
                                this.kings[0] = this.board[i, j].piece;
                                this.whitePieces.Add(this.board[i, j].piece);
                            } 
                            else
                            {
                                this.kings[1] = this.board[i, j].piece;
                                this.blackPieces.Add(this.board[i, j].piece);
                            } 

                            break;
                        case Queen:
                            this.board[i, j] = new Cell(new Queen(new Position { x = i, y = j }, board[i, j].piece.white, this));
                            if (this.board[i, j].piece.white) this.whitePieces.Add(this.board[i, j].piece);
                            else this.blackPieces.Add(this.board[i, j].piece);
                            break;
                        case Bishop:
                            this.board[i, j] = new Cell(new Bishop(new Position { x = i, y = j }, board[i, j].piece.white, this));
                            if (this.board[i, j].piece.white) this.whitePieces.Add(this.board[i, j].piece);
                            else this.blackPieces.Add(this.board[i, j].piece);
                            break;
                        case Knight:
                            this.board[i, j] = new Cell(new Knight(new Position { x = i, y = j }, board[i, j].piece.white, this));
                            if (this.board[i, j].piece.white) this.whitePieces.Add(this.board[i, j].piece);
                            else this.blackPieces.Add(this.board[i, j].piece);
                            break;
                        case Rook:
                            this.board[i, j] = new Cell(new Rook(new Position { x = i, y = j }, board[i, j].piece.white, this));
                            if (this.board[i, j].piece.white) this.whitePieces.Add(this.board[i, j].piece);
                            else this.blackPieces.Add(this.board[i, j].piece);
                            break;
                        default:
                            this.board[i, j] = new Cell(null);
                                break;
                    }
                    #pragma warning restore
                }
            }
        }

        /**
         * @brief Creates a board from a fen string
         * @param fen string representation of the board
         * @return Cell[,] representing the board
         */
        private Cell[,] CreateBoardFromFen(string fen)
        {
            Cell[,] board = new Cell[8, 8];
            string[] lines = fen.Split('/');
            lines[^1] = lines[^1].Split()[0];
            for (int i = 0; i < 8; i++)
            {
                int j = 0;
                foreach (char c in lines[i])
                {
                    if (char.IsDigit(c))
                    {
                        for (int z = j; z < j + c - '0'; ++z)
                        {
                            board[z, 7 - i] = new Cell(null);
                        }
                        j += c - '0';
                    }
                    else
                    {
                        #pragma warning disable
                        switch (c)
                        {
                            case 'p':
                                board[j, 7 - i] = new Cell(new Pawn(new Position { x = j, y = 7 - i }, false, this));
                                this.blackPieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'P':
                                board[j, 7 - i] = new Cell(new Pawn(new Position { x = j, y = 7 - i }, true, this));
                                this.whitePieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'k':
                                board[j, 7 - i] = new Cell(new King(new Position { x = j, y = 7 - i }, false, this));
                                this.blackPieces.Add(board[j, 7 - i].piece);
                                this.kings[1] = board[j, 7 - i].piece;
                                break;
                            case 'K':
                                board[j, 7 - i] = new Cell(new King(new Position { x = j, y = 7 - i }, true, this));
                                this.whitePieces.Add(board[j, 7 - i].piece);
                                this.kings[0] = board[j, 7 - i].piece;
                                break;
                            case 'q':
                                board[j, 7 - i] = new Cell(new Queen(new Position { x = j, y = 7 - i }, false, this));
                                this.blackPieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'Q':
                                board[j, 7 - i] = new Cell(new Queen(new Position { x = j, y = 7 - i }, true, this));
                                this.whitePieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'b':
                                board[j, 7 - i] = new Cell(new Bishop(new Position { x = j, y = 7 - i }, false, this));
                                this.blackPieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'B':
                                board[j, 7 - i] = new Cell(new Bishop(new Position { x = j, y = 7 - i }, true, this));
                                this.whitePieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'n':
                                board[j, 7 - i] = new Cell(new Knight(new Position { x = j, y = 7 - i }, false, this));
                                this.blackPieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'N':
                                board[j, 7 - i] = new Cell(new Knight(new Position { x = j, y = 7 - i }, true, this));
                                this.whitePieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'r':
                                board[j, 7 - i] = new Cell(new Rook(new Position { x = j, y = 7 - i }, false, this));
                                this.blackPieces.Add(board[j, 7 - i].piece);
                                break;
                            case 'R':
                                board[j, 7 - i] = new Cell(new Rook(new Position { x = j, y = 7 - i }, true, this));
                                this.whitePieces.Add(board[j, 7 - i].piece);
                                break;
                        }
                        #pragma warning restore
                        ++j;
                    }
                }
            }
            return board;
        }
        
        /**
         * @brief Gets the FEN representation of the board
         * @return string representation of the board
         */
        public string GetFEN()
        {
            string fen = "";
            // board
            for (int j = 7; j >= 0; --j)
            {
                int emptyCells = '0';
                for (int i = 0; i < 8; ++i)
                {
                    if (board[i, j].piece is null)
                    {
                        ++emptyCells;
                        continue;
                    }
                    else if (emptyCells > '0')
                    {
                        fen += (char)emptyCells;
                        emptyCells = '0';
                    }
                    #pragma warning disable
                    // Warnings disabled cause there is no way board.piece is null
                    switch (board[i, j].piece)
                    {
                        case Pawn:
                            if (board[i, j].piece.white) fen += 'P';
                            else fen += 'p';
                            break;
                        case King:
                            if (board[i, j].piece.white) fen += 'K';
                            else fen += 'k';
                            break;
                        case Queen:
                            if (board[i, j].piece.white) fen += 'Q';
                            else fen += 'q';
                            break;
                        case Bishop:
                            if (board[i, j].piece.white) fen += 'B';
                            else fen += 'b';
                            break;
                        case Knight:
                            if (board[i, j].piece.white) fen += 'N';
                            else fen += 'n';
                            break;
                        case Rook:
                            if (board[i, j].piece.white) fen += 'R';
                            else fen += 'r';
                            break;
                    }
                    #pragma warning restore
                }
                if (emptyCells > '0') fen += (char)emptyCells;
                if (j != 0) fen += '/';
            }
            // Active color
            fen += ' ';
            if (WhiteToMove) fen += "w ";
            else fen += "b ";
            // Castling rights
            if (CastlingRights[1]) fen += 'K';
            if (CastlingRights[0]) fen += 'Q';
            if (CastlingRights[3]) fen += 'k';
            if (CastlingRights[2]) fen += 'q';
            else if (!CastlingRights[0] && !CastlingRights[1] && !CastlingRights[2] && !CastlingRights[3]) fen += '-';
            fen += ' ';
            // en-passant
            if (EnPassant.x == -1) fen += "- ";
            else
            {
                int letter = 'a' + EnPassant.x;
                int number = '1' + EnPassant.y;
                fen += (char)letter; fen += (char)number; fen += ' ';
            }
            // half moves
            fen += HalfMoves.ToString(); fen += ' ';
            // full moves
            fen += FullMoves.ToString();
            return fen;
        }

        /**
         * @brief Prints the board to the console
         */
        public void PrintBoard()
        {
            for (int j = 7; j >= 0; --j)
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (board[i, j].piece is null)
                    {
                        Console.Write(" -");
                        continue;
                    }
                    #pragma warning disable
                    // Warnings disabled cause there is no way board.piece is null
                    switch (board[i, j].piece)
                    {
                        case Pawn:
                            if (board[i, j].piece.white) Console.Write(" P");
                            else Console.Write(" p");
                            break;
                        case King:
                            if (board[i, j].piece.white) Console.Write(" K");
                            else Console.Write(" k");
                            break;
                        case Queen:
                            if (board[i, j].piece.white) Console.Write(" Q");
                            else Console.Write(" q");
                            break;
                        case Bishop:
                            if (board[i, j].piece.white) Console.Write(" B");
                            else Console.Write(" b");
                            break;
                        case Knight:
                            if (board[i, j].piece.white) Console.Write(" N");
                            else Console.Write(" n");
                            break;
                        case Rook:
                            if (board[i, j].piece.white) Console.Write(" R");
                            else Console.Write(" r");
                            break;

                    }
                #pragma warning restore
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
