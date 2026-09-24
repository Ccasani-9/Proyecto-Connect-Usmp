# USMP Connect

Plataforma comunitaria para alumnos y docentes de la USMP (Programación 1).

## Flujo de ramas

| Rama | Uso |
|---|---|
| `main` | Producción. Lo que está aquí se despliega en Render. |
| `develop` | Integración. Aquí se juntan las ramas de cada integrante y se prueba. |
| `Ccasani`, `Maximo`, `Andrew` | Rama de trabajo de cada integrante. |

1. Cada integrante trabaja en su rama y abre un **Pull Request hacia `develop`**.
2. Cuando `develop` está probado, se abre un **Pull Request `develop` → `main`**.
3. Render despliega automáticamente desde `main`.
