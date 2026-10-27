"""Regenera el PDF versionado. Solo para autoría: python -m pip install reportlab.

Ejecutar desde cualquier directorio; las pruebas nunca regeneran el corpus.
"""
from pathlib import Path
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak

DESTINO = Path(__file__).resolve().parents[2] / "UsmpConnect/Data/Conocimiento/manual-fia-usmp-connect.pdf"
estilos = getSampleStyleSheet()
estilos.add(ParagraphStyle("TituloFia", parent=estilos["Heading1"], fontSize=20,
                          leading=25, textColor=colors.HexColor("#7a152d"), spaceAfter=18))
estilos.add(ParagraphStyle("TextoFia", parent=estilos["BodyText"], fontSize=11,
                          leading=17, spaceAfter=12))

paginas = [
    ("Calculadora de Promedios y organización", [
        ("1. Calculadora FIA y Mis Notas",
         "El acceso Calculadora FIA en los portales del sitio abre la web externa calculadorafiausmp.com. "
         "No es una calculadora integrada en Inicio. Revisa las ponderaciones del sílabo de tu curso antes de calcular."),
        ("Notas de demostración",
         "Mis Notas permite registrar PP, EP y EF, y estimar el promedio con los pesos de demostración "
         "30%, 30% y 40%. Si falta el EF, estima la nota necesaria para llegar a 10.5. "
         "Es una referencia académica de la maqueta, no una constancia oficial ni una regla para todos los cursos."),
        ("2. Mi Horario y Mi Calendario",
         "Mi Horario muestra los bloques de los cursos matriculados del alumno de demostración. "
         "Mi Calendario reúne eventos académicos y permite añadir recordatorios propios al iniciar sesión."),
        ("3. Abrir el Asistente FIA",
         "Primero inicia sesión como alumno. En Inicio y los módulos con el menú común aparece el botón "
         "flotante Asistente FIA abajo a la derecha. Haz clic para abrirlo. No aparece en la pantalla de login "
         "ni al navegar sin sesión. Permite consultar contenido de la plataforma y muestra sus fuentes; "
         "no realiza modificaciones en tu nombre.")
    ]),
    ("Restaurantes, reseñas y comunidad", [
        ("4. Crear, editar y eliminar reseñas",
         "Abre el detalle de un restaurante e inicia sesión para publicar una reseña de 1 a 5 estrellas. "
         "Los botones Editar y Eliminar aparecen junto a tus propias reseñas. Antes de eliminar, confirma la acción. "
         "La autoría se valida también en el servidor."),
        ("Sentimiento y recomendaciones",
         "Al crear o editar una reseña, ML.NET clasifica el comentario como Positivo o Negativo con la "
         "probabilidad de la clase predicha. Se recalcula el promedio del restaurante y se invalida la caché del catálogo. "
         "Estas acciones no reentrenan el modelo de recomendaciones: utiliza el dataset de demostración. "
         "Una reseña nueva no cambia automáticamente las predicciones personalizadas del modelo."),
        ("5. Catálogo y caché",
         "El indicador de la cabecera registra el origen del catálogo al cargar la página: Redis Cache, "
         "SQLite o Caché local. Abrir un detalle consulta SQLite y no cambia el origen registrado del catálogo. "
         "Para observar la reconstrucción tras invalidar la caché, recarga el catálogo. Una carga posterior puede "
         "mostrar Redis si otra consulta o la precarga ya lo reconstruyó."),
        ("6. Convivencia y fuentes",
         "No compartas contraseñas ni datos privados. Las publicaciones pertenecen a sus estudiantes autores. "
         "Este manual describe una maqueta universitaria de USMP Connect, no un reglamento oficial de la facultad. "
         "El asistente puede recuperar sus páginas mediante PdfPig y Qdrant, con respaldo de búsqueda local.")
    ])
]

def pie(canvas, doc):
    canvas.setFont("Helvetica", 9)
    canvas.setFillColor(colors.HexColor("#626571"))
    canvas.drawString(44, 30, "USMP Connect | Manual de demostración | 27/10/2026")
    canvas.drawRightString(A4[0] - 44, 30, f"Página {doc.page}")

if __name__ == "__main__":
    contenido = []
    for numero, (titulo, secciones) in enumerate(paginas):
        if numero:
            contenido.append(PageBreak())
        contenido.append(Paragraph("Manual de USMP Connect", estilos["TituloFia"]))
        contenido.append(Paragraph(titulo, estilos["Heading2"]))
        contenido.append(Spacer(1, 8))
        for subtitulo, texto in secciones:
            contenido.append(Paragraph(subtitulo, estilos["Heading3"]))
            contenido.append(Paragraph(texto, estilos["TextoFia"]))
    SimpleDocTemplate(str(DESTINO), pagesize=A4, leftMargin=44, rightMargin=44,
                      topMargin=42, bottomMargin=48, title="Manual de USMP Connect",
                      author="Equipo USMP Connect").build(contenido, onFirstPage=pie, onLaterPages=pie)
    print(DESTINO)
