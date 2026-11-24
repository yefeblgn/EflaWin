# Victoria del Efla

**Diller / Languages:** [TR](README.md) · [EN](README.en.md) · [ES](README.es.md) · [RU](README.ru.md) · [DE](README.de.md) · [中文](README.zh.md) · [日本語](README.ja.md)

Efla Win recopila descargas de aplicaciones, control de controladores y configuraciones del sistema utilizadas con frecuencia en una sola ventana al configurar una nueva computadora con Windows. Desarrollado con WinUI 3.

<img src="Assets/efla.png" alt="Efla Win logosu" width="72" height="72">

## Comienzo

Abra el archivo `Efla Win 1.0.0.exe` en Windows 11 de 64 bits y otorgue la aprobación del administrador. No se requiere paquete de instalación. Los componentes .NET y Windows App SDK se incluyen en el EXE y se abren en la memoria caché local en la primera ejecución. Las descargas y el escaneo de Windows Update requieren una conexión a Internet.

### Descargar e instalar aplicaciones

1. Comience con **Recomendado** o seleccione la categoría en la pantalla **Todas las aplicaciones**. **Ctrl+F** enfoca el cuadro de búsqueda.
2. Para una sola aplicación, use el botón **Descargar**. Para poner en cola algunos, marque las casillas junto al logotipo y presione el botón **Descargar seleccionados**.
3. De forma predeterminada, los archivos descargados se escriben en el directorio `Efla Win` en la carpeta Descargas de Windows. Puede seleccionar otra carpeta con **Cambiar** en la barra inferior y ver los archivos con **Abrir carpeta**.
4. Una vez que se completa la descarga, el botón **Instalar** abre el instalador. Cuando Windows informa que la aplicación está instalada, aparece el botón **Desinstalar** y, después de su confirmación, se abre la herramienta de eliminación de la aplicación.

La opción **Código abierto** en el menú contextual de una tarjeta abre la página de distribución en el catálogo.

### Seguimiento de descargas y navegación

**Acciones disponibles** en el menú de la izquierda muestra la cola de descarga y el análisis del controlador incluso si cambia de página. Aparece la cantidad de datos recibidos durante la descarga. Si el servidor informa el tamaño total, también se mostrará. El botón ****** al lado de un proceso detiene ese trabajo.

Cuando la ventana se minimiza, pasa al área de notificación. Puede abrir la ventana, la carpeta de descarga o la pantalla de memoria desde el menú contextual del icono de Efla Win. **Salir** cierra la aplicación.

### editar ventanas

| Pantalla | ¿Qué puedes hacer? |
| --- | --- |
| **Configuración de Windows** | Verifique las opciones de búsqueda, barra de tareas, privacidad, rendimiento y Explorador de archivos. Vaya a las páginas de aplicaciones de inicio, almacenamiento, red y proxy. |
| **Configuración del juego** | Consulta las opciones del modo de juego, la grabación en segundo plano y la barra de juegos de Xbox. Abra las preferencias de gráficos y el plan de energía. |
| **Hardware** | Vea su procesador, tarjeta gráfica, versión de Windows, uso de RAM y espacio libre en disco. |
| **Conductores** | Destaca las herramientas Intel, AMD y NVIDIA según el dispositivo. Busque los controladores que se ofrecen en Windows Update y abra la pantalla **Actualizaciones opcionales** para la instalación. |
| **Memoria** | Ver el uso actual de RAM. **Purgar** para reducir los conjuntos de trabajo de aplicaciones elegibles en esta sesión. |
| **Configuración de la aplicación** | El sistema ajusta el tema claro u oscuro, el color de resaltado, el tamaño de fuente, el texto en negrita, el contraste y la reducción de movimiento. |

Si una configuración dice **Bloqueado** junto a ella, el botón de información muestra la política y la ruta de guardado que aplica Windows. **No disponible** indica que la opción no es compatible con esta versión de Windows o computadora. Algunas configuraciones solo se reflejan cuando inicias sesión nuevamente.

## compilar desde la fuente

El SDK de .NET 10 y el SDK de Windows deben estar instalados en Windows. Abra el archivo fuente y ejecute PowerShell en la carpeta que contiene el archivo `EflaWin.csproj`.

```powershell
dotnet publish .\EflaWin.csproj -c Release -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

La salida de transmisión está en la ruta `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\EflaWin.exe`. Este EXE es suficiente para la distribución.

## Comentario

Al informar un error, incluya la versión de Windows, la pantalla donde ocurrió el error y los pasos a repetir. Si se trata de un problema de descarga, incluya también el nombre de la aplicación y el texto de error que aparece en la tarjeta. [GitHub](https://github.com/yefeblgn/eflawin)

## Recursos y licencias

Mientras se preparaban las opciones de Windows, se examinaron [Optimizer](https://github.com/hellzerg/optimizer), [Sparkle](https://github.com/thedogecraft/sparkle), [ReviOS Playbook](https://github.com/meetrevision/playbook), [GTweak](https://github.com/Greedeks/GTweak) y [vacpp/windows11](https://github.com/vacpp/windows11). [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) se tomó como ejemplo en la interfaz del menú del área de notificación.

© 2026 Estudios BLGN. Los términos de uso se encuentran en el archivo [LICENSE](LICENSE). Las marcas y aplicaciones del catálogo pertenecen a sus respectivos dueños.

hecho por ❤️[yefeblgn](https://yefeblgn.net)
Desarrollado por BLGN Studios
