import './App.css';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';

function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();
    async function handleLogin() {
        const loginData = {
            UserMail: email,
            UserPassword: password
        };

        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/Login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(loginData)
            });

            if (response.ok) {
                const result = await response.json();

                localStorage.setItem('token', result.token);

                const decodedToken = jwtDecode(result.token);

                const userRoles = decodedToken.role || decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || [];

                const rolesArray = Array.isArray(userRoles) ? userRoles : [userRoles];

                localStorage.setItem('roles', JSON.stringify(rolesArray));

                if (rolesArray.includes('admin')) {
                    navigate('/admin');
                } else if (rolesArray.includes('guard')) {
                    navigate('/guard');
                } else {
                    navigate('/robots');
                }

                window.location.reload();

            } else {
                alert("Помилка входу: перевірте пошту чи пароль");
            }
        } catch {
            alert("Не вдалося з'єднатися з сервером");
        }
    }

    return (
        <div className="main-content">
            <div className="form-container">
                <h2>Вхід</h2>
                <input
                    type="email"
                    placeholder="Ваша пошта"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                />
                <input
                    type="password"
                    placeholder="Пароль"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
                <button className="primary-btn" onClick={handleLogin}>
                    Увійти
                </button>
            </div>
        </div>
    );
}

export default Login;
