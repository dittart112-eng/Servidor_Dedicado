# Multiplayer Player–Host (Unity 6 · .NET Standard 2.1)

> Cliente/host minimalista con UI que asigna el `LocalId` y activa/desactiva el control local.  
> Servidor TCP **dual-stack** (IPv4 + IPv6) con apagado limpio. Sin `LocalInputConnector`.

![Unity](https://img.shields.io/badge/Unity-6.x-black?logo=unity)
![.NET](https://img.shields.io/badge/.NET-Standard%202.1-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Características

- **Arquitectura Player–Host**: un proceso puede actuar como **Host** (servidor + cliente) y otros como **Clientes**.  
- **UI autorreferencial**: la interfaz define `IP`, `Puerto`, `GameId` y `LocalId`.  
- **Control local dinámico**: `GameManager.RebindLocalControl()` agrega o quita `LocalInputTransfer` según el `LocalId`.  
- **Servidor dual-stack**: `TcpListener` escuchando en `0.0.0.0` (IPv4) e IPv6 cuando está disponible.  
- **Rutas REST simples**: `GET/POST /server/{room}/{playerId}` con `ServerData` en JSON.  
- **Sin paquetes externos**: UGUI (Legacy InputField) + componentes propios.

---

## Requisitos

- **Unity 6.x** (URP o 3D Core; no dependencias externas).  
- **.NET Standard 2.1** en scripts.  
- (Opcional) `curl` para probar endpoints.

---

## Estructura de carpetas (sugerida)

```
Assets/
  Scripts/
    ApiCliente.cs
    ApiServidor.cs
    GameManager.cs
    MultiplayerUI.cs
    PlayerController.cs
    LocalInputTransfer.cs
    SyncTask.cs
    HostController.cs (opcional)
  Scenes/
    Main.unity
```

---

## Scripts (resumen)

- **ApiCliente**: construye `baseUrl`, hace `GET/POST` de `ServerData`.  
- **ApiServidor**: servidor TCP dual-stack, expone `/server/{room}/{playerId}`.  
- **GameManager**: orquesta jugadores, `RebindLocalControl(localId)`, pausa global, envía/recibe estado.  
- **MultiplayerUI**: panel de conexión, arranca Host o Client, conmutación de paneles.  
- **PlayerController**: movimiento básico de cápsulas por `MovePlayer()`.  
- **LocalInputTransfer**: lee WASD/Arrows y publica al `GameManager` (se añade/retira dinámicamente).  
- **SyncTask**: bucle de sincronización (pull/push) temporizado.  
- **HostController** *(opcional)*: controles extra cuando se ejecuta como host.

---

## Modelo de datos

**ServerData (JSON)**

```json
{
  "posX": 0.0,
  "posY": 0.0,
  "posZ": 0.0,
  "paused": false
}
```

**Endpoints**

- `GET  /server/{room}/{playerId}` → `ServerData`  
- `POST /server/{room}/{playerId}` ← `ServerData`

Ejemplos:

```bash
# GET
curl http://127.0.0.1:5005/server/room-1/0

# POST
curl -X POST http://127.0.0.1:5005/server/room-1/0 \
  -H "Content-Type: application/json" \
  -d '{"posX":1.2,"posY":0.0,"posZ":-3.4,"paused":false}'
```

---

## Puesta en marcha rápida

1. **Importa** los scripts a `Assets/Scripts/`.  
2. **Crea la escena**:

   - Tres cápsulas: `Player`, `Player (1)`, `Player (2)`  
     - Añade **PlayerController** y asigna ids `0`, `1`, `2`.  
     - **No** uses `LocalInputConnector`.

   - GameObject **GameManager** con:  
     - `ApiCliente`, `GameManager`, `SyncTask`.  
     - En `GameManager.players` arrastra las cápsulas ordenadas por id (`0 → 1 → 2`).

   - **Canvas** con `MultiplayerUI`:  
     - `DisconnectedPanel` (activo): 4 **Input Field (Legacy)** → **IP**, **Port**, **GameId**, **LocalId**; botones **Start Host** y **Start Client**.  
     - `ConnectedPanel` (inactivo): botón **Stop**.  
     - (Opcional) `HostPanel` con `ApiServidor` y `HostController`.

   - **EventSystem** con **Standalone Input Module**.

3. **Prueba básica**:

   - **Editor (Host)**  
     - IP `127.0.0.1`, Port `5005`, GameId `room-1`, LocalId `0`.  
     - Click **Start Host**.  
     - Deberías ver en consola:  
       ```
       [ApiServidor] TcpListener escuchando en http://0.0.0.0:5005/ (dual-stack)
       ```
     - En navegador: `http://127.0.0.1:5005/server/room-1/0` devuelve JSON.

   - **Build (Cliente)**  
     - IP `127.0.0.1`, Port `5005`, GameId `room-1`, LocalId `1`.  
     - Click **Start Client**.

**Resultado esperado**: Cada instancia controla **solo su** cápsula (según `LocalId`) y ambas ven posiciones sincronizadas.

---

## Flujo de la UI

```mermaid
flowchart LR
    A[DisconnectedPanel] -- Start Host --> B[ApiServidor.StartServer]
    B --> C[ApiCliente.baseUrl = http://127.0.0.1:PORT/server]
    A -- Start Client --> C
    C --> D[GameManager.gameId = GameId]
    C --> E[SyncTask.localPlayerId = LocalId]
    D --> F[GameManager.RebindLocalControl(LocalId)]
    E --> G[SyncTask.StartSync()]
    G --> H[ConnectedPanel]
```

---

## Notas de configuración

- **Dual-stack**: asegúrate de que `ApiServidor` escuche en `0.0.0.0:PORT` (acepta IPv4/IPv6 si el sistema lo soporta).  
- **Firewall**: en Windows, permite el puerto (`5005` por defecto).  
- **Local vs LAN**:  
  - En el **mismo equipo**, usa `127.0.0.1`.  
  - En **equipos distintos**, usa la **IP LAN** del Host (p. ej., `192.168.1.X`).  
- **Editor + Build**: el flujo recomendado es Host en Editor y Client en Build (o viceversa).

---
