# Gran expansión RPG — Reino de las Cenizas

Esta actualización añade una capa RPG al reino existente. No sustituye la escena ni los modelos originales: el constructor crea un grupo independiente llamado `RPG_ADVENTURE_EXPANSION` dentro de `PUEBLO_GENERADO`.

## Antes de construir

1. Fusiona el Pull Request de esta expansión y actualiza tu copia local con `git pull --ff-only origin main`.
2. Haz una copia de `Assets/Scenes/SampleScene.unity` desde el Project de Unity antes de generar el mundo.
3. Comprueba que los cuatro modelos OBJ + MTL siguen importados en `Assets/Models/Pueblo/`:
   - `Casa_Pequena`
   - `Casa_Mediana`
   - `Casa_Dos_Pisos`
   - `Casa_Piedra_Porche`
4. Abre la escena del reino y espera a que Unity termine de compilar.

El constructor valida los cuatro modelos **antes** de modificar la escena. Si falta alguno, se detiene sin construir una expansión parcial.

## Construir

En el menú superior de Unity ejecuta:

**RPG > Construir gran expansión RPG**

Después guarda la escena con Ctrl+S y pulsa Play.

El constructor es repetible: al volver a ejecutarlo sustituye solamente `RPG_ADVENTURE_EXPANSION`. No borra `PUEBLO_GENERADO`, `REINO_EXPANSION`, los cuatro interiores originales ni los archivos de modelo.

## Contenido añadido

- **32 casas nuevas** construidas con los cuatro modelos originales. Cada una tiene colisiones exteriores, un interior independiente, muebles, botín y portales de entrada/salida.
- **Comerciante del mercado** con espadas, dagas, katanas, arcos, ballestas, escudos, armaduras y pociones. El equipo más poderoso cuesta muchas monedas.
- **Recolección y economía:** hierbas, mineral de hierro, madera, pieles, colmillos y reliquias antiguas. Se pueden vender al comerciante. Los cofres, la exploración y la arena ofrecen otras recompensas.
- **Personaje sin equipo inicial:** empieza sin arma, escudo ni armadura. El equipo se desbloquea comprándolo, encontrándolo o como recompensa de misión.
- **Sistema RPG:** salud, armadura, experiencia, niveles, inventario y guardado local mediante PlayerPrefs.
- **Combate:** ataque con clic izquierdo, bloqueo con clic derecho cuando se tiene escudo, esquiva con Q y selección de armas con las teclas 1 a 7.
- **Castillo explorable** con un capitán y una misión principal.
- **Arena de bestias** con tres oleadas, IA sencilla, HP para enemigos y recompensas.
- **Mejoras visuales:** materiales con texturas procedurales de estilo low-poly, luz ambiental, sol, sombras suaves, antorchas e iluminación interior.

## Controles

| Acción | Control |
|---|---|
| Moverse | WASD / flechas |
| Girar cámara | Ratón |
| Saltar | Espacio |
| Correr | Shift |
| Interactuar / recoger / entrar | E |
| Atacar | Clic izquierdo |
| Bloquear con escudo | Mantener clic derecho |
| Esquivar | Q |
| Equipar espada / daga / katana | 1 / 2 / 3 |
| Equipar arco / ballesta | 4 / 5 |
| Equipar arma legendaria / arco ancestral | 6 / 7 |
| Abrir inventario | I |
| Liberar ratón | Escape |

Las teclas de equipo solo funcionan cuando el arma correspondiente ya está desbloqueada.

## Progresión de la primera misión

1. Entra al castillo y habla con el capitán.
2. Explora el reino y consigue 3 reliquias antiguas.
3. Entra a la arena y supera sus tres oleadas.
4. Regresa con la recompensa de la misión: monedas, experiencia, una espada y armadura de la Guardia Real.

## Guardado

El saldo, los recursos, la salud, el nivel, la experiencia, el equipo y el avance de la misión se guardan localmente en PlayerPrefs. Para probar una partida completamente nueva, borra las claves de guardado de la aplicación desde el menú de PlayerPrefs o usa un perfil de juego limpio. No ejecutes un reset si quieres conservar tu progreso.

## Nota de pruebas

El repositorio se actualiza por código y constructor de Unity. La compilación real, el aspecto final de los OBJ importados y la navegación dentro de la escena deben comprobarse en tu editor y en Play Mode; el Pull Request no afirma que se haya ejecutado Unity aquí.
