namespace Chess
{
    /**
     * @brief Enum for the type of piece that the pawn will be promoted to
     */
    public enum PromotePieceType
    {
        Queen,
        Rook,
        Bishop,
        Knight
    }

    /**
     * @brief Abstract class for the chess pieces
     */
    public abstract class Piece
    {
        internal ChessUI UI; // to which board game does the piece belong
        internal Position position { get; set; } // position of the piece on the board
        internal bool white { get; } // color of the piece
        
        internal List<Position> blocking_ = new List<Position>(); // Positions where a piece could move if something didn't prevent it
        internal List<Position> possibleMoves= new List<Position>(); // Positions where the piece can move to
        
        public static string ImagePath() => Path.Combine(Application.StartupPath, @"..\\Images\\");

        public abstract int Value { get; } // value of the piece
        
        public Piece(Position position, bool white, ChessUI UI)
        {
            this.position = position;
            this.white = white;
            this.UI = UI;
        }

        /**
         * @brief Returns the path to the image of the piece
         */
        public abstract string image();

        /**
         * @brief Updates the possible moves of the piece
         */
        public abstract void UpdatePosibleMoves();
        
        /**
         * @brief Checks if the move is valid and if it doesn't leave the king in check
         * @param where The position where the piece is going to move
         * @return True if the move is valid, false otherwise
         */
        internal bool MoveLeavesKingInCheck(Position where)
        {
            Position piecePosition = this.position;
            UI.board[position.x, position.y].piece = null;
            Piece takenPiece = UI.board[where.x, where.y].piece;
            UI.board[where.x, where.y].piece = this;
            this.position = where;

            if (takenPiece is not null) takenPiece.RemoveAll();
            var attackers = white ? UI.board[piecePosition.x, piecePosition.y].attackedByBlack 
                : UI.board[piecePosition.x, piecePosition.y].attackedByWhite;
            for (int i = attackers.Count - 1; i >= 0; --i)
            {
                attackers[i].UpdatePosibleMoves();
            }
            if (takenPiece is null)
            {
                var newLocationAttackers = white ? UI.board[where.x, where.y].attackedByBlack
                    : UI.board[where.x, where.y].attackedByWhite;
                for (int i = newLocationAttackers.Count - 1; i >= 0; --i)
                {
                    newLocationAttackers[i].UpdatePosibleMoves();
                }
            }
            var kingAttackers = white ? UI.board[UI.kings[0].position.x, UI.kings[0].position.y].attackedByBlack
                : UI.board[UI.kings[1].position.x, UI.kings[1].position.y].attackedByWhite;
            bool kingAttacked = kingAttackers.Count > 0;
            UI.board[piecePosition.x, piecePosition.y].piece = this;
            this.position = piecePosition;

            UI.board[where.x, where.y].piece = takenPiece;
            if (takenPiece is not null)
                takenPiece.UpdatePosibleMoves();
            else 
            { 
                var newLocationAttackers = white ? UI.board[where.x, where.y].attackedByBlack
                    : UI.board[where.x, where.y].attackedByWhite;
                for (int i = newLocationAttackers.Count - 1; i >= 0; --i)
                {
                    newLocationAttackers[i].UpdatePosibleMoves();
                }
            }
            for (int i = attackers.Count - 1; i >= 0; --i)
            {
                attackers[i].UpdatePosibleMoves();
            }
            return kingAttacked;
        }

        /**
         * @brief Checks if the move is valid
         * @param where The position where the piece is going to move
         * @return True if the move is valid, false otherwise
         */
        public bool CheckMove(Position where)
        {
            // piece has the color of the current player
            if (white != UI.WhiteToMove) return false;
            // is one of the possible moves of the selected piece
            if (!possibleMoves.Contains(where)) return false;
            // if the move leaves the king in check
            if (MoveLeavesKingInCheck(where)) return false;
            // if the king is in check and the piece is not the king or the piece is not blocking the check
            if (this is not King && (UI.State == StateOfGame.Check && !UI.stopKingCheck.Contains(where))) return false;
            return true;
        }

        /**
         * @brief Handles taking of a piece from the board
         * if a piece is on the position where the piece is going to move, it gets removed and and the game is updated accordingly
         * @param where The position of the piece that is going to be taken
         */
        internal void TakingOfAPiece(Position where)
        {
            if (UI.board[where.x, where.y].piece is not null)
            {
                UI.board[where.x, where.y].piece.RemoveAll();
                if (white)
                {
                    UI.blackPieces.Remove(UI.board[where.x, where.y].piece);
                    if (UI.board[where.x, where.y].piece is Rook)
                    {
                        if (where.x == 0 && where.y == 7) UI.CastlingRights[2] = false;
                        else if (where.x == 7 && where.y == 7) UI.CastlingRights[3] = false;
                    }
                }
                else
                {
                    UI.whitePieces.Remove(UI.board[where.x, where.y].piece);
                    // check if the piece is a rook and the proper castling is still possible - if so, update castling rights 
                    if (UI.board[where.x, where.y].piece is Rook)
                    {
                        if (where.x == 0 && where.y == 0) UI.CastlingRights[0] = false;
                        else if (where.x == 7 && where.y == 0) UI.CastlingRights[1] = false;
                    }
                }
            }
        }

