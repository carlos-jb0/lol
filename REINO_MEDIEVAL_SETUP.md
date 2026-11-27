# Reino medieval — segunda fase

Esta fase amplía el pueblo generado sin reemplazar la escena ni los cuatro interiores existentes.

## Aplicar en Unity
1. Actualiza el repositorio y abre `Assets/Scenes/SampleScene.unity`.
2. Espera a que Unity compile los scripts.
3. Ejecuta **Reino medieval > Expandir reino y configurar cámara**.
4. Guarda una copia de la escena antes de probar si quieres conservar el diseño anterior.

El constructor:
- Amplía el terreno y extiende las dos avenidas.
- Añade una muralla almenada perimetral, torres y puerta fortificada.
- Añade una fortaleza, distrito exterior, puestos de mercado, fuente, pozo y antorchas.
- Configura una cámara orbital en tercera persona con zoom fijo, colisión contra paredes y giro con ratón.
- Sustituye visualmente el mesh anterior por una armadura low-poly original de placas oscuras y acentos carmesí.
- Añade sprint con resistencia usando Shift.

## Controles
- WASD: movimiento relativo a la cámara.
- Ratón: girar la cámara (se captura al iniciar).
- Clic izquierdo: volver a capturar el ratón tras pulsar Escape.
- Escape: liberar el ratón.
- Shift izquierdo: correr mientras haya resistencia.
- Espacio: saltar.
- E: interactuar con puertas, muebles y monedas.

## Importante
El constructor crea geometría procedural de prototipo; no reemplaza modelos artísticos finales ni añade todavía combate, NPCs, misiones o guardado persistente. Los cuatro interiores y el sistema de monedas previos se conservan.
