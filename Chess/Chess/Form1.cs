namespace Chess
{
    public partial class Chess : Form
    {
        private bool vsAI = false;
        private ChessUI? game;
        private Position? previousClick = null;
        private Position? currentClickCoordinates = null;
        private Panel[,] panels = new Panel[8, 8];
        private Label[] boardLabels = new Label[16];
        private List<PictureBox> pieces = new List<PictureBox>();
        private Button[] promotionButtons = new Button[4];
        private bool cellsCreated = false;
        private Pawn? promotionPiece = null;
        private List<Move> highlightedMoves = new List<Move>();
        private Label gameendl;
        private List<string> history = new List<string>(); // FENs of previous turns
        private string lastFEN = "";
        private Minimax minimax;
        private bool playAsWhite = true;
        private Move? playedMove = null;
        /**
        * @brief This function is the constructor of the Chess class.
        * It initializes the components of the fform.
        */
        public Chess()
        {
            InitializeComponent();
            AIdifficultybar.Visible = false;
            AIdifficultyl.Visible = false;
            // I don't initialize the game board here because this.ClientSize changes later which
            // makes the board positioned incorrectly and the size is also incorrect
            // workaround -> i have a bool cellsCreated and create it only once - when the first StartGame() is called

        }

        private void Chess_Load(object sender, EventArgs e)
        {

        }

        /**
        * @brief This function starts the game by creating 8*8 panels with alternating colors (OliveDrab and grey) and adding them to a list,
        * then creates labels for the board (1-8) to the left of the board and (a-h) to the bottom of the board.
        * It also adds the Click event handler to all the panels.
        */
        public void CreateCells()
        {
            // create 8*8 panels with alternating colors (OliveDrab and grey) and add them to a list
            int size = (this.ClientSize.Height) / 11;
            int totalSize = 8 * size; // total size of all panels
            // calculate the starting point to center the panels
            int x = (this.ClientSize.Width - totalSize) / 2;
            int y = (this.ClientSize.Height - totalSize) / 2 + 30;

            for (int j = 7; j >= 0; --j)
            {
                for (int i = 0; i < 8; ++i)
                {
                    panels[i, j] = new Panel();
                    panels[i, j].Size = new Size(size, size);
                    panels[i, j].BackColor = Color.Lime;
                    panels[i, j].BackColor = ((i + j) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;
                    panels[i, j].Location = new Point(x, y);

                    this.Controls.Add(panels[i, j]);
                    x += size;
                }
                y += size;
                x = (this.ClientSize.Width - totalSize) / 2; // reset x for the new row
            }

            // create labels for the board (1-8) to the left of the board and (a-h) to the bottom of the board
            int labelSize = size / 2;
            int labelX = (this.ClientSize.Width - totalSize) / 2 - labelSize;
            int labelY = (this.ClientSize.Height + labelSize - totalSize) / 2 + 30;
            for (int i = 0; i < 8; ++i)
            {
                boardLabels[i] = new Label();
                boardLabels[i].Size = new Size(labelSize, labelSize);
                boardLabels[i].Text = (8 - i).ToString();
                boardLabels[i].Location = new Point(labelX, labelY);
                boardLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                boardLabels[i].Font = new Font("Calibri", 18, FontStyle.Bold);
                this.Controls.Add(boardLabels[i]);
                labelY += size;
            }
            labelX = (this.ClientSize.Width + labelSize - totalSize) / 2;
            // it is already pretty much at the right position, just need move it closer to the board
            labelY -= labelSize / 2 - 5;
            for (int i = 8; i < 16; ++i)
            {
                boardLabels[i] = new Label();
                boardLabels[i].Size = new Size(labelSize, labelSize);
                boardLabels[i].Text = ((char)('a' + i - 8)).ToString();
                boardLabels[i].Location = new Point(labelX, labelY);
                boardLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                boardLabels[i].Font = new Font("Calibri", 18, FontStyle.Bold);
                this.Controls.Add(boardLabels[i]);
                labelX += size;
            }
            // add the Click event handler to all the panels
            foreach (Panel panel in panels)
            {
                panel.Click += new EventHandler(this.Cell_Click);
            }
            this.cellsCreated = true;
        }

        /**
        * @brief This function loads the pieces on the board by adding coreesponding PictueBoxes 
        * of the pieces to the panels representing Cells.
        */
        public void StartGame()
        {
            // because here this.ClientSize is already set correctly
            if (!this.cellsCreated)
            {
                this.CreateCells();
            }
            for (int j = 7; j >= 0; --j)
            {
                for (int i = 0; i < 8; ++i)
                {

                    Piece? piece = this.game.board[i, j].piece;
                    if (piece != null)
                    {
                        PictureBox picture = new PictureBox();
                        pieces.Add(picture);
                        picture.Image = Image.FromFile(piece.image());
                        picture.Tag = piece.image();
                        picture.SizeMode = PictureBoxSizeMode.StretchImage;
                        picture.Dock = DockStyle.Fill;
                        panels[i, j].Controls.Add(picture);
                        picture.Click += new EventHandler(this.Cell_Click);
                    }
                }
            }
            this.lastFEN = this.game.GetFEN();

            if (this.game.WhiteToMove)
            {
                this.currentplayerl.Text = "White's turn";
            }
            else
            {
                this.currentplayerl.Text = "Black's turn";
            }
            this.ColorKingCheck();
        }

        /**
         * @brief This function colors the possible moves of the piece on the panel with yellow.
         * @param position The position of the piece Whose moves were highlighted.
         * @param back If true, it colors the panels back to their original color.
         */
        private void ColorPossibleMoves(Position position, bool back = false)
        {
            // if the king is in check, color his panel red - could have been overcolored back if the user clicked on the king
            this.ColorKingCheck();
            // if there was a move was played, highlight it - could have been overcolored back if the cell was one of the possible moves
            this.ColorPlayedMove(true);

            if (back)
            {
                foreach (Move move in this.highlightedMoves)
                {
                    panels[move.to.x, move.to.y].BackColor =
                        ((move.to.x + move.to.y) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;
                }
                this.highlightedMoves.Clear();
            }
            else
            {
                foreach (Move move in this.game.possibleMoves)
                {
                    if (move.from.x == position.x && move.from.y == position.y)
                    {
                        panels[move.to.x, move.to.y].BackColor =
                            ((move.to.x + move.to.y) % 2 == 0) ? Color.Gray : Color.DimGray; 
                        this.highlightedMoves.Add(move);
                    }
                }
            }

            if (back) this.panels[position.x, position.y].BackColor =
                ((position.x + position.y) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;
            else this.panels[position.x, position.y].BackColor = Color.Yellow;
        }

        private void ColorKingCheck()
        {
            // index of the current king
            int king_index = this.game.WhiteToMove ? 0 : 1;

            // color king's panel back to its original color                    
            Position kingPosition = this.game.kings[1 - king_index].position;
            panels[kingPosition.x, kingPosition.y].BackColor =
                ((kingPosition.x + kingPosition.y) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;

            kingPosition = this.game.kings[king_index].position;

            // If king is in check color his panel red
            if (this.game.State == StateOfGame.Check)
                panels[kingPosition.x, kingPosition.y].BackColor = Color.Red;
            else // color king's panel back to its original color
                panels[kingPosition.x, kingPosition.y].BackColor = 
                    ((kingPosition.x + kingPosition.y) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;
        }

        /**
         * @brief This function takes care of the previous move by changing the color of the panels back to their original color.
         */
        private void ColorPlayedMove(bool recolor = false)
        {
            // recolor only if we Clicked on a Cell and changed the color back to its original color
            if (recolor)
            {
                if (this.playedMove.HasValue)
                {
                    panels[this.playedMove.Value.from.x, this.playedMove.Value.from.y].BackColor = Color.LimeGreen;
                    panels[this.playedMove.Value.to.x, this.playedMove.Value.to.y].BackColor = Color.Lime;
                }
                return;
            }
            if (this.playedMove.HasValue)
            {
                panels[this.playedMove.Value.from.x, this.playedMove.Value.from.y].BackColor =
                    ((this.playedMove.Value.from.x + this.playedMove.Value.from.y) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;
                panels[this.playedMove.Value.to.x, this.playedMove.Value.to.y].BackColor =
                    ((this.playedMove.Value.to.x + this.playedMove.Value.to.y) % 2 == 0) ? Color.AntiqueWhite : Color.OliveDrab;
            }

            if (this.previousClick.HasValue && this.currentClickCoordinates.HasValue)
            {
                this.playedMove = new Move(this.previousClick.Value, this.currentClickCoordinates.Value);
                panels[this.playedMove.Value.from.x, this.playedMove.Value.from.y].BackColor = Color.LimeGreen;
                panels[this.playedMove.Value.to.x, this.playedMove.Value.to.y].BackColor = Color.Lime;
            }
        }

        /**
         * @brief This function updates the visuals of the board by adding or removing pieces from the panels
         * by comparing it to the current state of the game
         */
        private void UpdateGameBoard()
        {
            this.ColorPlayedMove();
            this.ColorKingCheck();
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    if (this.game.board[i, j].piece is not null)
                    {
                        #pragma warning disable
                        if (panels[i, j].Controls.Count > 0)
                        {
                            // Tag won't be null because we are sure that there is a piece which has a tag
                            if ((string)panels[i, j].Controls[0].Tag == this.game.board[i, j].piece.image()) continue;
                            panels[i, j].Controls[0].Dispose();
                        }
                        PictureBox picture = new PictureBox();
                        pieces.Add(picture);
                        picture.Image = Image.FromFile(this.game.board[i, j].piece.image());
                        picture.Tag = this.game.board[i, j].piece.image();
                        picture.SizeMode = PictureBoxSizeMode.StretchImage;
                        picture.Dock = DockStyle.Fill;
                        panels[i, j].Controls.Add(picture);
                        picture.Click += new EventHandler(this.Cell_Click);
                        #pragma warning restore
                    }
                    else if (panels[i, j].Controls.Count > 0)
                    {
                        pieces.Remove((PictureBox)panels[i, j].Controls[0]);
                        panels[i, j].Controls[0].Dispose();
                    }
                }
            }
            // game is not null if UpdateGameBoard is called
            #pragma warning disable
            if (this.game.WhiteToMove)
            {
                this.currentplayerl.Text = "White's turn";
            }
            else
            {
                this.currentplayerl.Text = "Black's turn";
            }
            #pragma warning restore
        }

        /**
         * @brief This function updates the visuals of the board by adding or removing pieces from the panels
         * by comparing it to the current state of the game
         * if vsAI is set and it's the turn of the AI, it also plays a move
         */
        private void UpdateGame()
        {
            this.UpdateGameBoard();

            if (this.vsAI && this.playAsWhite != this.game.WhiteToMove)
            {
                // clicks won't have a value only if we are using the back button
                if (this.previousClick.HasValue && this.currentClickCoordinates.HasValue)
                {
                    Move playedMove = new Move { from = this.previousClick.Value, to = this.currentClickCoordinates.Value };
                    var level = this.AIdifficultybar.Value;
                    Move? minimaxMove = this.minimax.MinimaxPlay(this.game, playedMove);
                    if (minimaxMove != null)
                    {
                        this.previousClick = minimaxMove.Value.from;
                        this.currentClickCoordinates = minimaxMove.Value.to;
                    }
                    this.lastFEN = this.game.GetFEN();
                    this.playedMove = playedMove;
                }
                this.UpdateGameBoard();
            }
            this.previousClick = null;
            this.currentClickCoordinates = null;
        }

        /**
         * @brief This function returns the position of the panel that the user clicked on.
         * @param x The x coordinate of the click.
         * @param y The y coordinate of the click.
         * @return The position of the panel that the user clicked on.
         */
        private Position? GetPositionOfClick(int x, int y)
        {
            int size = (this.ClientSize.Height) / 11;
            int totalSize = 8 * size; // total size of all panels
            int x0 = (this.ClientSize.Width - totalSize) / 2;
            int y0 = (this.ClientSize.Height - totalSize) / 2 + 30;

            int i = (x - x0) / size;
            int j = 7 - (y - y0) / size;
            if (i < 8 && j < 8 && i >= 0 && j >= 0)
                return new Position(i, j);
            return null;
        }

        /** @brief This function is called when the user clicks on a panel.
        * It checks if the user clicked on a piece and if it is the user's turn.
        * If the user clicked on a piece, it highlights the panel with yellow.
        * If the user clicked on a panel with a highlighted panel, it moves the piece to the new panel.
        * @param sender The object that called the function.
        * @param e The event arguments.
        */
        private void Cell_Click(object? sender, EventArgs e)
        {
            if (!(this.game.State == StateOfGame.Game || this.game.State == StateOfGame.Check)) return;
            currentClickCoordinates = GetPositionOfClick(MousePosition.X, MousePosition.Y);
            if (currentClickCoordinates is null)
                return;
            Position currentClickPosition = currentClickCoordinates.Value;
            if (!this.previousClick.HasValue)
            {
                if (this.game.board[currentClickPosition.x, currentClickPosition.y].piece is not null
                    && this.game.board[currentClickPosition.x, currentClickPosition.y].piece.white == this.game.WhiteToMove)
                {
                    this.previousClick = currentClickPosition;
                    ColorPossibleMoves(currentClickPosition);
                }
            }
            else
            {
                if (this.game.Move(new Move(this.previousClick.Value, currentClickPosition)))
                {
                    // possible Moves are rewriten before this, that's why we need the highlightedMoves list
                    ColorPossibleMoves(this.previousClick.Value, true);

                    if (this.game.State == StateOfGame.PromotionPending)
                    {
                        this.promotionPiece = (Pawn)this.game.board[this.previousClick.Value.x, this.previousClick.Value.y].piece;
                        this.CreatePromotionButtons();
                    }
                    else
                    {
                        this.history.Add(this.lastFEN);
                        this.lastFEN = this.game.GetFEN();
                        backb.Enabled = true;

                        // Update the board also plays a move if minimax is active
                        this.UpdateGame();
                    }
                }
                else if (this.game.board[currentClickPosition.x, currentClickPosition.y].piece is not null
                    && this.game.board[currentClickPosition.x, currentClickPosition.y].piece.white == this.game.WhiteToMove)
                {
                    ColorPossibleMoves(this.previousClick.Value, true);
                    this.previousClick = currentClickPosition;
                    ColorPossibleMoves(currentClickPosition);
                }
            }
            if (this.game.State == StateOfGame.Draw
                || this.game.State == StateOfGame.WhiteWins
                || this.game.State == StateOfGame.BlackWins)
            {
                this.CreateEndGameLabel();
            }
        }

        /**
         * @brief This function switches between the Main Menu to Game interface
         * and if vsAI is active it starts the minimax algorithm if switching to a game, othrewise it stops it.
         */
        private void SwitchInterface(bool toGame = true)
        {
            if (toGame)
            {
                this.playb.Visible = false;
                this.exitb.Visible = false;

                this.saveb.Visible = true;
                this.loadb.Enabled = false;

                this.vsAIb.Enabled = false;
                this.AIdifficultybar.Enabled = false;

                this.backb.Visible = true;
                backb.Enabled = false;
                this.mainmenub.Visible = true;
                // first time cells are not initialized yet
                if (this.cellsCreated)
                {
                    foreach (Panel panel in panels)
                    {
                        panel.Visible = true;
                    }
                    foreach (Label label in boardLabels)
                    {
                        label.Visible = true;
                    }
                }
                if (this.vsAI == false)
                {
                    this.currentplayerl.Visible = true;
                }
                else
                {
                    // Create a thread that will calculate the minimax algorithm using the static Minimax class 
                    List<Move> moves = null;
                    var level = this.AIdifficultybar.Value;
                    this.minimax = new Minimax(this.game, level);
                    this.playasb.Visible = false;
                    if (this.playAsWhite != this.game.WhiteToMove)
                    {
                        // Move _ so that i don't have to create a similar function MinimaxPlay
                        Move _ = new Move { from = new Position(), to = new Position() };
                        this.minimax.MinimaxPlay(this.game, _);

                        this.UpdateGame();
                        this.lastFEN = this.game.GetFEN();
                        if (this.game.State == StateOfGame.Draw || this.game.State == StateOfGame.WhiteWins
                                                       || this.game.State == StateOfGame.BlackWins)
                        {
                            this.CreateEndGameLabel();
                        }
                    }
                }
            }
            else
            {
                this.currentplayerl.Visible = false;
                this.playb.Visible = true;
                this.exitb.Visible = true;

                this.saveb.Visible = false;
                this.loadb.Enabled = true;

                this.vsAIb.Enabled = true;
                this.AIdifficultybar.Enabled = true;

                this.backb.Visible = false;
                this.mainmenub.Visible = false;

                if (this.vsAI)
                {
                    this.playasb.Visible = true;
                    this.minimax.Cancel();
                }
                if (this.gameendl != null)
                {
                    this.Controls.Remove(this.gameendl);
                    this.gameendl.Dispose();
                }


                foreach (Panel panel in panels)
                {
                    panel.Visible = false;
                    panel.Controls.Clear();
                }
                foreach (Label label in boardLabels)
                {
                    label.Visible = false;
                }
                foreach (PictureBox piece in pieces)
                {
                    piece.Dispose();
                }
                foreach (Button button in promotionButtons)
                {
                    if (button != null)
                        button.Dispose();
                }
            }
        }

        /**
         * @brief Creates a label that says who won the game and adds it to the form.
         */
        private void CreateEndGameLabel()
        {
            if (this.gameendl != null) return;
            if (this.previousClick.HasValue)
            {
                this.previousClick = null;
            }
            this.gameendl = new Label();
            this.gameendl.Size = new Size(this.ClientSize.Width / 2,
                this.ClientSize.Height / 2);
            this.gameendl.Font = new Font("Calibri", 72, FontStyle.Bold);

            this.gameendl.Location = new Point(this.ClientSize.Width / 2 - this.gameendl.Width / 2,
                this.ClientSize.Height / 2 - this.gameendl.Height / 2);
            this.gameendl.BackColor = Color.WhiteSmoke;

            switch (this.game.State)
            {
                case StateOfGame.WhiteWins:
                    this.gameendl.Text = "CheckMate! White wins";
                    break;
                case StateOfGame.BlackWins:
                    this.gameendl.Text = "CheckMate! Black wins";
                    break;
                case StateOfGame.Draw:
                    if (this.game.possibleMoves.Count == 0)
                        this.gameendl.Text = "Stalemate! Draw";
                    else
                        this.gameendl.Text = "Draw! Insufficient material";
                    break;

                default:
                    // should never happen
                    throw new Exception("Invalid state of game");
            }
            this.gameendl.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(this.gameendl);
            this.gameendl.BringToFront();
        }

        /**
         * @brief This function is called when the user clicks on the play button.
         * It starts the game by creating a new ChessUI object and calling the StartGame function.
         * It also makes the start menu invisible.
         */
        private void playb_Click(object sender, EventArgs e)
        {
            this.game = new ChessUI();
            this.StartGame();
            this.SwitchInterface();
        }

        /**
         * @brief This function is called when the user clicks on the exit button.
         * It closes the application.
         */
        private void exitb_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /**
         * @brief This function is called when the user clicks on the vs AI button.
         * It changes the text of the vs AI button to "Player vs player" if the user clicked on it when it was "Player vs AI" and vice versa.
         * It also makes the AI difficulty bar and label visible if the user clicked on it when it was "Player vs AI" and vice versa.
         */
        private void vsAIb_Click(object sender, EventArgs e)
        {
            if (this.vsAI)
            {
                vsAIb.Text = "Player vs player";
                AIdifficultybar.Visible = false;
                AIdifficultyl.Visible = false;
                this.vsAI = false;
                this.playasb.Visible = false;
            }
            else
            {
                vsAIb.Text = "Player vs AI";
                AIdifficultybar.Visible = true;
                AIdifficultyl.Visible = true;
                this.vsAI = true;
                this.playasb.Visible = true;
            }
        }

        /**
         * @brief This function is called when the user changes the value of the AI difficulty bar.
         * It changes the text of the AI difficulty label to the value of the AI difficulty bar.
         */
        private void AIdifficultybar_Scroll(object sender, EventArgs e)
        {
            AIdifficultyl.Text = "AI difficulty: " + AIdifficultybar.Value;
        }

        /**
         * @brief This function is called when the user clicks on the play as button.
         * It changes the text of the play as button to "Play as white" if the user clicked on it when it was "Play as black" and vice versa.
         */
        private void playasb_Click(object sender, EventArgs e)
        {
            this.playAsWhite = !this.playAsWhite;
            if (this.playAsWhite)
            {
                this.playasb.Text = "Play as white";
            }
            else
            {
                this.playasb.Text = "Play as black";
            }
        }

        /**
         * @brief This function is called when the user clicks on the save button.
         * It opens a file dialog and saves the game to the selected file.
         */
        private void saveb_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // save the working directory
                string? _ = Application.StartupPath;
                string workingDirectory = (_ is not null) ? _ : "";
                // this is where the directory of Saved games is
                saveFileDialog.InitialDirectory = workingDirectory + "../../../SaveGames";
                // looking only for .txt files
                saveFileDialog.Filter = "text file (*.txt)|*.txt";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of specified file
                    string filePath = saveFileDialog.FileName;

                    // Write the contents of the file into a stream
                    var fileStream = saveFileDialog.OpenFile();

                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        this.game.SaveGame(writer);
                    }
                }
            }

        }

        /**
         * @brief This function is called when the user clicks on the load button.
         * It opens a file dialog and loads the game from the selected file.
         */
        private void loadb_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // save the working directory
                string workingDirectory = Application.StartupPath;
                // this is where the directory of Saved games is
                openFileDialog.InitialDirectory = workingDirectory + "../../../SaveGames";
                // looking only for .txt files
                openFileDialog.Filter = "text file (*.txt)|*.txt";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of specified file
                    string filePath = openFileDialog.FileName;

                    // Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        this.game = new ChessUI(reader);
                        this.StartGame();
                        this.SwitchInterface();
                    }
                }
            }
        }

        /**
         * @brief This function is called when the user clicks on the back or Main menu button.
         * It resets resets the colors of the played move and deletes the history of the played move.
         */
        private void ResetColorsOfPlayedMove()
        {
            // so that the next game doesn't start with colored panels from the previous game
            this.previousClick = null;
            this.currentClickCoordinates = null;
            this.ColorPlayedMove();
            this.playedMove = null;

        }

        /**
         * @brief This function is called when the user clicks on the back button.
         * It undoes the last move and updates the game board.
         */
        private void backb_Click(object? sender, EventArgs e)
        {
            if (this.game.State == StateOfGame.Draw || this.game.State == StateOfGame.WhiteWins
                || this.game.State == StateOfGame.BlackWins)
            {
                this.Controls.Remove(this.gameendl);
                this.gameendl.Dispose();
            }
            if (this.previousClick.HasValue)
            {
                ColorPossibleMoves(this.previousClick.Value, true);
            }
            if (this.game.State == StateOfGame.PromotionPending)
            {
                foreach (Button button in promotionButtons)
                {
                    button.Dispose();
                }
            }
            // TODO: if the game is in check, the king's panel is red, but if we go back, it should be green
            bool inCheck = this.game.State == StateOfGame.Check;
            this.game = new ChessUI(this.history.Last());
            this.history.RemoveAt(this.history.Count - 1);
            this.lastFEN = this.game.GetFEN();

            if (inCheck) ColorKingCheck();

            if (this.history.Count is 0)
            {
                backb.Enabled = false;
            }

            if (this.vsAI) this.minimax.StartOver(this.game);

            // clicks MUST be reset so that the minimax algorithm doesn't play a move (in UpdateGame the minimax reacts to a played move)
            this.previousClick = null;
            this.currentClickCoordinates = null;

            this.ResetColorsOfPlayedMove();
            this.UpdateGame();

            this.gameendl = null;
        }

        /**
         * @brief This function is called when the user clicks on the Main menu button.
         * It switches to the main menu from the game interface
         */
        private void mainmenub_Click(object sender, EventArgs e)
        {
            if (this.previousClick.HasValue)
            {
                ColorPossibleMoves(this.previousClick.Value, true);
            }

            this.history.Clear();
            SwitchInterface(false);

            this.ResetColorsOfPlayedMove();

            this.gameendl = null;
        }

        /**
         * @brief This function creates buttons for the promotion of the pawn.
         */
        private void CreatePromotionButtons()
        {
            // get the location of Images directory
            string imagesDirectory = Path.Combine(Application.StartupPath, @"..\\Images\\");
            int size = (this.ClientSize.Height) / 9;
            int totalSize = 4 * size; // total size of all panels
            // calculate the starting point to center the buttons
            int x = (this.ClientSize.Width - totalSize) / 2;
            int y = (this.ClientSize.Height - totalSize) / 2;
            promotionButtons[0] = new Button();
            promotionButtons[0].Size = new Size(size, size);
            if (this.game.WhiteToMove) promotionButtons[0].Image = Image.FromFile(imagesDirectory + "w_queen.png");
            else promotionButtons[0].Image = Image.FromFile(imagesDirectory + "b_queen.png");
            promotionButtons[0].Location = new Point(x, y);
            promotionButtons[0].Click += new EventHandler(this.Queen_Click);
            this.Controls.Add(promotionButtons[0]);
            x += size;
            promotionButtons[1] = new Button();
            promotionButtons[1].Size = new Size(size, size);
            if (this.game.WhiteToMove) promotionButtons[1].Image = Image.FromFile(imagesDirectory + "w_rook.png");
            else promotionButtons[1].Image = Image.FromFile(imagesDirectory + "b_rook.png");
            promotionButtons[1].Location = new Point(x, y);
            promotionButtons[1].Click += new EventHandler(this.Rook_Click);
            this.Controls.Add(promotionButtons[1]);
            x += size;
            promotionButtons[2] = new Button();
            promotionButtons[2].Size = new Size(size, size);
            if (this.game.WhiteToMove) promotionButtons[2].Image = Image.FromFile(imagesDirectory + "w_bishop.png");
            else promotionButtons[2].Image = Image.FromFile(imagesDirectory + "b_bishop.png");
            promotionButtons[2].Location = new Point(x, y);
            promotionButtons[2].Click += new EventHandler(this.Bishop_Click);
            this.Controls.Add(promotionButtons[2]);
            x += size;
            promotionButtons[3] = new Button();
            promotionButtons[3].Size = new Size(size, size);
            if (this.game.WhiteToMove) promotionButtons[3].Image = Image.FromFile(imagesDirectory + "w_knight.png");
            else promotionButtons[3].Image = Image.FromFile(imagesDirectory + "b_knight.png");
            promotionButtons[3].Location = new Point(x, y);
            promotionButtons[3].Click += new EventHandler(this.Knight_Click);
            this.Controls.Add(promotionButtons[3]);
            foreach (Button button in promotionButtons)
            {
                button.BringToFront();
            }
        }

        /**
         * @brief This function is called when the user clicks on the promotion button.
         * It updates the game board and removes the promotion buttons.
         */
        private void PromotionUpdate()
        {
            this.history.Add(this.lastFEN);
            this.lastFEN = this.game.GetFEN();
            backb.Enabled = true;

            this.UpdateGame();
            foreach (Button button in promotionButtons)
            {
                button.Dispose();
            }

            if (this.game.State == StateOfGame.Draw || this.game.State == StateOfGame.WhiteWins
                || this.game.State == StateOfGame.BlackWins)
            {
                this.CreateEndGameLabel();
            }
        }

        // This function is called when the user clicks on the promotion Queen button
        private void Queen_Click(object? sender, EventArgs e)
        {
            // change the pawn to a queen
            this.promotionPiece.Promote(PromotePieceType.Queen, this.currentClickCoordinates.Value);
            this.PromotionUpdate();
        }

        // This function is called when the user clicks on the promotion Rook button
        private void Rook_Click(object? sender, EventArgs e)
        {
            // change the pawn to a rook
            this.promotionPiece.Promote(PromotePieceType.Rook, this.currentClickCoordinates.Value);
            this.PromotionUpdate();
        }

        // This function is called when the user clicks on the promotion Bishop button
        private void Bishop_Click(object? sender, EventArgs e)
        {
            // change the pawn to a bishop
            this.promotionPiece.Promote(PromotePieceType.Bishop, this.currentClickCoordinates.Value);
            this.PromotionUpdate();

        }

        // This function is called when the user clicks on the promotion Knight button
        private void Knight_Click(object? sender, EventArgs e)
        {
            // change the pawn to a knight
            this.promotionPiece.Promote(PromotePieceType.Knight, this.currentClickCoordinates.Value);
            this.PromotionUpdate();
        }
    }
}