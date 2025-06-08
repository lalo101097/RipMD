# RipMD

**RipMD** es una herramienta basada en **Selenium** diseñada para automatizar la descarga de capítulos desde el sitio Zonatmo.

Nota: requiere tener instalado Google Chrome.

## Funcionalidades

### 📥 Descarga por capítulo

Introduce la URL de un capítulo individual para descargarlo directamente.

---

### 📚 Descarga por manga (completo o por rango)

Proporciona la URL del manga. Según lo que completes:

- **Sin rango:** descarga **todo el manga completo**.
- **Solo inicio:** descarga desde ese capítulo hasta el final.
- **Solo final:** descarga desde el inicio hasta ese capítulo.
- **Inicio y final:** descarga solo los capítulos dentro del rango especificado.

---

### 📋 Descarga en cola

Permite agregar múltiples URLs (mezcladas de mangas o capítulos) en una sola operación.  
El sistema identifica automáticamente si es capítulo o manga según el formato de la URL.

Para los mangas en cola, puedes definir rangos de dos formas:

- `url,<inicio>,<final>`
- `url <inicio>-<final>`

Esto facilita la descarga masiva de múltiples mangas o capítulos con mínima intervención.
