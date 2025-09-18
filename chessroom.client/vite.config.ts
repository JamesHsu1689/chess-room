import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'   // <-- use "react", not "plugin"
import fs from 'fs'
import path from 'path'
import child_process from 'child_process'
import { env } from 'process'

const baseFolder =
    env.APPDATA && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`

const certificateName = 'chessroom.client'
const certFilePath = path.join(baseFolder, `${certificateName}.pem`)
const keyFilePath = path.join(baseFolder, `${certificateName}.key`)

if (!fs.existsSync(baseFolder)) fs.mkdirSync(baseFolder, { recursive: true })

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    const result = child_process.spawnSync('dotnet', [
        'dev-certs', 'https',
        '--export-path', certFilePath,
        '--format', 'Pem',
        '--no-password',
    ], { stdio: 'inherit' })
    if (result.status !== 0) throw new Error('Could not create certificate.')
}

const target =
    env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
        env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] :
            'https://localhost:7115'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
        // make absolutely sure only one React/ReactDOM instance is used
        dedupe: ['react', 'react-dom'],
    },
    server: {
        proxy: {
            // this proxy is optional; your app calls the API directly via VITE_API_BASE
            '^/weatherforecast': { target, secure: false },
        },
        port: parseInt(env.DEV_SERVER_PORT || '60201', 10),
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        },
    },
})
