// Enhanced game.js with improved UX functionality
"use strict";

// Initialize chess board and game state
var board = null;
var game = new Chess();
const connection = new signalR.HubConnectionBuilder().withUrl("/gamehub").build();

// UI elements
var $status = $('#status');
var $fen = $('#fen');
var $pgn = $('#pgn');
var $moveHistory = $('#moveHistory');
var $playerInfo = $('#playerInfo');
var $timer = $('#timer');
var $lastMove = $('#lastMove');
var $turnIndicator = $('#turnIndicator');

// Game context
var side = context.side;
var gameId = context.gameId;
var lastMove = null;
var isMyTurn = side[0] === game.turn();

// Initialize UI
function initUI() {
    updatePlayerInfo();
    updateTurnIndicator();

    // Add event listeners
    $('#offerDrawBtn').on('click', offerDraw);
    $('#resignBtn').on('click', resignGame);

    // Show welcome notification
    showNotification("Game started. " + (isMyTurn ? "It's your turn!" : "Waiting for opponent's move."));
}

// Update the turn indicator
function updateTurnIndicator() {
    isMyTurn = side[0] === game.turn();
    $turnIndicator.text(isMyTurn ? "Your turn" : "Opponent's turn");
    $turnIndicator.removeClass('text-success text-warning');
    $turnIndicator.addClass(isMyTurn ? 'text-success' : 'text-warning');

    // Highlight board based on turn
    $('.board-b72b1').css('box-shadow', isMyTurn ? '0 0 15px rgba(40, 167, 69, 0.5)' : '0 0 15px rgba(255, 193, 7, 0.5)');
}

// Piece drag start handler - only allow dragging when it's player's turn
function onDragStart(source, piece, position, orientation) {
    // Do not pick up pieces if the game is over
    if (game.game_over()) {
        showNotification("Game is over!");
        return false;
    }

    // Only pick up pieces for the player's side and when it's their turn
    if (game.turn() !== side[0]) {
        showNotification("Not your turn!", "warning");
        return false;
    }

    // Only pick up pieces for the side to move
    if ((game.turn() === 'w' && piece.search(/^b/) !== -1) ||
        (game.turn() === 'b' && piece.search(/^w/) !== -1)) {
        return false;
    }
}

// Handle piece drop
function onDrop(source, target) {
    // Clear any highlighted squares
    removeHighlights();

    // See if the move is legal
    var move = game.move({
        from: source,
        to: target,
        promotion: 'q' // Always promote to a queen for example simplicity
    });

    // Illegal move
    if (move === null) {
        showNotification("Illegal move!", "warning");
        return 'snapback';
    }

    // Store the last move for highlighting
    lastMove = { from: source, to: target };
    highlightLastMove();

    // Send move to SignalR hub
    connection.invoke("SendPGNGameState", gameId, game.pgn())
        .catch(e => {
            console.error(e);
            showNotification("Failed to send move", "danger");
        });

    // Update status and UI
    updateStatus();
    updateMoveHistory();
    updateTurnIndicator();

    // Play move sound
    playMoveSound(move);
}

// Highlight the last move
function highlightLastMove() {
    if (lastMove) {
        // Add highlight class to source and target squares
        $('#board .square-' + lastMove.from).addClass('highlight-source');
        $('#board .square-' + lastMove.to).addClass('highlight-target');
    }
}

// Remove all highlights
function removeHighlights() {
    $('#board .square-55d63').removeClass('highlight-source highlight-target');
}

// Update the board position after the piece snap
function onSnapEnd() {
    board.position(game.fen());
}

// Update game status display
function updateStatus() {
    var status = '';
    var moveColor = 'White';

    if (game.turn() === 'b') {
        moveColor = 'Black';
    }

    // Checkmate?
    if (game.in_checkmate()) {
        status = 'Game over, ' + moveColor + ' is in checkmate!';
        connection.invoke("RegisterGameEnd", gameId, moveColor).catch(e => console.error(e));
        showGameOverMessage(moveColor + ' wins by checkmate!');
    }
    // Draw?
    else if (game.in_draw()) {
        status = 'Game over, drawn position';
        connection.invoke("RegisterGameEnd", gameId, "draw").catch(e => console.error(e));
        showGameOverMessage('Game ended in a draw');
    }
    // Game still on
    else {
        status = moveColor + ' to move';

        // Check?
        if (game.in_check()) {
            status += ', ' + moveColor + ' is in check!';
            playCheckSound();
        }
    }

    $status.html(status);
    $fen.html(game.fen());
    $pgn.html(game.pgn());
}

// Update move history with scrolling list and move numbers
function updateMoveHistory() {
    var history = game.history({ verbose: true });
    var html = '';

    for (var i = 0; i < history.length; i++) {
        // Add move number at start of white's move
        if (i % 2 === 0) {
            html += '<div class="move-pair">';
            html += '<span class="move-number">' + (Math.floor(i / 2) + 1) + '.</span>';
        }

        var move = history[i];
        var piece = move.piece.toUpperCase();
        var capture = move.captured ? 'x' : '';

        // Create move notation with piece symbol
        html += '<span class="move ' + (move.color === 'w' ? 'white-move' : 'black-move') + '">';
        html += piece !== 'P' ? piece : '';
        html += capture;
        html += move.to;
        html += '</span>';

        // Close div after black's move
        if (i % 2 === 1) {
            html += '</div>';
        }
    }

    // If we ended on white's move, close the div
    if (history.length % 2 === 1) {
        html += '</div>';
    }

    $moveHistory.html(html);

    // Scroll to bottom
    $moveHistory.scrollTop($moveHistory[0].scrollHeight);
}

