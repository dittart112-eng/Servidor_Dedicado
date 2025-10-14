Esto es un proyecto taller para aprender a conectar jugadores con Player Host, A continuacion explico el paso a paso: 

Player Host (Anfitrión Híbrido)
El modo Player Host se activa cuando un jugador inicia el juego con la opción "Start As Host" (MultiplayerUI.StartAsHost()). Esta instancia asume dos roles
Arranca el motor de la API en el puerto local (127.0.0.1:5005). Almacena la "verdad" del juego (RoomState, PlayerState).ApiClient y SyncLoop Se conecta inmediatamente a su propia dirección de servidor (127.0.0.1) e inicia el ciclo de juego.
MultiplayerUI.StartAsHost() Llama a apiServer.StartServer(). Esto activa la autoridad de estado en la máquina local. 
MultiplayerUI / ApiClient : El ApiClient se configura para usar la dirección de loopback: http://127.0.0.1:5005/server.
SyncLoop.StartSync() : El cliente inicia su ciclo de red, que ahora apunta al servidor que acaba de arrancar.
GameManager.RebindLocalControl(0) : Se le otorga el control de movimiento local al jugador Host (generalmente con playerId = 0).

Funcionamiento Lógico:
PUSH : Host Servidor (Local) SyncLoop llama a gameManager.SendPlayerPosition(0). El ApiClient envía la posición del Host al ApiServer local.
PULL : Servidor Host (Local SyncLoop pide los datos de todos los demás jugadores. El ApiClient hace GET al ApiServer local para obtener las posiciones de todos los jugadores (incluida la suya, y las de los clientes remotos).
Aplicación : Host  GameScene : GameManager aplica las posiciones recibidas a todos los PlayerController mediante Vector3.Lerp (solo para los remotos).

Resumen de los Códigos :  
PlayerController.cs : Contiene el flag isLocal (para autoridad) y el targetPosition para la suavización (Vector3.Lerp).
ServerData.cs : Clase simple con [Serializable] que define el formato JSON para la posición (posX, posY, posZ) y el estado (paused).
ApiServer.cs : Contiene la lógica del servidor de red (manejo de sockets y multithreading), almacena y recupera el estado del juego. Librería clave: System.Net.Sockets.
ApiClient.cs : Construye y envía peticiones HTTP (POST/GET) al ApiServer (local o remoto). 
SyncLoop.cs : Utiliza una Coroutine (IEnumerator Loop()) para forzar las peticiones ApiClient a una frecuencia fija (tickRate). Librería clave: System.Collections.
MultiplayerUI.cs : Proporciona los métodos StartAsHost() y StartAsClient() enlazados a botones para configurar y activar los roles.

Librerias Implementadas :
1:System.Net y System.Threading: Utilizadas por ApiServer.cs para crear un servidor web ligero y responder a múltiples peticiones de clientes simultáneamente (multihilo).
2:System.Collections.Generic y System.Collections: Esenciales en SyncLoop.cs para gestionar el bucle periódico (IEnumerator) y listas de jugadores.
3:System.Linq: Agregada para permitir la manipulación de colecciones (ej. .ToList()) de forma segura en MultiplayerUI.cs al desconectar.
4:System (con [Serializable]): Utilizada en ServerData.cs para marcar la clase como serializable a formato JSON.
