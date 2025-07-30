# CHESS

**author**: Lukáš Hofman  
MFF UK 2024

# Introduction  

I am a student of Charles University at the faculty of Mathematics and Physics and am currently aiming to gain the bachelor's degree in AI.
This is my credit program for the classes of C# - Chess with minimax in Win Forms.  
The minimax is planned to be calculated in a different thread.     

# Rules of Chess

## Movement
### Basic moves

Each type of chess piece has its own method of movement. A piece moves to a vacant square except when capturing an opponent's piece.  

Except for any move of the knight and castling, pieces cannot jump over other pieces. A piece is captured (or taken) when an attacking enemy piece replaces it on its square. The captured piece is thereby permanently removed from the game. The king can be put in check but cannot be captured.  

The king moves exactly one square horizontally, vertically, or diagonally. A special move with the king known as castling is allowed only once per player, per game.  

A rook moves any number of vacant squares horizontally or vertically. It also is moved when castling.  

A bishop moves any number of vacant squares diagonally. (Thus a bishop can move to only light or dark squares, not both.)  

The queen moves any number of vacant squares horizontally, vertically, or diagonally.  
A knight moves to one of the nearest squares not on the same rank, file, or diagonal. (This can be thought of as moving two squares horizontally then one square vertically, or moving one square horizontally then two squares vertically—i.e. in an "L" pattern.) The knight is not blocked by other pieces; it jumps to the new location.  

Pawns have the most complex rules of movement:  
- A pawn moves straight forward one square, if that square is vacant. If it has not yet moved, a pawn also has the option of moving two squares straight forward, provided both squares are vacant. Pawns cannot move backwards.  
- A pawn, unlike other pieces, captures differently from how it moves. A pawn can capture an enemy piece on either of the two squares diagonally in front of the pawn. It cannot move to those squares when vacant except when capturing en passant.  
- The pawn is also involved in the two special moves en passant and promotion.

### Castling

Castling consists of moving the king two squares towards a rook, then placing the rook on the other side of the king, adjacent to it. It is not allowed to move both king and rook in the same time, because "Each move must be played with one hand only." Castling is only permissible if all of the following conditions hold:  

- The king and rook involved in castling must not have previously moved;
- There must be no pieces between the king and the rook;
- The king may not currently be under attack, nor may the king pass through or end up in a square that is under attack by an enemy piece (though the rook is permitted to be under attack and to pass over an attacked square);  
- The castling must be kingside or queenside
- An unmoved king and an unmoved rook of the same color on the same rank are said to have castling rights.

### En passant

When a pawn advances two squares on its initial move and ends the turn adjacent to an enemy pawn on the same rank, it may be captured en passant by the enemy pawn as if it had moved only one square. This capture is legal only on the move immediately following the pawn's advance. The diagrams demonstrate an instance of this: if the white pawn moves from a2 to a4, the black pawn on b4 can capture it en passant, moving from b4 to a3, and the white pawn on a4 is removed from the board.  

### Promotion

If a player advances a pawn to its eighth rank, the pawn is then promoted (converted) to a queen, rook, bishop, or knight of the same color at the choice of the player (a queen is usually chosen). The choice is not limited to previously captured pieces. Hence it is theoretically possible for a player to have up to nine queens or up to ten rooks, bishops, or knights if all of their pawns are promoted. If the desired piece is not available, the player must call the arbiter to provide the piece.

### Check

A king is in check when it is under attack by at least one enemy piece. A piece unable to move because it would place its own king in check (it is pinned against its own king) may still deliver check to the opposing player.  
  
It is illegal to make a move that places or leaves one's king in check. The possible ways to get out of check are:  

- Move the king to a square where it is not in check.  
- Capture the checking piece (possibly with the king).  
- Block the check by placing a piece between the king and the opponent's threatening piece.  

If it is not possible to get out of check, the king is checkmated and the game is over (see the next section).  
  
In informal games, it is customary to announce "check" when making a move that puts the opponent's king in check. In formal competitions, however, check is rarely announced
## End of the game

### Checkmate

If a player's king is placed in check and there is no legal move that player can make to escape check, then the king is said to be checkmated, the game ends, and that player loses. Unlike the other pieces, the king is never captured.  

### Draws

The game ends in a draw if any of these conditions occur:  

- The player to move is not in check and has no legal move. This situation is called a stalemate. An example of such a position is shown in the adjacent diagram.
- The same board position has occurred three times with the same player to move and all pieces having the same rights to move, including the right to castle or capture en passant (see threefold repetition rule).
- There has been no capture or pawn move in the last fifty moves by each player, if the last move was not a checkmate (see fifty-move rule).

## Goals

- [x] [Create functional Console Chess]
- [x] [Create a visual enviroment in Win Forms]
- [x] [Create multithread Minimax]
- [x] [Create a evaluation function of the position]

## Project status
Finished
