
    const apiModelSecurity = 'http://localhost:5081';

    document.addEventListener("DOMContentLoaded", () => {
        const loginButton = document.querySelector(".login-button");

        loginButton.addEventListener("click", async (e) => {
            e.preventDefault();

            const userName = document.getElementById("username").value.trim();
            const password = document.getElementById("password").value.trim();

            if (!userName || !password) {
                alert("Por favor ingresa usuario y contraseña");
                return;
            }

            try {
                const response = await fetch(`${apiModelSecurity}/api/auth/login`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        userName: userName,  // 👈 Debe coincidir con LoginRequestDto
                        password: password   // 👈 Igual al backend
                    })
                });

                if (!response.ok) {
                    throw new Error("Error en login: " + response.status);
                }

                const data = await response.json();

                // Guardamos el token
                localStorage.setItem("token", data.token);

               

                // Redirigir al dashboard (ajusta la ruta)
                window.location.href = "index.html";

            } catch (error) {
                console.error("Error:", error);
                alert("❌ No se pudo iniciar sesión");
            }
        });
    });

