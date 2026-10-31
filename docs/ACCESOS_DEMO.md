# Accesos de USMP Connect

Decisión del equipo: Google con cuentas personales, acceso local y perfiles demo. Microsoft/Entra se retiró del alcance por petición del usuario.

## Google mediante Firebase

- Se valida el ID token en el servidor: firma, emisor, audiencia, vencimiento, email verificado y proveedor Google. El correo enviado por el navegador no sirve como prueba de identidad.
- Se admiten correos personales; `Registro:DominioCorreo` conserva su uso en el formulario de registro local, no limita Google.
- El acceso externo se identifica mediante proveedor y UID. Una nueva cuenta no recibe contraseña, código universitario ni carrera inventados.
- El rol Alumno permite utilizar la maqueta; Google no verifica matrícula o pertenencia a la USMP.
- Una cuenta existente con el mismo correo no se fusiona automáticamente. Hay que utilizar su método original. Este bloque no incorpora una pantalla de vinculación de proveedores.
- Las cuentas bloqueadas y cuentas docentes sin rol de alumno continúan rechazándose.

En Firebase Authentication, Google debe estar habilitado y el dominio `usmpconnect.onrender.com` debe figurar en Settings / Authorized domains. Para desarrollo autorizar localhost si se necesita. No cambiar el authDomain del código por la dirección de Render.

## Comprobación manual

1. Abrir login sin sesión y seleccionar Google.
2. Elegir una cuenta personal nueva para la maqueta; completar el acceso.
3. Cerrar sesión y repetir: debe recuperar la misma cuenta, sin duplicarla.
4. Si el correo coincide con una cuenta local previa, se muestra conflicto y no se inicia sesión externa ni se modifica esa cuenta.
5. Cancelar el popup debe dejar disponible el botón para volver a intentar.

La verificación del token y los casos de permisos se prueban con dobles y claves de prueba. Una prueba automatizada no se presenta como evidencia de que el usuario ya completó el popup real de Google.
