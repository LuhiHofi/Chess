using System;
using System.Collections.Concurrent;

namespace Chess
{
    /**
     * This class is used to implement the minimax algorithm
     */
    public class Minimax
    {
        public ConcurrentDictionary<Move, Move> _bestResponses = new ConcurrentDictionary<Move, Move>();
        private Task _minimax;
        private CancellationTokenSource _cancellationTokenSource;
        private int _level { get; set; }

        public Minimax(ChessUI currentBoard, int level = 0)
        {
            this._level = level;
            this._cancellationTokenSource = new CancellationTokenSource();
            this._minimax = Task.Run(() => this.FindBestResponses(currentBoard, this._level, this._cancellationTokenSource.Token));
        }

        /**
         * This function is used to start the minimax algorithm
         * @param currentBoard the current board
         */
        public void Start(ChessUI currentBoard)
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            this._minimax = Task.Run(() => this.FindBestResponses(currentBoard, this._level, this._cancellationTokenSource.Token));
        }

        /**
         * This function is used to cancel the previous computations and start the minimax algorithm over
         * @param currentBoard the current board
         */
        public void StartOver(ChessUI currentBoard)
        {
            this._cancellationTokenSource.Cancel();

            this._cancellationTokenSource = new CancellationTokenSource();
            this._minimax = Task.Run(() => this.FindBestResponses(currentBoard, this._level, this._cancellationTokenSource.Token));
        }

        /**
         * This function is used to play the best move for the current player
         * @param currentBoard the current board
         * @param lastMove the last move played by the player to check if there is a best response to it already computed
         */
        public Move? MinimaxPlay(ChessUI currentBoard, Move lastMove)
        {
            this._cancellationTokenSource.Cancel();

            currentBoard.MinimaxActive = true;

            Move bestMove = new Move();

            if (_bestResponses.ContainsKey(lastMove))
            {
                currentBoard.Move(_bestResponses[lastMove]);
                bestMove = _bestResponses[lastMove];
            }
            else
            {
                if (!(currentBoard.State == StateOfGame.Game || currentBoard.State == StateOfGame.Check)) return null;
                this._cancellationTokenSource = new CancellationTokenSource();
                Task<Move> TaskMove = Task<Move>.Run(() => BestPlay(currentBoard, this._level, this._cancellationTokenSource.Token));
                TaskMove.Wait();
                bestMove = TaskMove.Result;
                currentBoard.Move(bestMove);
            }

            currentBoard.MinimaxActive = false;
            this._bestResponses.Clear();
            Start(currentBoard);
            return bestMove;
        }

        /**
         * This function is used to cancel the computations of the minimax algorithm
         */
        public void Cancel()
        {
            this._cancellationTokenSource.Cancel();
            this._minimax.Wait();
        }

        /**
         * This function is used to score a piece
         * @param piece the piece to score
         * @return the score of the piece
         */
        private static double ScorePiece(Piece piece)
        {
            double score = piece.Value;
            switch (piece)
            {
                case Pawn:
                    score += 0.05 * (piece.position.x * (7 - piece.position.x));
                    if (piece.white) score += 0.1 * piece.position.y;
                    else score += 0.1 * (7 - piece.position.y);
                    break;
                case King:
                    break;
                case Queen:
                    score += 0.01 * piece.possibleMoves.Count;
                    break;
                case Bishop:
                    score += 0.03 * piece.possibleMoves.Count;
                    break;
                case Knight:
                    score += 0.5 * piece.possibleMoves.Count;
                    break;
                case Rook:
                    score += 0.02 * piece.possibleMoves.Count;
                    break;
                default: // should never happen
                    break;
            }
            if (piece.position.y == 3 || piece.position.y == 4) score += 0.1;
            if (piece.position.x == 3 || piece.position.x == 4) score += 0.1;

            return score;
        }

        /**
         * This function is used to evaluate the current board
         * @param currentBoard the current board
         * @return the score of the board
         */
        private static double Evaluate(ChessUI currentBoard)
        {
            if (currentBoard.State == StateOfGame.WhiteWins) return 100;
            else if (currentBoard.State == StateOfGame.BlackWins) return -100;
            else if (currentBoard.State == StateOfGame.Draw) return 0;
            double score = 0;
            foreach (Piece piece in currentBoard.whitePieces)
            {
                score += ScorePiece(piece);
            }
            foreach (Piece piece in currentBoard.blackPieces)
            {
                score -= ScorePiece(piece);
            }
            score = currentBoard.WhiteToMove ?  score + 1 : score - 1;

            // to make the games more diverse
            var random = new Random();
            score += random.NextDouble();
            
            return score;
        }

