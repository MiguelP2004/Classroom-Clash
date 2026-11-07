# Classroom Clash

Proyecto de la asignatura Fundamentos de Videojuegos (Grupo 11) - ETSIINF, UPM.

## De qué va el juego

Es un tower defense con acción. En un futuro no muy lejano, una IA ha tomado el control de la educación y ha subido la dificultad a niveles imposibles: las asignaturas se han vuelto monstruos y los exámenes salen en masa. Solo queda un profesor dispuesto a plantar cara y salvar la carrera de sus alumnos.

Tú eres ese profesor y tienes que defender el aula de las oleadas de exámenes. Para ello combinas dos cosas:

- Tu puntería: el profesor apunta con el ratón y dispara lápices a los exámenes.
- Tus alumnos: los colocas por el aula y funcionan como torres, disparando solos a los exámenes que tienen cerca.

Al eliminar exámenes consigues energía, y tú decides en qué gastarla: más alumnos para defender o mejoras para el profesor. Si llegan demasiados exámenes al final del camino, pierdes.

Cada nivel termina con un jefe final, una asignatura, con mucha vida y ataques especiales. Al superarla se desbloquea el siguiente nivel y puedes conseguir mejoras en la tienda.

La idea final del juego es tener menú principal, tutorial, selección de niveles (una asignatura por nivel), partidas por oleadas, pantalla de victoria o derrota y una tienda de mejoras entre niveles.

## Controles

| Acción | Cómo |
|---|---|
| Mover la cámara | Llevar el ratón al borde izquierdo o derecho de la pantalla |
| Apuntar | Ratón |
| Disparar | Clic izquierdo (se puede mantener) |
| Poner alumno | Botón de alumno y clic en una zona libre (no vale encima del camino ni de otro alumno) |
| Quitar alumno | Botón de la papelera y clic en el alumno |
| Pausa | Botón de pausa (ahí también se cambia el volumen) |

Al empezar la partida hay un tutorial que explica todo esto paso a paso.

## Cómo está organizado

- `Assets/Codigo` → los scripts en C#
- `Assets/Scenes` → la escena del juego
- `Assets/Dibujos` → los sprites
- `Assets/Sonidos` → los sonidos
- Los prefabs (Alumno, Basico, Pesado, Lapiz...) están en la raíz de `Assets`

El script principal es `ControladorJuego.cs`, que lleva la energía, las vidas, la pausa y el Game Over.

## Estado actual

Estamos en la iteración 2 (core technical demo). Ya funciona:

- Profesor que apunta y dispara, con mejora de disparo doble
- Colocar y quitar alumnos
- Oleadas de exámenes básicos y pesados
- Energía, vidas, pausa y Game Over
- Tutorial

Números de la demo: empiezas con 50 de energía, un examen básico da 2 y uno pesado 4, un alumno cuesta 20, la mejora del profesor 70, y tienes 12 vidas.

Para las siguientes iteraciones quedan los jefes (las asignaturas), el menú principal, la selección de nivel, la pantalla de victoria y la tienda.

## Herramientas

- Unity 2D y C#
- Git / GitHub
- Microsoft Planner para el Kanban
- Teams y Discord para hablar entre nosotros

## Equipo

- Miguel Pereira Ortiz
- Daniel Valero Jiménez
- Mario Homedes Fernández
- Diego Javier Garabán Gil
- Eduardo Arroyo Lafuente
- Álvaro Bravo Rodríguez

## Cómo puedo jugar

1- Dirígete a la sección `Releases`.<br>
2- Dentro, verás una carpeta llamada `Demo.del.juego.-.Classroom.Clash.zip`.<br>
3- Descárgala y extraela.<br>
4- Al abrirla encontrarás el ejecutable llamado `Classroom Clash - A Tower Defense Game.exe`.<br>
5- Ejecútalo y disfruta.<br>
