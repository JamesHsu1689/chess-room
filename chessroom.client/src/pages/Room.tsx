import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import * as signalR from '@microsoft/signalr'
import { Chess } from 'chess.js'
import { Chessboard } from 'react-chessboard'
import { API_BASE } from '../api'  // removed getRoom

type Presence = {
    players: { white: boolean; black: boolean }
}

type JoinAck = {
    fen: string
    turn: 'w' | 'b'
    whiteMs: number
    blackMs: number
    lastMoveUnix: number
    role?: 'white' | 'black' | null
    presence: Presence
}

export default function Room() {
    const { roomId } = useParams()
    const [role, setRole] = useState<'white' | 'black' | null>(null)
    const [fen, setFen] = useState('start')
    const [turn, setTurn] = useState<'w' | 'b'>('w')
    const chess = useMemo(() => new Chess(), [])
    const connRef = useRef<signalR.HubConnection | null>(null)

    useEffect(() => {
        if (!roomId) return

        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE}/hubs/game`)
            .withAutomaticReconnect()
            .build()

        conn.on('Joined', (ack: JoinAck) => {
            const f = ack.fen === 'startpos' ? undefined : ack.fen
            chess.reset()
            if (f) chess.load(f)
            setFen(chess.fen()); setTurn(ack.turn)
            setRole(ack.role ?? null)
        })

        conn.on('GameUpdated', (u: any) => {
            chess.load(u.fen)
            setFen(chess.fen()); setTurn(u.turn)
        })

        conn.start().then(async () => {
            await conn.invoke('JoinRoom', roomId, null)
        })

        connRef.current = conn
        return () => { conn.stop() }
    }, [roomId, chess])

    async function takeSeat(which: 'white' | 'black') {
        if (!connRef.current) return
        await connRef.current.invoke('JoinRoom', roomId, which)
    }

    async function onDrop(source: string, target: string) {
        if (!connRef.current) return false
        const isMyTurn = (turn === 'w' && role === 'white') || (turn === 'b' && role === 'black')
        if (!isMyTurn) return false

        const move = chess.move({ from: source, to: target, promotion: 'q' })
        if (!move) return false

        await connRef.current.invoke('MakeMove', roomId, {
            from: source, to: target, promotion: move.promotion ?? null,
            san: move.san, fenAfter: chess.fen()
        })
        return true
    }

    return (
        <div style={{ maxWidth: 960, margin: '2rem auto', padding: 16 }}>
            <h2>Room {roomId}</h2>
            <div style={{ display: 'flex', gap: 24, alignItems: 'flex-start' }}>
                <div style={{ width: 480 }}>
                    <Chessboard
                        id="board"
                        position={fen === 'startpos' ? undefined : fen}
                        arePiecesDraggable={role !== null}
                        boardOrientation={role === 'black' ? 'black' : 'white'}
                        onPieceDrop={onDrop}
                    />
                </div>
                <div>
                    <p>Turn: {turn === 'w' ? 'White' : 'Black'}</p>
                    <div style={{ display: 'flex', gap: 8 }}>
                        <button onClick={() => takeSeat('white')} disabled={role === 'white'}>Play White</button>
                        <button onClick={() => takeSeat('black')} disabled={role === 'black'}>Play Black</button>
                        <button onClick={() => setRole(null)}>Spectate</button>
                    </div>
                    <p style={{ marginTop: 16 }}>Share this code: <b>{roomId}</b></p>
                </div>
            </div>
        </div>
    )
}