        /**
         * This function is used to find the value of the current board using the alpha-beta pruning
         * @param currentBoard the current board
         * @param depth the _level of the minimax algorithm
         * @param alpha the alpha value
         * @param beta the beta value
         * @return the value of the current board
         */
        private static double MinimaxValue(ChessUI currentBoard, int depth, CancellationToken cancellationToken, double alpha = double.MinValue, double beta = double.MaxValue)
        {

            if (depth <= 1 || !(currentBoard.State == StateOfGame.Game || currentBoard.State == StateOfGame.Check))
                return Evaluate(currentBoard);

            double bestScore = currentBoard.WhiteToMove ? double.MinValue : double.MaxValue;
            List<Move> moves = currentBoard.possibleMoves;

            // i doubled the code so that i don't have to ask if i it's white to move or not in the loop
            if (currentBoard.WhiteToMove)
            {
                foreach (Move move in moves)
                {
                    if (cancellationToken.IsCancellationRequested) return 0;
                    ChessUI newBoard = new ChessUI(currentBoard);
                    newBoard.Move(move);
                    double moveValue = MinimaxValue(newBoard, depth - 1, cancellationToken, alpha, beta);
                    bestScore = Math.Max(bestScore, moveValue);
                    alpha = Math.Max(alpha, bestScore);
                    if (beta <= alpha) break; // Beta cutoff
                }
            }
            else
            {
                foreach (Move move in moves)
                {
                    if (cancellationToken.IsCancellationRequested) return 0;
                    ChessUI newBoard = new ChessUI(currentBoard);
                    newBoard.Move(move);
                    double moveValue = MinimaxValue(newBoard, depth - 1, cancellationToken, alpha, beta);
                    bestScore = Math.Min(bestScore, moveValue);
                    beta = Math.Min(beta, bestScore);
                    if (beta <= alpha) break; // Alpha cutoff
                }
            }
            return bestScore;
        }

        /**
         * This function is used to find the best move for the current player
         * @param currentBoard the current board
         * @param _level the _level of the minimax algorithm
         * @return the best move
         */
        private static async Task<Move> BestPlay(ChessUI currentBoard, int depth, CancellationToken cancellationToken)
        {
            Move bestMove = currentBoard.possibleMoves[0];
            List<Move> moves = currentBoard.possibleMoves;
            ConcurrentBag<(Move, Task<double>)> scoredMoves = new ConcurrentBag<(Move, Task<double>)>();
            double bestScore = currentBoard.WhiteToMove ? double.MinValue : double.MaxValue;

            foreach (Move move in moves)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ChessUI newBoard = new ChessUI(currentBoard);
                newBoard.Move(move);
                // run asynchrounsly MinimaxValue and add it to the scoredMoves list 
                scoredMoves.Add((move, Task.Run<double>(() => MinimaxValue(newBoard, depth, cancellationToken))));
            }
            await Task.WhenAll(scoredMoves.Select(x => x.Item2));

            if (currentBoard.WhiteToMove)
            {
                foreach (var scoredMove in scoredMoves)
                {
                    if (scoredMove.Item2.Result > bestScore)
                    {
                        bestScore = scoredMove.Item2.Result;
                        bestMove = scoredMove.Item1;
                    }
                }
            }
            else
            {
                foreach (var scoredMove in scoredMoves)
                {
                    if (scoredMove.Item2.Result < bestScore)
                    {
                        bestScore = scoredMove.Item2.Result;
                        bestMove = scoredMove.Item1;
                    }
                }
            }

            return bestMove;
        }
        /**
         * This function is used to find an estimate of the best move for the current player
         * to then calculate the best response to them first
         * @param currentBoard the current board
         * @return estimated ordered list of moves from best to worst
         */
        private static List<Move> MovesOrderedByQuality(ChessUI currentBoard, int depth, CancellationToken cancellationToken)
        {
            List<(Move, double)> moves = new List<(Move, double)>();
            var movesList = new List<Move>(currentBoard.possibleMoves);
            foreach (Move move in movesList)
            {
                if (cancellationToken.IsCancellationRequested) return new List<Move>();
                ChessUI newBoard = new ChessUI(currentBoard);
                newBoard.Move(move);
                double score = MinimaxValue(newBoard, depth, cancellationToken);
                moves.Add((move, score));
            }
            moves.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            // return the moves in order 
            return moves.Select(x => (x.Item1)).ToList();
        }

        /**
         * This function is used to find the best response to the current board
         * @param currentBoard the current board
         * @param _level the _level of the minimax algorithm
         * @return the best response to the current board
         */
        private async void FindBestResponses(ChessUI currentBoard, int depth, CancellationToken cancellationToken)
        {
            var orderedMoves = Minimax.MovesOrderedByQuality(currentBoard, 1, cancellationToken);

            foreach (Move move in orderedMoves)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                ChessUI newBoard = new ChessUI(currentBoard);
                newBoard.Move(move);
                if (!(newBoard.State == StateOfGame.Game || newBoard.State == StateOfGame.Check)) continue;
                try
                {
                    Move bestResponse = await BestPlay(newBoard, depth, cancellationToken);
                    this._bestResponses[move] = bestResponse;
                }
                catch (OperationCanceledException)
                {
                    // Do nothing
                }
            }
        }
    }
}
