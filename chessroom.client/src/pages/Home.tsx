import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createRoom } from '../api'

export default function Home() {
    const [joinId, setJoinId] = useState('')
    const nav = useNavigate()

    return (
        <div style={{ maxWidth: 480, margin: '4rem auto', padding: 16 }}>
            <h1>Quick Chess</h1>
            <p>Create a room and share the code, or join an existing one.</p>

            <button onClick={async () => {
                const r = await createRoom()
                nav(`/r/${r.roomId}`)
            }}>New Game</button>

            <div style={{ marginTop: 24 }}>
                <input value={joinId} onChange={e => setJoinId(e.target.value.toUpperCase())}
                    placeholder="Enter room ID" />
                <button onClick={() => nav(`/r/${joinId}`)}>Join</button>
            </div>
        </div>
    )
}
