# Multiplayer Player-Host — Unity 6 / .NET Standard 2.1

Ejemplo **cliente–servidor local** con control por jugador y **sin `LocalInputBinder`**.  
La **UI** asigna el `LocalId` y el `GameManager` activa o desactiva el `LocalInputMover` con `RebindLocalControl()`.  
El **servidor** usa `TcpListener` **dual‑stack** (acepta IPv4 e IPv6) y cierra de forma limpia.

---

## Características
- ✅ Arquitectura Player‑Host: un proceso actúa como **host** (servidor + jugador) y otros como **clientes**.
- ✅ **Sin `LocalInputBinder`**: la UI decide qué cápsula controlas en caliente.
- ✅ **SyncLoop** para envíos/recepciones a intervalo fijo (suavizado).
- ✅ **TCP dual‑stack**: escucha en `0.0.0.0` e IPv6; apagado ordenado.
- ✅ UI mínima para conectar, elegir `LocalId` y detener sesión.
- ✅ Probado en **Unity 6** con **.NET Standard 2.1**.

---

## Requisitos
- Unity 6 (URP o 3D Core).
- Plataforma Standalone (Windows/macOS/Linux).
- Abrir el puerto (por defecto **5005**) en el firewall del host.

---

## Estructura (resumen)
```
Assets/
  Scripts/
    ApiClient.cs        // cliente TCP: envía input/posiciones y recibe estado
    ApiServer.cs        // servidor TCP dual‑stack
    GameManager.cs      // orquestación + RebindLocalControl(localId)
    LocalInputMover.cs  // controla solo al jugador local
    MultiplayerUI.cs    // UI de conexión/elección de LocalId
    SyncLoop.cs         // temporizador de snapshots
```
> No se utiliza `LocalInputBinder`.

---

## Montaje de escena
1. **Jugadores**
   - Crea 3 cápsulas: `Player`, `Player (1)`, `Player (2)`.
   - Añade **PlayerController** y fija `playerId` = 0, 1, 2.
   - Añade **LocalInputMover** (déjalo **desactivado** en todos).

2. **GameManager (GO vacío)**
   - Componentes: **ApiClient**, **GameManager**, **SyncLoop**.
   - En `GameManager.players` arrastra las cápsulas en orden **0→N**.
   - Ajusta `SyncLoop.interval` (p.ej. 0.25 s).

3. **Canvas de UI**
   - `DisconnectedPanel` (activo): *Input Fields* → **IP**, **Port**, **GameId**, **LocalId**; botones **Start Host** y **Start Client**.
   - `ConnectedPanel` (inactivo): botón **Stop**.
   - (Opcional) `HostPanel` con **ApiServer** y `HostControls`.

4. **EventSystem**
   - Usa **Standalone Input Module**.

> La UI llama `GameManager.RebindLocalControl(localId)` al conectar.

---

## Puesta en marcha rápida
### Host (Editor)
- IP: `127.0.0.1` — Port: `5005` — GameId: `room-1` — LocalId: `0` → **Start Host**.
- Log esperado:  
  ```
  [ApiServer] TcpListener escuchando en http://0.0.0.0:5005/ (dual-stack)
  ```
- Comprobación opcional: abre `http://127.0.0.1:5005/server/room-1/0` y verifica JSON.

### Cliente (Build)
- IP: `127.0.0.1` — Port: `5005` — GameId: `room-1` — LocalId: `1` (o `2`) → **Start Client**.

**Resultado:** cada instancia controla su cápsula; todas ven a las demás moverse con suavizado.

---

## Controles por defecto
- **WASD / Flechas**: moverse.
- **Espacio**: salto/impulso (si está implementado en `LocalInputMover`).
- **Stop**: cierra conexión y vuelve a `DisconnectedPanel`.

---

## Ajustes recomendados
- `SyncLoop.interval`: 0.1–0.15 s para más “vivo”; >0.25 s para menos tráfico.
- `GameManager` → `sendInterval` y `minMove`: envíos solo si hubo movimiento.
- `ApiServer.port`: cambia si 5005 está ocupado.
- `MultiplayerUI.LocalId`: reasigna control sin recargar escena.

---

## Solución de problemas
- **No conecta**
  - Permite la app en el firewall o desactívalo para probar.
  - Asegura que el host muestre el log de *dual‑stack*.
  - En LAN usa la IP privada del host (no `127.0.0.1`).

- **Ambas instancias mueven la misma cápsula**
  - Usa **LocalId** distinto en cada ventana (0,1,2).
  - Verifica que `players[]` esté en orden y que `playerId` coincida.

- **Mi jugador no se mueve**
  - Comprueba que `LocalInputMover` se active **solo** en el local.
  - Revisa bindings de entrada si los personalizaste.

- **Lag/teleport**
  - Reduce `SyncLoop.interval`.
  - Envía solo al superar un umbral de movimiento (`minMove > 0`).

---

