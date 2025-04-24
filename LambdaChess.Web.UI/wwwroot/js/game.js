"use strict";

var board = null
var game = new Chess()
const connection = new signalR.HubConnectionBuilder().withUrl("/gamehub").build();
var $status = $('#status')
var $fen = $('#fen')
var $pgn = $('#pgn')

var side = context.side;
var gameId = context.gameId;

$("#test").html("GameId: " + gameId + " Side: " + side);

function onDragStart(source, piece, position, orientation) {
    // do not pick up pieces if the game is over
    if (game.game_over()) return false
    if (game.turn() !== side[0]) return false
    // only pick up pieces for the side to move
    if ((game.turn() === 'w' && piece.search(/^b/) !== -1) ||
        (game.turn() === 'b' && piece.search(/^w/) !== -1)) {
        return false
    }
}

function onDrop(source, target) {
    // see if the move is legal
    var move = game.move({
        from: source,
        to: target,
        promotion: 'q' // NOTE: always promote to a queen for example simplicity
    })

    // illegal move
    if (move === null) return 'snapback'

    connection.invoke("SendPGNGameState", gameId, game.pgn()).catch(e => console.error(e));

    updateStatus()
}

// update the board position after the piece snap
// for castling, en passant, pawn promotion
function onSnapEnd() {
    board.position(game.fen())
}

function updateStatus() {
    var status = ''

    var moveColor = 'White'
    if (game.turn() === 'b') {
        moveColor = 'Black'
    }

    // checkmate?
    if (game.in_checkmate()) {
        status = 'Game over, ' + moveColor + ' is in checkmate.'
        connection.invoke("RegisterGameEnd", gameId, status).catch(e => console.error(e));
    }

    // draw?
    else if (game.in_draw()) {
        status = 'Game over, drawn position'
        connection.invoke("RegisterGameEnd", gameId, status).catch(e => console.error(e));
    }

    // game still on
    else {
        status = moveColor + ' to move'

        // check?
        if (game.in_check()) {
            status += ', ' + moveColor + ' is in check'
        }
    }

    $status.html(status)
    $fen.html(game.fen())
    $pgn.html(game.pgn())
}

var config = {
    draggable: true,
    position: 'start',
    orientation: side,
    onDragStart: onDragStart,
    onDrop: onDrop,
    onSnapEnd: onSnapEnd
}
board = Chessboard('myBoard', config)

if (context.pgn) loadPGN(context.pgn);

updateStatus()

const loadPGN = (pgn) => {
    game.load_pgn(pgn);
    board.position(game.fen());
    console.log(pgn, game.pgn());

    updateStatus();
}

connection.on("ReceivePGNGameState", loadPGN);

connection.on("Error", function (message) {
    console.error(message);
});

connection.start().then(function () {
    connection.invoke("JoinGame", gameId).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});