// Update player information
function updatePlayerInfo() {
    var whitePlayer = context.whitePlayerName || "White Player";
    var blackPlayer = context.blackPlayerName || "Black Player";

    var html = '<div class="player white-player">';
    html += '<i class="fas fa-chess-king white-piece"></i> ';
    html += '<span>' + whitePlayer + '</span>';
    html += side === 'white' ? ' (You)' : '';
    html += '</div>';

    html += '<div class="player black-player">';
    html += '<i class="fas fa-chess-king black-piece"></i> ';
    html += '<span>' + blackPlayer + '</span>';
    html += side === 'black' ? ' (You)' : '';
    html += '</div>';

    $playerInfo.html(html);
}

// Show notification
function showNotification(message, type = "info") {
    // Remove any existing notification
    $('.game-notification').remove();

    // Create notification
    var $notification = $('<div class="game-notification alert alert-' + type + ' alert-dismissible fade show">' +
        message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
        '</div>');

    // Add to DOM
    $('#notificationArea').append($notification);

    // Auto-remove after 5 seconds
    setTimeout(function () {
        $notification.alert('close');
    }, 5000);
}

// Show game over message
function showGameOverMessage(message) {
    var $modal = $('#gameOverModal');
    $('#gameOverMessage').text(message);
    $modal.modal('show');
}

// Offer draw
function offerDraw() {
    // In a real application, this would send a draw offer to the opponent via SignalR
    showNotification("Draw offer functionality would be implemented here");
}

// Resign game
function resignGame() {
    // In a real application, this would send a resignation notification via SignalR
    if (confirm("Are you sure you want to resign?")) {
        connection.invoke("RegisterGameEnd", gameId, side[0] === "w" ? "Black" : "White").catch(e => console.error(e));
        showGameOverMessage("You resigned the game");
    }
}

// Play move sound
function playMoveSound(move) {
    // In a real application, this would play a sound based on the move type
    // Capture, check, castle, etc. would have different sounds
    var audio;

    if (move.flags.includes('c')) {
        // Capture sound
        audio = new Audio('/sounds/capture.mp3');
    } else if (move.flags.includes('k') || move.flags.includes('q')) {
        // Castle sound
        audio = new Audio('/sounds/castle.mp3');
    } else {
        // Regular move sound
        audio = new Audio('/sounds/move.mp3');
    }

    // Play sound if available
    if (audio) {
        audio.volume = 0.5;
        audio.play().catch(e => console.log("Audio play failed: " + e));
    }
}

// Play check sound
function playCheckSound() {
    var audio = new Audio('/sounds/check.mp3');
    audio.volume = 0.5;
    audio.play().catch(e => console.log("Audio play failed: " + e));
}

// Configuration for the chess board
var config = {
    draggable: true,
    position: 'start',
    orientation: side,
    onDragStart: onDragStart,
    onDrop: onDrop,
    onSnapEnd: onSnapEnd
};

// Initialize the board
board = Chessboard('myBoard', config);

// If there is saved game state, load it
if (context.pgn) {
    loadPGN(context.pgn);
}

// Update initial status
updateStatus();
updateMoveHistory();
initUI();

// Function to load a PGN
function loadPGN(pgn) {
    game.load_pgn(pgn);
    board.position(game.fen());

    // Set last move for highlighting
    var history = game.history({ verbose: true });
    if (history.length > 0) {
        var lastMoveObj = history[history.length - 1];
        lastMove = { from: lastMoveObj.from, to: lastMoveObj.to };
        highlightLastMove();
    }

    updateStatus();
    updateMoveHistory();
    updateTurnIndicator();
}

// SignalR connection handlers
connection.on("ReceivePGNGameState", pgn => {
    loadPGN(pgn);
    showNotification("Opponent made a move");
});

connection.on("UserJoined", username => {
    showNotification(username + " joined the game", "success");
    updatePlayerInfo(); // Refresh player info when opponent joins
});

connection.on("Error", message => {
    console.error(message);
    showNotification("Error: " + message, "danger");
});

connection.on("PlayerRoleAssigned", function(role) {
    console.log("Player role assigned:", role);
    context.side = role;
    config.orientation = role;
    side = role;
    board = Chessboard('myBoard', config)
});

// Start SignalR connection
connection.start()
    .then(() => {
        connection.invoke("JoinGame", gameId).catch(err => {
            console.error(err.toString());
            showNotification("Failed to join game", "danger");
        });
        connection.invoke("RequestPGNGameState", gameId).catch(err => {
            console.error(err.toString());
            showNotification("Failed to request game state", "danger");
        });
    })
    .catch(err => {
        console.error(err.toString());
        showNotification("Failed to connect to game server", "danger");
    });

// Responsive handling
$(window).resize(function () {
    board.resize();
});

// Add keyboard controls
$(document).keydown(function (e) {
    // Undo last move with Ctrl+Z (for testing)
    if (e.ctrlKey && e.keyCode === 90) {
        game.undo();
        board.position(game.fen());
        updateStatus();
        updateMoveHistory();
    }
});