        /**
         * @brief Moves the piece to the given position
         * @param where The position where the piece is going to move
         * @return True if the move was successful, false otherwise
         */
        public virtual bool Move(Position where)
        {
            if (!this.CheckMove(where)) return false; 

            // En-passant resets if not played right away
            UI.EnPassant = new Position { x = -1, y = -1 };

            TakingOfAPiece(where);

            UpdateMoveAndPosition(where);
            return true;
        }

        /**
         * @brief Fills the highlightedCells list with the Position of Cells that the piece is blocking
         */
        public abstract void CellsStoppingCheck(List<Position> highlightedCells);
        
        /**
         * @brief Clears all moves, attacks and blockings of the piece
         */
        public void RemoveAll()
        {
            if (white)
            {
                foreach (Position cell in blocking_)
                {
                    UI.board[cell.x, cell.y].attackedByWhite.Remove(this);
                }
                foreach (Position cell in possibleMoves)
                {
                    UI.board[cell.x, cell.y].attackedByWhite.Remove(this);
                }
            }
            else
            {
                foreach (Position cell in blocking_)
                {
                    UI.board[cell.x, cell.y].attackedByBlack.Remove(this);
                }
                foreach (Position cell in possibleMoves)
                {
                    UI.board[cell.x, cell.y].attackedByBlack.Remove(this);
                }
            }
            possibleMoves.Clear();
            blocking_.Clear();
        }

        /**
         * @brief Updates the Position of the board, the possible moves of affected pieces, switches the current player and updates the num of moves
         */
        public void UpdateMoveAndPosition(Position where)
        {
            UI.WhiteToMove = !UI.WhiteToMove;
            ++UI.HalfMoves;
            if (!white) ++UI.FullMoves;

            UI.board[position.x, position.y].piece = null;
            UpdatePawnMoves(); // update pawn moves now that the piece left
            UI.board[where.x, where.y].piece = this;

            UpdateMovesOfCellAttackers(UI.board[position.x, position.y]);

            position = where;
            
            UpdatePosibleMoves();

            UpdateMovesOfCellAttackers(UI.board[position.x, position.y]);
            
            UpdatePawnMoves(); // update the pawn moves now that a new piece has appeared in their way
            // so that they can't move to Cells that are attacked by an enemy piece

            foreach (King king in UI.kings)
            {
                king.UpdatePosibleMoves();
            }
        }

        /**
         * @brieg Updates the Moves of Pawns that were previously blocked
         */
        internal void UpdatePawnMoves()
        {
            if (position.y + 1 < 8)
            {
                if (UI.board[position.x, position.y + 1].piece is Pawn 
                    && !UI.board[position.x, position.y + 1].piece.white)
                    UI.board[position.x, position.y + 1].piece.UpdatePosibleMoves();
                if (position.y + 2 == 6
                    && UI.board[position.x, position.y + 2].piece is Pawn
                    && !UI.board[position.x, position.y + 2].piece.white)
                    UI.board[position.x, position.y + 2].piece.UpdatePosibleMoves();
            }

            if (position.y - 1 >= 0)
            {
                if (UI.board[position.x, position.y - 1].piece is Pawn 
                    && UI.board[position.x, position.y - 1].piece.white)
                    UI.board[position.x, position.y - 1].piece.UpdatePosibleMoves();
                if (position.y - 2 == 1
                    && UI.board[position.x, position.y - 2].piece is Pawn
                    && UI.board[position.x, position.y - 2].piece.white)
                    UI.board[position.x, position.y - 2].piece.UpdatePosibleMoves();
            }
        }

        /**
         * @brief Updates the moves of attackers of a Cell
         * @param cell the cell whose attackers moves shall be udpated
         */
        internal void UpdateMovesOfCellAttackers(Cell cell)
        {
            var copiedAttackers = cell.attackedByBlack.ToList<Piece>();
            foreach (Piece blackPiece in copiedAttackers)
            {
                blackPiece.UpdatePosibleMoves();
            }
            copiedAttackers = cell.attackedByWhite.ToList<Piece>();
            foreach (Piece whitePiece in copiedAttackers)
            {
                whitePiece.UpdatePosibleMoves();
            }
        }

