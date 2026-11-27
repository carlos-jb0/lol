# Pueblo explorable (Unity 6.3)

## Preparar los cuatro modelos
1. Descomprime `Casas_LowPoly_Unity_4_Modelos.zip`.
2. Copia los cuatro pares `.obj + .mtl` a `Assets/Models/Pueblo/` dentro de este proyecto:
   - Casa_Pequena
   - Casa_Mediana
   - Casa_Dos_Pisos
   - Casa_Piedra_Porche
3. Deja que Unity termine de importarlos.

## Construir el pueblo
1. Abre `Assets/Scenes/SampleScene.unity`.
2. En Unity selecciona **Pueblo > Construir pueblo explorable**.
3. El editor conservará el objeto `Player` existente, añadirá el sistema de interacción y guardará la escena.
4. Pulsa Play.

## Controles
- WASD / flechas: moverse (el controlador existente).
- Espacio: saltar.
- E: entrar/salir de casas, revisar muebles y recoger monedas.

## Qué se genera
- Pueblo con caminos, plaza, vegetación y cuatro casas.
- Interior básico independiente por casa, con cama, cómoda, mesa, baúl y lámpara.
- Los muebles se pueden revisar una vez; las monedas aparecen y se recogen individualmente con E.
- El contador de monedas aparece en pantalla.

El constructor es repetible: si vuelves a ejecutarlo, reemplaza únicamente el grupo PUEBLO_GENERADO. No elimina el resto de la escena.
