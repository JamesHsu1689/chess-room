//export const API_BASE = import.meta.env.VITE_API_BASE;
export const API_BASE = import.meta.env.VITE_API_BASE || '';

export async function createRoom() {
    const res = await fetch(`${API_BASE}/api/rooms`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ allowSpectators: true }),
    });
    if (!res.ok) throw new Error('Failed to create room');
    return res.json() as Promise<{ roomId: string; joinUrl: string; hostToken: string }>;
}

export async function getRoom(roomId: string) {
    const res = await fetch(`${API_BASE}/api/rooms/${roomId}`);
    if (!res.ok) throw new Error('Room not found');
    return res.json();
}