        /**
         * @brief Adds the moves of a bishop to possible moves of the piece
         */
        internal void AddBishopMoves()
        {
            if (white)
            {
                // down left
                int move = 1;
                while (position.x - move >= 0 && position.y - move >= 0 && UI.board[position.x - move, position.y - move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x - move, position.y - move));
                    UI.board[position.x - move, position.y - move].attackedByWhite.Add(this);
                    ++move;
                }
                if (position.x - move >= 0 && position.y - move >= 0) // and not null
                {
                    if (UI.board[position.x - move, position.y - move].piece.white == white) blocking_.Add(new Position(position.x - move, position.y - move));
                    else possibleMoves.Add(new Position(position.x - move, position.y - move));
                    UI.board[position.x - move, position.y - move].attackedByWhite.Add(this);
                }                
                // top left
                move = 1;
                while (position.x - move >= 0 && position.y + move < 8 && UI.board[position.x - move, position.y + move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x - move, position.y + move));
                    UI.board[position.x - move, position.y + move].attackedByWhite.Add(this);
                    ++move;
                }
                if (position.x - move >= 0 && position.y + move < 8)
                {
                    if (UI.board[position.x - move, position.y + move].piece.white == white) blocking_.Add(new Position(position.x - move, position.y + move));
                    else possibleMoves.Add(new Position(position.x - move, position.y + move));
                    UI.board[position.x - move, position.y + move].attackedByWhite.Add(this);
                }
                // down right
                move = 1;
                while (position.x + move < 8 && position.y - move >= 0 && UI.board[position.x + move, position.y - move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x + move, position.y - move));
                    UI.board[position.x + move, position.y - move].attackedByWhite.Add(this);
                    ++move;
                }
                if (position.x + move < 8 && position.y - move >= 0)
                {
                    if (UI.board[position.x + move, position.y - move].piece.white == white) blocking_.Add(new Position(position.x + move, position.y - move));
                    else possibleMoves.Add(new Position(position.x + move, position.y - move));
                    UI.board[position.x + move, position.y - move].attackedByWhite.Add(this);
                }
                // top right
                move = 1;
                while (position.x + move < 8 && position.y + move < 8 && UI.board[position.x + move, position.y + move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x + move, position.y + move));
                    UI.board[position.x + move, position.y + move].attackedByWhite.Add(this);
                    ++move;
                }
                if (position.x + move < 8 && position.y + move < 8)
                {
                    if (UI.board[position.x + move, position.y + move].piece.white == white) blocking_.Add(new Position(position.x + move, position.y + move));
                    else possibleMoves.Add(new Position(position.x + move, position.y + move));
                    UI.board[position.x + move, position.y + move].attackedByWhite.Add(this);
                }
            }
            else
            {
                // down left
                int move = 1;
                while (position.x - move >= 0 && position.y - move >= 0 && UI.board[position.x - move, position.y - move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x - move, position.y - move));
                    UI.board[position.x - move, position.y - move].attackedByBlack.Add(this);
                    ++move;
                }
                if (position.x - move >= 0 && position.y - move >= 0)
                {
                    if (UI.board[position.x - move, position.y - move].piece.white == white) blocking_.Add(new Position(position.x - move, position.y - move));
                    else possibleMoves.Add(new Position(position.x - move, position.y - move));
                    UI.board[position.x - move, position.y - move].attackedByBlack.Add(this);
                }
                // top left
                move = 1;
                while (position.x - move >= 0 && position.y + move < 8 && UI.board[position.x - move, position.y + move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x - move, position.y + move));
                    UI.board[position.x - move, position.y + move].attackedByBlack.Add(this);
                    ++move;
                }
                if (position.x - move >= 0 && position.y + move < 8)
                {
                    if (UI.board[position.x - move, position.y + move].piece.white == white) blocking_.Add(new Position(position.x - move, position.y + move));
                    else possibleMoves.Add(new Position(position.x - move, position.y + move));
                    UI.board[position.x - move, position.y + move].attackedByBlack.Add(this);
                }
                // down right
                move = 1;
                while (position.x + move < 8 && position.y - move >= 0 && UI.board[position.x + move, position.y - move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x + move, position.y - move));
                    UI.board[position.x + move, position.y - move].attackedByBlack.Add(this);
                    ++move;
                }
                if (position.x + move < 8 && position.y - move >= 0)
                {
                    if (UI.board[position.x + move, position.y - move].piece.white == white) blocking_.Add(new Position(position.x + move, position.y - move));
                    else possibleMoves.Add(new Position(position.x + move, position.y - move));
                    UI.board[position.x + move, position.y - move].attackedByBlack.Add(this);
                }
                // top right
                move = 1;
                while (position.x + move < 8 && position.y + move < 8 && UI.board[position.x + move, position.y + move].piece is null)
                {
                    possibleMoves.Add(new Position(position.x + move, position.y + move));
                    UI.board[position.x + move, position.y + move].attackedByBlack.Add(this);
                    ++move;
                }
                if (position.x + move < 8 && position.y + move < 8)
                {
                    if (UI.board[position.x + move, position.y + move].piece.white == white) blocking_.Add(new Position(position.x + move, position.y + move));
                    else possibleMoves.Add(new Position(position.x + move, position.y + move));
                    UI.board[position.x + move, position.y + move].attackedByBlack.Add(this);
                }
            }
        }
        
        /**
         * @brief Adds the moves of a rook to possible moves of the piece
         */
        internal void AddRookMoves()
        {
            if (white)
            {
                // rook going down
                int x = position.x;
                int y = position.y - 1;
                while (y >= 0 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                    --y;
                }
                if (y >= 0)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                }
                // rook going up
                x = position.x;
                y = position.y + 1;
                while (y < 8 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                    ++y;
                }
                if (y < 8)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                }
                // rook going left
                x = position.x - 1;
                y = position.y;
                while (x >= 0 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                    --x;
                }
                if (x >= 0)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                }
                // rook going right
                x = position.x + 1;
                y = position.y;
                while (x < 8 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                    ++x;
                }
                if (x < 8)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByWhite.Add(this);
                }
            }
            else
            {
                // rook going down
                int x = position.x;
                int y = position.y - 1;
                while (y >= 0 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                    --y;
                }
                if (y >= 0)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                }
                // rook going up
                x = position.x;
                y = position.y + 1;
                while (y < 8 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                    ++y;
                }
                if (y < 8)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                }
                // rook going left
                x = position.x - 1;
                y = position.y;
                while (x >= 0 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                    --x;
                }
                if (x >= 0)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                }
                // rook going right
                x = position.x + 1;
                y = position.y;
                while (x < 8 && UI.board[x, y].piece is null)
                {
                    possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                    ++x;
                }
                if (x < 8)
                {
                    if (UI.board[x, y].piece.white == white) blocking_.Add(new Position(x, y));
                    else possibleMoves.Add(new Position(x, y));
                    UI.board[x, y].attackedByBlack.Add(this);
                }
            }
        }
    }

    /**
     * @brief Class representing the Pawn piece
     */
    public class Pawn : Piece
    {
        public Pawn(Position position, bool white, ChessUI UI) : base(position, white, UI) { }

        public override int Value => 1;

        public override string image()
        {
            if (white) return Piece.ImagePath() + "w_pawn.png";
            else return Piece.ImagePath() + "b_pawn.png";
        }

        public override void CellsStoppingCheck(List<Position> highlightedCells)
        {
            highlightedCells.Add(position);
        }

        public override void UpdatePosibleMoves()
        {
            RemoveAll();

            if (white)
            {
                // move forward
                if (UI.board[position.x, position.y + 1].piece is null)
                {
                    possibleMoves.Add(new Position(position.x, position.y + 1));
                    // hasn't moved yet
                    if (position.y == 1)
                    {
                        if (UI.board[position.x, position.y + 2].piece is null) possibleMoves.Add(new Position(position.x, position.y + 2));
                        else blocking_.Add(new Position(position.x, position.y + 2));
                    }
                }
                else blocking_.Add(new Position(position.x, position.y + 1));
                // taking of pieces
                if (position.x - 1 >= 0)
                {
                    if (UI.board[position.x - 1, position.y + 1].piece is not null && !UI.board[position.x - 1, position.y + 1].piece.white)
                        possibleMoves.Add(new Position(position.x - 1, position.y + 1));
                    else blocking_.Add(new Position(position.x - 1, position.y + 1));
                    UI.board[position.x - 1, position.y + 1].attackedByWhite.Add(this);
                }
                if (position.x + 1 < 8)
                {
                    if (UI.board[position.x + 1, position.y + 1].piece is not null && !UI.board[position.x + 1, position.y + 1].piece.white)
                        possibleMoves.Add(new Position(position.x + 1, position.y + 1));
                    else blocking_.Add(new Position(position.x + 1, position.y + 1));
                    UI.board[position.x + 1, position.y + 1].attackedByWhite.Add(this);
                }
                // en-passant
                if (UI.EnPassant.y == 5 && (UI.EnPassant.x == position.x + 1 || UI.EnPassant.x == position.x - 1) && position.y == UI.EnPassant.y - 1)
                    possibleMoves.Add(new Position(UI.EnPassant.x, UI.EnPassant.y));
            }
            else
            {
                // move forward
                if (UI.board[position.x, position.y - 1].piece is null)
                {
                    possibleMoves.Add(new Position(position.x, position.y - 1));
                    // hasn't moved yet
                    if (position.y == 6)
                    {
                        if (UI.board[position.x, position.y - 2].piece is null) possibleMoves.Add(new Position(position.x, position.y - 2));
                        else blocking_.Add(new Position(position.x, position.y - 2));
                    } 
                }
                else blocking_.Add(new Position(position.x, position.y - 1));
                // taking of pieces
                if (position.x - 1 >= 0)
                {
                    if (UI.board[position.x - 1, position.y - 1].piece is not null && UI.board[position.x - 1, position.y - 1].piece.white)
                        possibleMoves.Add(new Position(position.x - 1, position.y - 1));
                    else blocking_.Add(new Position(position.x - 1, position.y - 1));
                    UI.board[position.x - 1, position.y - 1].attackedByBlack.Add(this);
                }
                if (position.x + 1 < 8)
                {
                    if (UI.board[position.x + 1, position.y - 1].piece is not null && UI.board[position.x + 1, position.y - 1].piece.white)
                        possibleMoves.Add(new Position(position.x + 1, position.y - 1));
                    else blocking_.Add(new Position(position.x + 1, position.y - 1));
                    UI.board[position.x + 1, position.y - 1].attackedByBlack.Add(this);
                }
                // en-passant
                if (UI.EnPassant.y == 2 && (UI.EnPassant.x == position.x + 1 || UI.EnPassant.x == position.x - 1) && position.y == UI.EnPassant.y + 1)
                    possibleMoves.Add(new Position(UI.EnPassant.x, UI.EnPassant.y));
            }
        }

        public override bool Move(Position where)
        {
            if (!this.CheckMove(where)) return false;

            // en-passant
            if (where.x == UI.EnPassant.x && where.y == UI.EnPassant.y)
            {
                UI.EnPassant = new Position { x = -1, y = -1 };
                if (white)
                {
                    UI.board[where.x, where.y - 1].piece.RemoveAll();
                    UI.blackPieces.Remove(UI.board[where.x, where.y - 1].piece);
                    UI.board[where.x, where.y - 1].piece = null;
                    this.UpdateMovesOfCellAttackers(UI.board[where.x, where.y - 1]);
                }
                else
                {
                    UI.board[where.x, where.y + 1].piece.RemoveAll();
                    UI.whitePieces.Remove(UI.board[where.x, where.y + 1].piece);
                    UI.board[where.x, where.y + 1].piece = null;
                    this.UpdateMovesOfCellAttackers(UI.board[where.x, where.y + 1]);
                }
            }
            else
            {
                // En-passant resets if not played right away
                UI.EnPassant = new Position { x = -1, y = -1 };

                this.TakingOfAPiece(where);

                // add new en-passant
                if (where.y - position.y == 2)
                {
                    UI.EnPassant = new Position { x = where.x, y = where.y - 1 };
                    if (where.x > 0)
                    {
                        if (UI.board[where.x - 1, where.y].piece is not null) UI.board[where.x - 1, where.y].piece.UpdatePosibleMoves();
                    }
                    if (where.x < 7)
                    {
                        if (UI.board[where.x + 1, where.y].piece is not null) UI.board[where.x + 1, where.y].piece.UpdatePosibleMoves();
                    }
                }
                else if (where.y - position.y == -2) 
                {
                    UI.EnPassant = new Position { x = where.x, y = where.y + 1 };
                    if (where.x > 0)
                    {
                        if (UI.board[where.x - 1, where.y].piece is not null) UI.board[where.x - 1, where.y].piece.UpdatePosibleMoves();
                    }
                    if (where.x < 7)
                    {
                        if (UI.board[where.x + 1, where.y].piece is not null) UI.board[where.x + 1, where.y].piece.UpdatePosibleMoves();
                    }
                }
            }

            // checks if the position the pawn moved is on the edge of the board - promotion needed
            if (where.y == 0 || where.y == 7)
            {
                if (this.UI.MinimaxActive)
                {
                    Promote(PromotePieceType.Queen, where);
                }
                else
                {
                    this.UI.State = StateOfGame.PromotionPending;
                }
            }
            else
            {
                UpdateMoveAndPosition(where);
            }
            return true;
        }

        /** 
         * @brief Updates the possible moves of pieces after promotion, this is the alternative function of UpdateMoveAndPosition
         * and is used after promotion instead of UpdateMoveAndPosition
         */
        public void PromotionUpdateOfMoves(Position where)
        {
            UI.WhiteToMove = !UI.WhiteToMove;
            ++UI.HalfMoves;
            if (!white) ++UI.FullMoves;

            UI.board[position.x, position.y].piece = null;
            UpdatePawnMoves(); // update pawn moves now that the piece left
            UI.board[where.x, where.y].piece = this;

            UpdateMovesOfCellAttackers(UI.board[position.x, position.y]);

            position = where;

            RemoveAll();

            UpdateMovesOfCellAttackers(UI.board[position.x, position.y]);

            UpdatePawnMoves(); // update the pawn moves now that a new piece has appeared in their way
            // so that they can't move to Cells that are attacked by an enemy piece
        }

        /**
         * @brief Promotes this Pawn to a selected piece
         * @param promotion is the piece that this Pawn is going to get promoted to
         * @param where is the Position where the Promotion is happening
         * this class gets deleted
         */
        public void Promote(PromotePieceType promotion, Position where)
        {
            
            if (white) UI.whitePieces.Remove(this);
            else UI.blackPieces.Remove(this);
            PromotionUpdateOfMoves(where);

            switch (promotion)
            {
                case PromotePieceType.Queen:
                    UI.board[position.x, position.y].piece = new Queen(position, white, UI);
                    break;
                case PromotePieceType.Rook:
                    UI.board[position.x, position.y].piece = new Rook(position, white, UI);
                    break;
                case PromotePieceType.Bishop:
                    UI.board[position.x, position.y].piece = new Bishop(position, white, UI);
                    break;
                case PromotePieceType.Knight:
                    UI.board[position.x, position.y].piece = new Knight(position, white, UI);
                    break;
                default:
                    throw new ArgumentException("Promotion type does not exist!");
            }
            if (white) UI.whitePieces.Add(UI.board[position.x, position.y].piece);
            else UI.blackPieces.Add(UI.board[position.x, position.y].piece);

            UI.board[position.x, position.y].piece.UpdatePosibleMoves();

            foreach (King king in UI.kings)
            {
                king.UpdatePosibleMoves();
            }
            
            this.UI.Check();
            this.UI.GameEnd();
        }
    }

    /**
     * @brief Class representing the King piece
     */
    public class King : Piece
    {
        public King(Position position, bool white, ChessUI UI) : base(position, white, UI) { }

        public override int Value => 0;

        public override string image()
        {
            if (white) return Piece.ImagePath() + "w_king.png";
            else return Piece.ImagePath() + "b_king.png";
        }
        
        // CellsStoppingCheck doesnt do anything as you cant check with the king
        public override void CellsStoppingCheck(List<Position> highlightedCells) {}

        public override void UpdatePosibleMoves()
        {
            RemoveAll();
            {
                // castling
                if (white && UI.board[this.position.x, this.position.y].attackedByBlack.Count == 0) // white king 
                {
                    // UI.CastlingRights[0] Q side
                    if (UI.CastlingRights[0] && (UI.board[1, 0].piece is null && UI.board[1, 0].attackedByBlack.Count == 0)
                        && (UI.board[2, 0].piece is null && UI.board[2, 0].attackedByBlack.Count == 0)
                        && (UI.board[3, 0].piece is null && UI.board[3, 0].attackedByBlack.Count == 0))
                    {
                        // Add castling Queen side as a move
                        possibleMoves.Add(new Position(2, 0));
                    }
                    // UI.CastlingRights[1] K side
                    if (UI.CastlingRights[1] && (UI.board[5, 0].piece is null && UI.board[5, 0].attackedByBlack.Count == 0)
                        && (UI.board[6, 0].piece is null && UI.board[6, 0].attackedByBlack.Count == 0))
                    {
                        // Add castling King side as a move
                        possibleMoves.Add(new Position(6, 0));
                    }
                }
                else if (!white && UI.board[this.position.x, this.position.y].attackedByWhite.Count == 0) // black king
                {
                    // UI.CastlingRights[0] Q side
                    if (UI.CastlingRights[2] && (UI.board[1, 7].piece is null && UI.board[1, 7].attackedByWhite.Count == 0)
                        && (UI.board[2, 7].piece is null && UI.board[2, 7].attackedByWhite.Count == 0) // won't work because it can be attacked by own pieces, need to fix later
                        && (UI.board[3, 7].piece is null && UI.board[3, 7].attackedByWhite.Count == 0))
                    {
                        // Add castling Queen side as a move
                        possibleMoves.Add(new Position(2, 7));
                    }
                    // UI.CastlingRights[1] K side
                    if (UI.CastlingRights[3] && (UI.board[5, 7].piece is null && UI.board[5, 7].attackedByWhite.Count == 0)
                        && (UI.board[6, 7].piece is null && UI.board[6, 7].attackedByWhite.Count == 0))
                    {
                        // Add castling King side as a move
                        possibleMoves.Add(new Position(6, 7));
                    }
                }
            }
            for (int i = -1; i <= 1; ++i)
            {
                if (position.x + i < 0 || position.x + i > 7) continue;
                for (int j = -1; j <= 1; ++j)
                {
                    if (position.y + j < 0 || position.y + j > 7) continue;
                    if (i == 0 && j == 0) continue;
                    if (white) UI.board[position.x + i, position.y + j].attackedByWhite.Add(this);
                    else UI.board[position.x + i, position.y + j].attackedByBlack.Add(this);
                    if ((UI.board[position.x + i, position.y + j].piece is not null && UI.board[position.x + i, position.y + j].piece.white == white)
                        || (white && UI.board[position.x + i, position.y + j].attackedByBlack.Count != 0)
                        || (!white && UI.board[position.x + i, position.y + j].attackedByWhite.Count != 0)
                        )
                        blocking_.Add(new Position(position.x + i, position.y + j));
                    else possibleMoves.Add(new Position(position.x + i, position.y + j));
                }
            }
        }

        public override bool Move(Position where)
        {
            if (!this.CheckMove(where)) return false;

            UI.EnPassant = new Position { x = -1, y = -1 };

            this.TakingOfAPiece(where);

            // castling
            if (Math.Abs(position.x - where.x) > 1)
            {
                if (where.x == 2) // queen side
                {
                    Piece rook = UI.board[0, where.y].piece;
                    UI.board[3, where.y].piece = rook;
                    UI.board[0, where.y].piece = null;
                    UpdateMovesOfCellAttackers(UI.board[0, where.y]);

                    Position rookPosition = new Position { y = where.y, x = 3 };
                    rook.position = rookPosition;

                    UpdatePosibleMoves();

                    UpdateMovesOfCellAttackers(UI.board[3, where.y]);
                }
                else if (where.x == 6) // king side
                {
                    Piece rook = UI.board[7, where.y].piece;
                    UI.board[5, where.y].piece = rook;
                    UI.board[7, where.y].piece = null;

                    UpdateMovesOfCellAttackers(UI.board[7, where.y]);

                    Position rookPosition = new Position { y = where.y, x = 5 };
                    rook.position = rookPosition;

                    UpdatePosibleMoves();

                    UpdateMovesOfCellAttackers(UI.board[5, where.y]);
                }
            }

            // a move locks the ability to castle
            if (white)
            {
                UI.CastlingRights[0] = false;
                UI.CastlingRights[1] = false;
            }
            else
            {
                UI.CastlingRights[2] = false;
                UI.CastlingRights[3] = false;
            }

            UpdateMoveAndPosition(where);
            return true;
        }
    }

    /**
     * @brief Class representing the Queen piece
     */
    public class Queen : Piece
    {
        public Queen(Position position, bool white, ChessUI UI) : base(position, white, UI) { }
        public override int Value => 9;

        public override string image()
        {
            if (white) return Piece.ImagePath() + "w_queen.png";
            else return Piece.ImagePath() + "b_queen.png";
        }

        public override void CellsStoppingCheck(List<Position> highlightedCells)
        {
            // Works only if the piece is checking the king
            Position kingsPosition;
            if (white) kingsPosition = UI.kings[1].position;
            else kingsPosition = UI.kings[0].position;
            bool onADiagonal = Math.Abs(kingsPosition.x - position.x) == Math.Abs(kingsPosition.y - position.y);
            if (onADiagonal)
            {
                int numOfXTiles = kingsPosition.x - position.x;
                bool positiveDiagonal = (numOfXTiles - (kingsPosition.y - position.y)) == 0;
                for (int i = 0; position.x + i != kingsPosition.x; i += (numOfXTiles / Math.Abs(numOfXTiles)))
                {
                    if (positiveDiagonal) highlightedCells.Add(new Position { x = position.x + i, y = position.y + i });
                    else highlightedCells.Add(new Position { x = position.x + i, y = position.y - i });
                }
            }
            else
            {
                int distance = kingsPosition.x - position.x + kingsPosition.y - position.y;
                bool X_distance = (kingsPosition.x - position.x) != 0;
                for (int i = 0; Math.Abs(i) < Math.Abs(distance); i += (distance / Math.Abs(distance)))
                {
                    if (X_distance)
                        highlightedCells.Add(new Position { x = position.x + i, y = position.y });
                    else
                        highlightedCells.Add(new Position { x = position.x, y = position.y + i });
                }
            }
        }

        public override void UpdatePosibleMoves()
        {
            RemoveAll();
            AddBishopMoves();
            AddRookMoves();
        }
    }

    /**
     * @brief Class representing the Bishop piece
     */
    public class Bishop : Piece
    {
        public Bishop(Position position, bool white, ChessUI UI) : base(position, white, UI) { }

        public override int Value => 3;

        public override string image()
        {
            if (white) return Piece.ImagePath() + "w_bishop.png";
            else return Piece.ImagePath() + "b_bishop.png";
        }

        public override void CellsStoppingCheck(List<Position> highlightedCells)
        {
            // Works only if this piece really checks the king
            Position kingsPosition;
            if (white) kingsPosition = UI.kings[1].position;
            else kingsPosition = UI.kings[0].position;
            int numOfXTiles = kingsPosition.x - position.x;
            bool positiveDiagonal = (numOfXTiles - (kingsPosition.y - position.y)) == 0;
            for (int i = 0; position.x + i != kingsPosition.x; i += (numOfXTiles / Math.Abs(numOfXTiles)))
            {
                if (positiveDiagonal) highlightedCells.Add(new Position { x = position.x + i, y = position.y + i });
                else highlightedCells.Add(new Position { x = position.x + i, y = position.y - i });
            }
        }
        public override void UpdatePosibleMoves()
        {
            RemoveAll();
            AddBishopMoves();            
        }
    }

    /**
     * @brief Class representing the Knight piece
     */
    public class Knight : Piece
    {
        public Knight(Position position, bool white, ChessUI UI) : base(position, white, UI) { }
        public override int Value => 3;

        public override string image()
        {
            if (white) return Piece.ImagePath() + "w_knight.png";
            else return Piece.ImagePath() + "b_knight.png";
        }

        public override void CellsStoppingCheck(List<Position> highlightedCells)
        {
            // Works only if this piece really checks the king
            highlightedCells.Add(position);
        }

        public override void UpdatePosibleMoves()
        {
            RemoveAll();

            if (white)
            {
                if (position.x - 2 >= 0 && position.y - 1 >= 0)
                {
                    UI.board[position.x - 2, position.y - 1].attackedByWhite.Add(this);
                    if (UI.board[position.x - 2, position.y - 1].piece is null || UI.board[position.x - 2, position.y - 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 2, position.y - 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 2, position.y - 1));
                    }
                }
                if (position.x - 1 >= 0 && position.y - 2 >= 0)
                {
                    UI.board[position.x - 1, position.y - 2].attackedByWhite.Add(this);
                    if (UI.board[position.x - 1, position.y - 2].piece is null || UI.board[position.x - 1, position.y - 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 1, position.y - 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 1, position.y - 2));
                    }
                }
                if (position.x + 2 < 8 && position.y - 1 >= 0)
                {
                    UI.board[position.x + 2, position.y - 1].attackedByWhite.Add(this);
                    if (UI.board[position.x + 2, position.y - 1].piece is null || UI.board[position.x + 2, position.y - 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 2, position.y - 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 2, position.y - 1));
                    }
                }
                if (position.x + 1 < 8 && position.y - 2 >= 0)
                {
                    UI.board[position.x + 1, position.y - 2].attackedByWhite.Add(this);
                    if (UI.board[position.x + 1, position.y - 2].piece is null || UI.board[position.x + 1, position.y - 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 1, position.y - 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 1, position.y - 2));
                    }
                }
                if (position.x - 2 >= 0 && position.y + 1 < 8)
                {
                    UI.board[position.x - 2, position.y + 1].attackedByWhite.Add(this);
                    if (UI.board[position.x - 2, position.y + 1].piece is null || UI.board[position.x - 2, position.y + 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 2, position.y + 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 2, position.y + 1));
                    }
                }
                if (position.x - 1 >= 0 && position.y + 2 < 8)
                {
                    UI.board[position.x - 1, position.y + 2].attackedByWhite.Add(this);
                    if (UI.board[position.x - 1, position.y + 2].piece is null || UI.board[position.x - 1, position.y + 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 1, position.y + 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 1, position.y + 2));
                    }
                }
                if (position.x + 2 < 8 && position.y + 1 < 8)
                {
                    UI.board[position.x + 2, position.y + 1].attackedByWhite.Add(this);
                    if (UI.board[position.x + 2, position.y + 1].piece is null || UI.board[position.x + 2, position.y + 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 2, position.y + 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 2, position.y + 1));
                    }
                }
                if (position.x + 1 < 8 && position.y + 2 < 8)
                {
                    UI.board[position.x + 1, position.y + 2].attackedByWhite.Add(this);
                    if (UI.board[position.x + 1, position.y + 2].piece is null || UI.board[position.x + 1, position.y + 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 1, position.y + 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 1, position.y + 2));
                    }
                }
            }
            else
            {
                if (position.x - 2 >= 0 && position.y - 1 >= 0)
                {
                    UI.board[position.x - 2, position.y - 1].attackedByBlack.Add(this);
                    if (UI.board[position.x - 2, position.y - 1].piece is null || UI.board[position.x - 2, position.y - 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 2, position.y - 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 2, position.y - 1));
                    }
                }
                if (position.x - 1 >= 0 && position.y - 2 >= 0)
                {
                    UI.board[position.x - 1, position.y - 2].attackedByBlack.Add(this);
                    if (UI.board[position.x - 1, position.y - 2].piece is null || UI.board[position.x - 1, position.y - 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 1, position.y - 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 1, position.y - 2));
                    }
                }
                if (position.x + 2 < 8 && position.y - 1 >= 0)
                {
                    UI.board[position.x + 2, position.y - 1].attackedByBlack.Add(this);
                    if (UI.board[position.x + 2, position.y - 1].piece is null || UI.board[position.x + 2, position.y - 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 2, position.y - 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 2, position.y - 1));
                    }
                }
                if (position.x + 1 < 8 && position.y - 2 >= 0)
                {
                    UI.board[position.x + 1, position.y - 2].attackedByBlack.Add(this);
                    if (UI.board[position.x + 1, position.y - 2].piece is null || UI.board[position.x + 1, position.y - 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 1, position.y - 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 1, position.y - 2));
                    }
                }
                if (position.x - 2 >= 0 && position.y + 1 < 8)
                {
                    UI.board[position.x - 2, position.y + 1].attackedByBlack.Add(this);
                    if (UI.board[position.x - 2, position.y + 1].piece is null || UI.board[position.x - 2, position.y + 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 2, position.y + 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 2, position.y + 1));
                    }
                }
                if (position.x - 1 >= 0 && position.y + 2 < 8)
                {
                    UI.board[position.x - 1, position.y + 2].attackedByBlack.Add(this);
                    if (UI.board[position.x - 1, position.y + 2].piece is null || UI.board[position.x - 1, position.y + 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x - 1, position.y + 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x - 1, position.y + 2));
                    }
                }
                if (position.x + 2 < 8 && position.y + 1 < 8)
                {
                    UI.board[position.x + 2, position.y + 1].attackedByBlack.Add(this);
                    if (UI.board[position.x + 2, position.y + 1].piece is null || UI.board[position.x + 2, position.y + 1].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 2, position.y + 1));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 2, position.y + 1));
                    }
                }
                if (position.x + 1 < 8 && position.y + 2 < 8)
                {
                    UI.board[position.x + 1, position.y + 2].attackedByBlack.Add(this);
                    if (UI.board[position.x + 1, position.y + 2].piece is null || UI.board[position.x + 1, position.y + 2].piece.white != white)
                    {
                        possibleMoves.Add(new Position(position.x + 1, position.y + 2));
                    }
                    else
                    {
                        blocking_.Add(new Position(position.x + 1, position.y + 2));
                    }
                }
            }
        }
    }

    /**
     * @brief Class representing the Rook piece
     */
    public class Rook : Piece
    {
        public Rook(Position position, bool white, ChessUI UI) : base(position, white, UI) { }
        public override int Value => 5;

        public override string image()
        {
            if (white) return Piece.ImagePath() + "w_rook.png";
            else return Piece.ImagePath() + "b_rook.png";
        }

        public override void CellsStoppingCheck(List<Position> highlightedCells)
        {
            // Works only if this piece really checks the king
            Position kingsPosition;
            if (white) kingsPosition = UI.kings[1].position;
            else kingsPosition = UI.kings[0].position;
            int distance = kingsPosition.x - position.x + kingsPosition.y - position.y;
            bool X_distance = (kingsPosition.x - position.x) != 0;
            for (int i = 0; Math.Abs(i) < Math.Abs(distance); i += (distance / Math.Abs(distance)))
            {
                if (X_distance)
                    highlightedCells.Add(new Position { x = position.x + i, y = position.y});
                else
                    highlightedCells.Add(new Position { x = position.x, y = position.y + i });
            }
        }

        public override void UpdatePosibleMoves()
        {
            RemoveAll();
            AddRookMoves();
        }

        public override bool Move(Position where)
        {
            if (!this.CheckMove(where)) return false;

            UI.EnPassant = new Position { x = -1, y = -1 };

            // castling is locked after a rook move
            if (white)
            {
                if (position.y == 0)
                {
                    if (position.x == 0)
                    {
                        if (UI.CastlingRights[0]) UI.kings[0].UpdatePosibleMoves();
                        UI.CastlingRights[0] = false;
                    }
                    else if (position.x == 7)
                    {
                        if (UI.CastlingRights[1]) UI.kings[0].UpdatePosibleMoves();
                        UI.CastlingRights[1] = false;
                    }
                }
            }
            else
            {
                if (position.y == 7)
                {
                    if (position.x == 0)
                    {
                        if (UI.CastlingRights[2]) UI.kings[0].UpdatePosibleMoves();
                        UI.CastlingRights[2] = false;
                    }
                    else if (position.x == 7)
                    {
                        if (UI.CastlingRights[3]) UI.kings[0].UpdatePosibleMoves();
                        UI.CastlingRights[3] = false;
                    }
                }
            }
            
            this.TakingOfAPiece(where);

            UpdateMoveAndPosition(where);
            return true;
        }
    }
}
